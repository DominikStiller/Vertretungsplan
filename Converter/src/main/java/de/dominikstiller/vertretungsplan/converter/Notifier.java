package de.dominikstiller.vertretungsplan.converter;

import java.io.IOException;
import java.net.HttpURLConnection;
import java.net.URL;

public class Notifier {

   // Notify the Api so it can load the converted json from S3
   static void notifyApi(String host, String authInfo) throws IOException {
      HttpURLConnection httpConnection = (HttpURLConnection) new URL("http://" + host + "/dates").openConnection();
      httpConnection.setRequestMethod("POST");
      httpConnection.setRequestProperty("Authorization", "Basic " + authInfo);
      httpConnection.setDoOutput(true);
      // Necessary to send request
      httpConnection.getOutputStream().close();
      httpConnection.getResponseCode();
      httpConnection.disconnect();
   }
}
