# A simple webserver to display the same content at every URL
# Can be run on a remote server to display the token for the Certbot HTTP challenge
#
# Usage: .\CertbotChallengeWebserver.ps1 -Content certbotToken (from elevated prompt)
#
# Adapted from http://community.idera.com/powershell/powertips/b/tips/posts/creating-powershell-web-server

# start web server
$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://*:80/")
$listener.Start()

[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$content = [Microsoft.VisualBasic.Interaction]::InputBox("Please enter the response body content:", "Content")

try
{
   while($listener.IsListening)
   {
      $buffer = [Text.Encoding]::UTF8.GetBytes($content)

      $response = $listener.GetContext().Response
      $response.ContentLength64 = $buffer.length
      $response.OutputStream.Write($buffer, 0, $buffer.length)

      $response.Close()
   }
}
finally
{
   $listener.Stop()
}
