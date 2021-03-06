package de.dominikstiller.vertretungsplan.converter;

import com.amazonaws.services.s3.AmazonS3;
import com.amazonaws.services.s3.AmazonS3ClientBuilder;
import com.amazonaws.services.s3.model.S3Object;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;
import java.io.IOException;
import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.time.LocalDate;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.Arrays;
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
   private static ObjectMapper jsonMapper;

   static {
      // Use across instances to reduce cold start time
      logger = Logger.getLogger(Converter.class.getName());
      s3 = AmazonS3ClientBuilder.defaultClient();
      config = new Properties();

      jsonMapper = new ObjectMapper();
      jsonMapper.registerModule(new JavaTimeModule());
      jsonMapper.disable(SerializationFeature.WRITE_DATES_AS_TIMESTAMPS);

      String environment = System.getenv().getOrDefault("VPCONVERTER_ENVIRONMENT", "Development");
      try {
         config.load(Converter.class.getResourceAsStream("/" + environment + ".properties"));
         config.put("Environment", environment);
         config.put("Database.LocalPath", config.get("TempPath") + "/vp.mdb");
      } catch (Exception e) {
         logger.log(Level.SEVERE, "ERROR while loading configuration", e);
         System.exit(0);
      }
   }

   public static void main(String[] args) throws SQLException, IOException {
      new Converter().convert();
   }

   public void convert() throws SQLException, IOException {
      downloadDatabase();

      datebaseConnection = (UcanaccessConnection) DriverManager.getConnection("jdbc:ucanaccess://" + config.getProperty("Database.LocalPath"));

      // Read and process data
      List<Vertretungsplan> vps = getDates().stream()
              .map(this::buildVertretungsplan)
              // buildVertretungsplan returns null when exception occurs
              // Workaround because map can not handle checked exceptions
              .filter(vp -> vp != null)
              .collect(Collectors.toList());
      datebaseConnection.close();

      if (config.getProperty("Environment").equals("Development")) {
         datebaseConnection.getDbIO().close();
         Files.deleteIfExists(Paths.get(config.getProperty("Database.LocalPath")));
      }

      // Upload converted file to S3
      s3.putObject(config.getProperty("Output.S3Bucket"), config.getProperty("Output.S3Key"), jsonMapper.writeValueAsString(vps));

      Notifier.notifyEndpoints(
              Arrays.stream(config.getProperty("Notification.Endpoints").split(",")).map(url -> url.trim()).collect(Collectors.toList()),
              Arrays.stream(config.getProperty("Notification.AuthInfos").split(",")).map(url -> url.trim()).collect(Collectors.toList()));
   }

   // Download database from S3
   private void downloadDatabase() throws IOException {
      S3Object database = s3.getObject(config.getProperty("Database.S3Bucket"), config.getProperty("Database.S3Key"));
      Files.copy(database.getObjectContent(), Paths.get(config.getProperty("Database.LocalPath")), StandardCopyOption.REPLACE_EXISTING);
   }

   // Get all dates that exist
   private List<String> getDates() throws SQLException {
      return new QueryRunner().query(datebaseConnection,
              "SELECT Datumkurz from TStatTextLehrer",
              new ColumnListHandler<>("Datumkurz"));
   }

   private Vertretungsplan buildVertretungsplan(String date) {
      try {
         Vertretungsplan vp = new Vertretungsplan();

         fillMetadata(date, vp);
         vp.entries = new QueryRunner().query(datebaseConnection,
                 "SELECT * FROM TDynTextAula WHERE Datumkurz = ?",
                 rs -> {
                    ArrayList<Entry> entries = new ArrayList<>();
                    while (rs.next()) {
                       entries.add(buildEntry(rs));
                    }

                    return entries;
                 }, date);

         return vp;
      } catch (Exception e) {
         logger.log(Level.SEVERE, "ERROR while building vertretungsplan", e);
         return null;
      }
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
      entry.originalSubject = originalTeacherSubject.length > 1 ? originalTeacherSubject[1] : "—";
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
}
