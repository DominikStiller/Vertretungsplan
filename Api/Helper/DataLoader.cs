﻿using Amazon.S3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

using DominikStiller.VertretungsplanServer.Models;

namespace DominikStiller.VertretungsplanServer.Api.Helper
{
    public class DataLoader
    {
        readonly VertretungsplanRepository vertretungsplanRepository;
        readonly ILogger logger;
        readonly DataLoaderOptions options;

        AmazonS3Client s3;

        public DataLoader(VertretungsplanRepository vertretungsplanRepository, ILogger<DataLoader> logger, IOptions<DataLoaderOptions> options)
        {
            this.vertretungsplanRepository = vertretungsplanRepository;
            this.logger = logger;
            this.options = options.Value;

            s3 = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1);
        }

        public void LoadDataFromS3()
        {
            try
            {
                var response = s3.GetObjectAsync(options.S3Bucket, options.S3Key).Result;

                string json;
                using (var reader = new StreamReader(response.ResponseStream))
                {
                    json = reader.ReadToEnd();
                }

                var vps = JsonConvert.DeserializeObject<List<Vertretungsplan>>(json);
                vertretungsplanRepository.Clear();
                vertretungsplanRepository.AddRange(vps);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }
    }

    public class DataLoaderOptions
    {
        public string S3Bucket { get; set; }
        public string S3Key { get; set; }
    }
}
