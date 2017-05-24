using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace DominikStiller.VertretungsplanServer.Api.Helper
{
    public class Notifier
    {
        readonly HttpClient client;
        readonly NotifierOptions options;

        public Notifier(IOptions<NotifierOptions> options)
        {
            this.options = options.Value;
            client = new HttpClient();
        }

        // Send notification to Firebase Cloud Messaging that new or updated data is available
        public async Task NotifyFCM()
        {
            if (options.FCMEnabled)
            {
                var client = new HttpClient() { BaseAddress = new Uri("https://fcm.googleapis.com/") };
                var request = new HttpRequestMessage(HttpMethod.Post, "/fcm/send");
                request.Headers.TryAddWithoutValidation("Authorization", "key=" + options.FCMServerKey);
                var content = new
                {
                    to = "/topics/updated"
                };
                request.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
                await client.SendAsync(request);
            }
        }
    }

    public class NotifierOptions
    {
        public bool FCMEnabled { get; set; }
        public string FCMServerKey { get; set; }
    }
}
