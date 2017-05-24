# Vertretungsplan
This is the code for displaying teacher substitution schedules created with [Time Substitute 2007](http://fsware.de/) online.

It was created for the [Gymnasium Unterrieden Sindelfingen](http://www.gymnasium-unterrieden.de/), but can be used by any school using the aforementioned software.


## Components
The system comprises four interacting parts:

### Uploader
Uploader is a simple GUI application for Windows which can be run in the background.
When exporting, Time Substitute writes the substitution schedule to TS-Internet.mdb (a MS Access database file).
Uploader watches this file in a selected location and uploads it into an Amazon S3 bucket when changes are detected.

### Converter
Converter is intended to run as a function on AWS Lambda, triggered by the upload of the database file to S3.
It converts the data from the database into JSON, formatting data and removing unnecessary information in the process, and uploads the converted file to S3.
Converter then notifies Api that a new version is available.

### Api
Api is an API server intended to run on Amazon EC2 using AWS Elastic Beanstalk.
The `Update data` operation loads the JSON file from S3 and sends a notification to Firebase Cloud Messaging.
The data is then accessible to Web and other clients (such as a mobile applications).

Refer to the [API documentation](Api/API_Documentation.md) for more details.

### Web
Web is the website server intended to run on Amazon EC2 using AWS Elastic Beanstalk.
It periodically checks with Api for new or updated data.
After loading, the data is then served as a responsive website.
There is a student and a teacher version, displaying the same data but with different filtering, formatting and arrangement.


## Platforms and Tools
The components are written for different platforms using different tools.

### Uploader (C#/.NET Framework 3.5)
Uploader is developed using Visual Studio 2017. It targets .NET Framework 3.5 for compatibility with Windows XP.

### Converter (Java 8)
Converter uses Gradle as build system and is the only component written in Java.
I use NetBeans for development, but any other IDE can be used.
The Gradle `deploy` task for deployment to AWS Lambda requires the [AWS Command Line Interface](http://docs.aws.amazon.com/cli/latest/userguide/installing.html) to be installed.

### Api and Web (C#/.NET Core 1.1)
Api and Web are written in ASP.NET Core for high performance and cross-platform compatibility.
For development, I use Visual Studio 2017 with the following extensions:
* [BundlerMinifier](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.BundlerMinifier) for minifying CSS and JavaScript files as soon as they have been changed
* [AWS Toolkit](http://docs.aws.amazon.com/toolkit-for-visual-studio/latest/user-guide/getting-set-up.html) for easy deployment to Amazon EC2 using AWS Elastic Beanstalk


## External libraries
The following libraries created by other entities are used by this software:

* [Apache Commons DbUtils](https://commons.apache.org/proper/commons-dbutils/)
* [AWS Logging .NET](https://github.com/aws/aws-logging-dotnet)
* [AWS SDK for Java](https://aws.amazon.com/sdk-for-java/)
* [AWS SDK for .NET](https://aws.amazon.com/sdk-for-net/)
* [BundlerMinifier](https://github.com/madskristensen/BundlerMinifier)
* [Jackson](https://github.com/FasterXML/jackson)
* [Json.NET](http://www.newtonsoft.com/json)
* [UCanAccess](http://ucanaccess.sourceforge.net)
* [Web Markup Minifier](https://github.com/Taritsyn/WebMarkupMin)


## License
This project is licensed under the terms of the MIT license. See [LICENSE.md](LICENSE.md) for further information.
