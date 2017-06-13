# private/
This directory contains private data and server-specific scripts.

## Contents

* `certs/`: Contains SSL certificates and scripts to generate them via [Certbot](https://certbot.eff.org/) running on the Windows Subsystem for Linux
* `Scripts/`
   * `LoadWebCertFromS3.ps1`/`LoadApiCertFromS3.ps1`: Scripts to download the SSL certificate and their password from Amazon S3 to the web/API server for `InstallSSLCert.ps1` to use
