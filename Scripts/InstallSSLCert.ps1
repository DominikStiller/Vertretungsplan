# Requires the .pfx file and its password at "C:\ssl-cert.pfx" and "C:\ssl-cert_password.txt" respectively
# Adapted from http://docs.aws.amazon.com/elasticbeanstalk/latest/dg/SSLNET.SingleInstance.html

Import-Module WebAdministration

# Cleanup existing binding
if (Get-WebBinding "Default Web Site" -Port 443) {
	Remove-WebBinding -Name "Default Web Site" -BindingInformation *:443:
}
if (Get-Item -path IIS:\SslBindings\0.0.0.0!443) {
	Remove-Item -path IIS:\SslBindings\0.0.0.0!443
}

# Install certificate
$pwd = Get-Content "C:\ssl-cert_password.txt" -Raw
$securepwd = ConvertTo-SecureString -String $pwd -Force -AsPlainText
$cert = Import-PfxCertificate -FilePath "C:\ssl-cert.pfx" cert:\localMachine\my -Password $securepwd

Remove-Item "C:\ssl-cert.pfx"
Remove-Item "C:\ssl-cert_password.txt"

# Create site binding
New-WebBinding -Name "Default Web Site" -IP "*" -Port 443 -Protocol https
New-Item -path IIS:\SslBindings\0.0.0.0!443 -value $cert -Force

# Update firewall
netsh advfirewall firewall add rule name="Open port 443" protocol=TCP localport=443 action=allow dir=OUT
