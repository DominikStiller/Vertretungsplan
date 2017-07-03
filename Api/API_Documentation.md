# Vertretungsplan API Documentation
This is the documentation for the REST API of the Vertretungsplan API server.

Every response body JSON-formatted and UTF-8-encoded.

The implementation can be found in [VertretungsplanController.cs](Controllers/VertretungsplanController.cs)

## Operations
These are the available operations:

### List dates
List the dates including metadata.

* **URL:** `/dates?metadata`

* **Method:** `GET`

* **Optional URL Parameters**

   * `hidepast`: only show today and future dates (Sample URL: `/dates?metadata&hidepast`)


* **Sample response:**

   * No dates available: `[]`

   * 2 dates available:

```json
[
   {
      "date":"2017-04-28T00:00:00",
      "version":3,
      "lastUpdated":"2017-04-28T07:16:49"
   },
   {
      "date":"2017-05-02T00:00:00",
      "version":2,
      "lastUpdated":"2017-04-28T07:17:20"
   }
]
```

### Show data (all dates)
Show the data for all dates.

* **URL:** `/dates`

* **Method:** `GET`

* **Optional URL Parameters**

   * `hidepast`: only show today and future dates (Sample URL: `/dates?hidepast`)


* **Sample response:**

   * No dates available: `[]`

   * 2 dates available:

```json
[
   {
      "date":"2017-04-28T00:00:00",
      "version":3,
      "lastUpdated":"2017-04-28T07:16:49",
      "studentNotes":"Schüleranmerkungen",
      "teacherNotes":"Lehreranmerkungen",
      "absentForms":"5a (Ab), 6b (Cd)",
      "absentTeachers":"Frau Mustermann, Herr Schmidt",
      "missingRooms":"100, 101",
      "entries":[
         {
            "form":"9a",
            "lesson":2,
            "originalTeacher":"Herr Weber",
            "originalSubject":"D",
            "substitutionTeacher":"Herr Maier",
            "substitutionSubject":"Aufs.",
            "substitutionRoom":"303",
            "note":"Raumänderung"
         }
      ]
   },
   {
      "date":"2017-05-02T00:00:00",
      "version":3,
      "lastUpdated":"2017-04-28T07:17:20",
      "studentNotes":"Schüleranmerkungen",
      "teacherNotes":"Lehreranmerkungen",
      "absentForms":"7c (Ef), 8d (Gh)",
      "absentTeachers":"Frau Schmidt, Herr Mustermann",
      "missingRooms":"200, 201",
      "entries":[
         {
            "form":"10b",
            "lesson":4,
            "originalTeacher":"Frau Schneider",
            "originalSubject":"M",
            "substitutionTeacher":"entfällt",
            "substitutionSubject":"—",
            "substitutionRoom":"—",
            "note":"verlegt auf  6. Std."
         }
      ]
   }
]
```

### Show data (single date)
Show the data for a specific date.

* **URL:** `/dates/:date`

* **Method:** `GET`

* **Parameter:**  `date=[string:yyyy-MM-dd]` (Sample URL for April 28, 2017: `/dates/2017-04-28`)

* **Sample response:**

   * Date does not exist: status code `404`, empty response body

   * Date exists:

```json
{
   "date":"2017-04-28T00:00:00",
   "version":3,
   "lastUpdated":"2017-04-28T07:16:49",
   "studentNotes":"Schüleranmerkungen",
   "teacherNotes":"Lehreranmerkungen",
   "absentForms":"5a (Ab), 6b (Cd)",
   "absentTeachers":"Frau Mustermann, Herr Schmidt",
   "missingRooms":"100, 101",
   "entries":[
      {
         "form":"9a",
         "lesson":2,
         "originalTeacher":"Herr Weber",
         "originalSubject":"D",
         "substitutionTeacher":"Herr Maier",
         "substitutionSubject":"Aufs.",
         "substitutionRoom":"303",
         "note":"Raumänderung"
      },
      {
         "form":"10b",
         "lesson":4,
         "originalTeacher":"Frau Schneider",
         "originalSubject":"M",
         "substitutionTeacher":"entfällt",
         "substitutionSubject":"—",
         "substitutionRoom":"—",
         "note":"verlegt auf  6. Std."
      }
   ]
}
```

### Update data
Update of data from Amazon S3. Authentication required.

* **URL:** `/dates`

* **Method:** `POST`

* **Request Headers**

   * `Authorization`:
   [Basic authentication](https://en.wikipedia.org/wiki/Basic_access_authentication#Client_side) with username `update` is used for authentication.
   The password is set in `appsettings.json`.
   (Sample value: `Basic dXBkYXRlOm15cGFzc3dvcmQ=` uses password `mypassword`)


* **Sample response:**

   Response body is always empty.

   * Update successfully triggered: status code `200`

   * Not authorized: status code `401`
