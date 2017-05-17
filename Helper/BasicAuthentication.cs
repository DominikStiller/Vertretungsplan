using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DominikStiller.VertretungsplanServer.Helper
{
    public class BasicAuthentication
    {
        public static IActionResult Auth(string username, string password, HttpContext context, Action authorizedCallback)
        {
            var authHeader = context.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authHeader) && authHeader.ToString().StartsWith("Basic "))
            {
                var authParts = Encoding.ASCII.GetString(Convert.FromBase64String(authHeader.ToString().Substring(6))).Split(':');
                if (authParts.Length == 2)
                {
                    var requestUsername = authParts[0];
                    var requestPassword = authParts[1];

                    bool authorized = requestUsername == username && requestPassword == password;
                    if (authorized)
                    {
                        authorizedCallback();
                        return new OkResult();
                    }
                }
            }
            context.Response.Headers.Add("WWW-Authenticate", "Basic");
            return new UnauthorizedResult();
        }
    }
}
