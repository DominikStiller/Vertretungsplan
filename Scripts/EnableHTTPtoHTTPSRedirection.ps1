Import-Module WebAdministration

# Redirect http to https
# https://blogs.msdn.microsoft.com/kaushal/2013/05/22/http-to-https-redirects-on-iis-7-x-and-higher/
$site = "IIS:\Sites\Default Web Site"
$filterRoot = "/System.WebServer/Rewrite/Rules/Rule[@name = 'HTTP to HTTPS Redirect']"

Add-WebConfigurationProperty -pspath $site -filter '/System.WebServer/Rewrite/Rules' -name "." -value @{name = "HTTP to HTTPS Redirect"; stopProcessing = "true"}
Set-WebConfigurationProperty -pspath $site -filter "$filterRoot/Match" -name "url" -value "(.*)"

Set-WebConfigurationProperty -pspath $site -filter "$filterRoot/Conditions" -name "logicalGrouping" -value "MatchAny"
Add-WebConfigurationProperty -pspath $site -filter "$filterRoot/Conditions" -name "." -value @{input = "{SERVER_PORT_SECURE}"; pattern = "^0$"}

Set-WebConfigurationProperty -pspath $site -filter "$filterRoot/Action" -name "type" -value "Redirect"
Set-WebConfigurationProperty -pspath $site -filter "$filterRoot/Action" -name "url" -value "https://{HTTP_HOST}{REQUEST_URI}"
Set-WebConfigurationProperty -pspath $site -filter "$filterRoot/Action" -name "redirectType" -value "Permanent"
