# Vertretungsplan
This is the code for displaying teacher substitution schedules created with [Time Substitute 2007](http://fsware.de/) online.

It was created for the [Gymnasium Unterrieden Sindelfingen](http://www.gymnasium-unterrieden.de/), but can be used by any school using the previously mentioned software.

## Components

### Uploader
Uploader is a simple GUI application for Windows which can be run in the background.
When exporting, Time Substitute creates a TS-Internet.mdb file (MS Access database).
Uploader watches this file in a selected location and uploads it into a Amazon S3 bucket when changes are detected.

### Converter
Converter is intended to run as a function in AWS Lambda, triggered by the upload of the database file to S3.
It converts this file into JSON, formatting data and removing unnecessary information in the process.
Converter then notifies Api that a new version has been uploadedd.

### Api
Api is an API Server intended to run in an Amazon EC2 instance (t2.nano or t2.micro) and be deployed using AWS Elastic Beanstalk.
The data is loaded at startup and upon invocation of the `Update data` operation.
The data is then accessible to Web and other clients (such as a mobile applications).

The API documentation can be found in the Api directory.

### Web
Web is the server for the website intended to run in an Amazon EC2 instance (t2.micro) and be deployed using AWS Elastic Beanstalk.
It periodically checks with Api for new or updated data.
After loading, the data is then served as a responsive HTML website.
There is a student and a teacher version, displaying the same data with different filtering, formatting and arrangement.
