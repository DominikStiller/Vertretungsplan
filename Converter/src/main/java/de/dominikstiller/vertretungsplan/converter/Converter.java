package de.dominikstiller.vertretungsplan.converter;

import com.amazonaws.services.s3.AmazonS3;
import com.amazonaws.services.s3.AmazonS3ClientBuilder;
import com.amazonaws.services.s3.model.S3Object;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;
import com.fasterxml.jackson.databind.node.ObjectNode;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.time.LocalDate;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;
import java.util.logging.Level;
import java.util.logging.Logger;
import java.util.stream.Collectors;
import net.ucanaccess.jdbc.UcanaccessConnection;
import org.apache.commons.dbutils.QueryRunner;
import org.apache.commons.dbutils.handlers.ColumnListHandler;

public class Converter {

   private static Logger logger;
   private static AmazonS3 s3;
   private static Properties config;
   private static UcanaccessConnection datebaseConnection;

   static {
      // Use across instances to reduce cold start time
      logger = Logger.getLogger(Converter.class.getName());
      s3 = AmazonS3ClientBuilder.defaultClient();
      config = new Properties();

      String environment = System.getenv().getOrDefault("VPCONVERTER_ENVIRONMENT", "Development");
      try {
         config.load(Converter.class.getResourceAsStream("/" + environment + ".properties"));
         config.put("Environment", environment);
         config.put("DatabasePath", config.get("TempPath") + "/vp.mdb");
      } catch (IOException ex) {
         logger.log(Level.SEVERE, null, ex);
      }
   }

   public static void main(String[] args) throws SQLException, IOException {
      new Converter().convert();
   }

   public void convert() throws SQLException, IOException {
      downloadDatabase();

      datebaseConnection = (UcanaccessConnection) DriverManager.getConnection("jdbc:ucanaccess://" + config.getProperty("DatabasePath"));

      // Read and process data
      List<Vertretungsplan> vps = getDates().stream()
              .map(this::buildVertretungsplan)
              // buildVertretungsplan returns null when exception occurs
              // Workaround because map can't handle checked exceptions
              .filter(vp -> vp != null)
              .collect(Collectors.toList());
      datebaseConnection.close();

      if (config.getProperty("Environment").equals("Development")) {
         datebaseConnection.getDbIO().close();
         Files.deleteIfExists(Paths.get(config.getProperty("DatabasePath")));
      }

      ObjectMapper mapper = new ObjectMapper();
      mapper.registerModule(new JavaTimeModule());
      mapper.disable(SerializationFeature.WRITE_DATES_AS_TIMESTAMPS);

      // Upload converted file to S3
      s3.putObject(config.getProperty("Output.S3Bucket"), config.getProperty("Output.S3Key"), mapper.writeValueAsString(vps));

      notifyApi();
      notifyFCM();
   }

   // Download database from S3
   private void downloadDatabase() throws IOException {
      S3Object database = s3.getObject(config.getProperty("Database.S3Bucket"), config.getProperty("Database.S3Key"));
      Files.copy((InputStream) database.getObjectContent(), Paths.get(config.getProperty("DatabasePath")), StandardCopyOption.REPLACE_EXISTING);
   }

   // Get all dates that exist
   private List<String> getDates() throws SQLException {
      return new QueryRunner().query(datebaseConnection,
              "SELECT Datumkurz from TStatTextLehrer",
              new ColumnListHandler<>("Datumkurz"));
   }

   private Vertretungsplan buildVertretungsplan(String date) {
      Vertretungsplan vp = new Vertretungsplan();
      try {
         fillMetadata(date, vp);

         new QueryRunner().query(datebaseConnection,
                 "SELECT * FROM TDynTextAula WHERE Datumkurz = ?",
                 rs -> {
                    vp.entries = new ArrayList<>();
                    while (rs.next()) {
                       vp.entries.add(buildEntry(rs));
                    }

                    return null;
                 }, date);
      } catch (SQLException ex) {
         logger.log(Level.SEVERE, null, ex);
         return null;
      }

      return vp;
   }

