using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace DominikStiller.VertretungsplanServer.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
