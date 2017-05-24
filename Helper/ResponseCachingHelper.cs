using System;
using System.Globalization;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DominikStiller.VertretungsplanServer.Helper
{
    public class ResponseCachingHelper
    {
        readonly HttpContext context;

        public ResponseCachingHelper(IHttpContextAccessor context)
        {
            this.context = context.HttpContext;
        }

        /// <returns>The appropriate response or null to indicate that the client does not possess the most recent version of the document</returns>
        public IActionResult UseETag(string tag)
        {
            context.Response.Headers["ETag"] = tag;

            var ifNoneMatch = context.Request.Headers["If-None-Match"];
            if (ifNoneMatch.Count > 0 && ifNoneMatch.Contains(tag))
            {
                return new StatusCodeResult(StatusCodes.Status304NotModified);
            }

            return null;
        }

        public IActionResult UseLastModified(DateTime date)
        {
            context.Response.Headers["Last-Modified"] = date.ToString("r");

            var ifModifiedSince = context.Request.Headers["If-Modified-Since"].ToString();
            if (!string.IsNullOrEmpty(ifModifiedSince))
            {
                DateTime ifModifiedSinceDate;
                var parsingSuccessful = DateTime.TryParseExact(ifModifiedSince, "r", CultureInfo.InvariantCulture, DateTimeStyles.None, out ifModifiedSinceDate);

                if (parsingSuccessful && ifModifiedSinceDate >= date)
                    return new StatusCodeResult(StatusCodes.Status304NotModified);
            }

            return null;
        }
    }
}
