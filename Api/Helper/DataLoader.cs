using Amazon.S3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

using DominikStiller.VertretungsplanServer.Models;
using System.Threading.Tasks;

namespace DominikStiller.VertretungsplanServer.Api.Helper
{
    public class DataLoader
    {
        readonly VertretungsplanRepository cache;
        readonly ILogger logger;
        readonly DataLoaderOptions options;

        AmazonS3Client s3;

        public DataLoader(VertretungsplanRepository cache, ILogger<DataLoader> logger, IOptions<DataLoaderOptions> options)
        {
            this.cache = cache;
            this.logger = logger;
            this.options = options.Value;

            s3 = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1);
        }

        public async Task LoadDataFromS3()
        {
            try
            {
                var response = await s3.GetObjectAsync(options.S3Bucket, options.S3Key);

                string json;
                using (var reader = new StreamReader(response.ResponseStream))
                {
                    json = reader.ReadToEnd();
                }

                var vps = JsonConvert.DeserializeObject<List<Vertretungsplan>>(json);
                cache.Clear();
                cache.AddRange(vps);
            }
            catch (Exception e)
            {
                logger.LogError($"ERROR while loading data\n{e}");
            }
        }
    }

    public class DataLoaderOptions
    {
        public string S3Bucket { get; set; }
        public string S3Key { get; set; }
    }
}
