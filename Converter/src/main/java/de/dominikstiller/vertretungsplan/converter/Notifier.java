package de.dominikstiller.vertretungsplan.converter;

import java.io.IOException;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.List;

public class Notifier {

   // Notify an endpoint about changed data (e.g. the API server so it can load the converted JSON from S3)
   static void notifyEndpoints(List<String> endpoints, List<String> authInfos) throws IOException {
      for (int i = 0; i < endpoints.size(); i++) {
         String endpoint = endpoints.get(i);
         String authInfo = authInfos.get(i);

         HttpURLConnection httpConnection = (HttpURLConnection) new URL(endpoint).openConnection();
         httpConnection.setRequestMethod("POST");
         httpConnection.setRequestProperty("Authorization", "Basic " + authInfo);
         httpConnection.setDoOutput(true);
         // Necessary to send request
         httpConnection.getOutputStream().close();
         httpConnection.getResponseCode();
         httpConnection.disconnect();
      }
   }
}
