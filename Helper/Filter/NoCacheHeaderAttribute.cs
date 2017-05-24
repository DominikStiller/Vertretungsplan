using Microsoft.AspNetCore.Mvc.Filters;

namespace DominikStiller.VertretungsplanServer.Helper
{
    public class NoCacheHeaderAttribute : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers["Cache-Control"] = "no-cache";
        }
    }
}
