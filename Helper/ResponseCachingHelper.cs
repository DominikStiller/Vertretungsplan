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
    }
}
