using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.ApplicationInsights.Extensibility;

namespace DominikStiller.VertretungsplanServer.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TelemetryConfiguration.Active.DisableTelemetry = true;

            var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
