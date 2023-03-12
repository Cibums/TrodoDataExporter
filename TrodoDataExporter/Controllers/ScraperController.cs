using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using dotenv.net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace TrodoDataExporter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScraperController : ControllerBase
    {
        private readonly ILogger<ScraperController> _logger;

        private readonly string accessKey;
        private readonly string secretKey;

        private readonly string scrapeUrl = @"https://www.trodo.se/";

        private const string BUCKET_NAME = "trodo-scraper";

        private IAmazonS3 s3Client;

        public ScraperController(ILogger<ScraperController> logger)
        {
            DotEnv.Load();

            accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

            _logger = logger;

            s3Client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
            {
                ServiceURL = scrapeUrl,
                RegionEndpoint = RegionEndpoint.EUNorth1
            });
        }

        [HttpGet(Name = "GetS3Object")]
        public async Task<Product[]> Get()
        {
            // Retrieve an object from the S3 bucket
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = BUCKET_NAME
            };

            ListObjectsV2Response response = await s3Client.ListObjectsV2Async(request);

            S3Object[] objectsArray = response.S3Objects.ToArray();
            S3Object latestObject = objectsArray.Aggregate((j, k) => k.LastModified > j.LastModified ? k : j);

            var getObjectRequest = new GetObjectRequest
            {
                BucketName = BUCKET_NAME,
                Key = latestObject.Key
            };

            var result = await s3Client.GetObjectAsync(getObjectRequest);
            using var streamReader = new StreamReader(result.ResponseStream);
            var products = new List<Product>();
            while (!streamReader.EndOfStream)
            {
                string line = await streamReader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    products.Add(JsonConvert.DeserializeObject<Product>(line));
                }
            }
            return products.ToArray();
        }
    }
}