   private void fillMetadata(String date, Vertretungsplan vp) throws SQLException {
      vp.date = LocalDate.parse(date, DateTimeFormatter.ofPattern("dd.MM.yyyy"));

      // Get data from teacher table
      new QueryRunner().query(datebaseConnection,
              "SELECT * from TStatTextLehrer WHERE Datumkurz = ?",
              rs -> {
                 if (rs.next()) {
                    vp.version = Integer.parseInt(rs.getString("Version").replace("Version ", ""));
                    vp.lastUpdated = rs.getTimestamp("Druckzeit").toLocalDateTime();
                    vp.teacherNotes = rs.getString("BitteBeachten").trim().replace("\r\n", "\n");
                    String absentForms = rs.getString("AbwKlassen");
                    String absentCourses = rs.getString("AbwKurse");
                    // Add comma only if both have content
                    vp.absentForms = absentForms + (absentForms.isEmpty() || absentCourses.isEmpty() ? "" : ", ") + absentCourses;
                    vp.absentTeachers = rs.getString("AbwLehrer");
                    vp.missingRooms = rs.getString("FehlRäume");
                 }

                 return null;
              }, date);

      // Get data from student table
      new QueryRunner().query(datebaseConnection,
              "SELECT BitteBeachten from TStatTextAula WHERE Datumkurz = ?",
              rs -> {
                 if (rs.next()) {
                    vp.studentNotes = rs.getString("BitteBeachten").trim().replace("\r\n", "\n");
                 }

                 return null;
              }, date);
   }

   private Entry buildEntry(ResultSet rs) throws SQLException {
      Entry entry = new Entry();

      entry.form = rs.getString("F1");
      entry.lesson = Integer.parseInt(rs.getString("F2").replace(".", ""));
      String[] originalTeacherSubject = rs.getString("F3").split(" / ");
      entry.originalTeacher = originalTeacherSubject[0];
      entry.originalSubject = originalTeacherSubject[1];
      entry.substitutionTeacher = replaceDot(rs.getString("F4"));
      entry.substitutionSubject = replaceDot(rs.getString("F5"));
      entry.substitutionRoom = replaceDot(rs.getString("F6"));
      entry.note = replaceDot(rs.getString("F7"));

      return entry;
   }

   // Replace dots denoting N/A with an em dash
   private String replaceDot(String string) {
      return string.equals(".") ? "—" : string;
   }

   private void notifyApi() throws IOException {
      HttpURLConnection httpConnection = (HttpURLConnection) new URL("http://" + config.getProperty("Api.Host") + "/dates").openConnection();
      httpConnection.setRequestMethod("POST");
      httpConnection.setRequestProperty("Authorization", "Basic " + config.getProperty("Api.AuthInfo"));
      httpConnection.setDoOutput(true);
      // Necessary to send request
      httpConnection.getOutputStream().close();
      httpConnection.getResponseCode();
      httpConnection.disconnect();
   }

   private void notifyFCM() throws IOException {
      String serverKey = config.getProperty("FCM.ServerKey");
      if (!serverKey.isEmpty()) {
         HttpURLConnection httpConnection = (HttpURLConnection) new URL("https://fcm.googleapis.com/fcm/send").openConnection();
         httpConnection.setRequestMethod(("POST"));
         httpConnection.setRequestProperty("Content-Type", "application/json");
         httpConnection.setRequestProperty("Authorization", "key=" + serverKey);
         httpConnection.setDoInput(true);
         httpConnection.setDoOutput(true);

         ObjectNode root = new ObjectMapper().createObjectNode();
         root.put("to", "/topics/updated");

         try (OutputStream out = httpConnection.getOutputStream();
                 OutputStreamWriter wr = new OutputStreamWriter(out)) {
            wr.write(root.toString());
         }
         
         httpConnection.getResponseCode();
         httpConnection.disconnect();
      }
   }
}
