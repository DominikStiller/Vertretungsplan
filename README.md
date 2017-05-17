# Vertretungsplan
This is the code for displaying teacher substitution schedules created with [Time Substitute 2007](http://fsware.de/) online.

It was created for the [Gymnasium Unterrieden Sindelfingen](http://www.gymnasium-unterrieden.de/), but can be used by any school using the aforementioned software.

## Components

### Uploader
Uploader is a simple GUI application for Windows which can be run in the background.
When exporting, Time Substitute writes the substitution schedule to TS-Internet.mdb (a MS Access database file).
Uploader watches this file in a selected location and uploads it into an Amazon S3 bucket when changes are detected.

### Converter
Converter is intended to run as a function in AWS Lambda, triggered by the upload of the database file to S3.
It converts the data from the database into JSON, formatting data and removing unnecessary information in the process, and uploads the converted file to S3.
Converter then notifies Api that a new version is available.

### Api
Api is an API server intended to run in an Amazon EC2 instance (t2.nano or t2.micro) and be deployed using AWS Elastic Beanstalk.
The JSON file is loaded from S3 at startup and upon invocation of the `Update data` operation.
The data is then accessible to Web and other clients (such as a mobile applications).

The API documentation can be found in the Api directory.

### Web
Web is the server for the website intended to run in an Amazon EC2 instance (t2.micro) and be deployed using AWS Elastic Beanstalk.
It periodically checks with Api for new or updated data.
After loading, the data is then served as a responsive website.
There is a student and a teacher version, displaying the same data but with different filtering, formatting and arrangement.
