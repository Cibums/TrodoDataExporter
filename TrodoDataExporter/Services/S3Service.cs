using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using dotenv.net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TrodoDataExporter.Controllers;
using TrodoDataExporter.Models;

namespace TrodoDataExporter.Services
{
    public class S3Service : IS3Service
    {
        private readonly string? accessKey;
        private readonly string? secretKey;
        private readonly string scrapeUrl = @"https://www.trodo.se/";
        private readonly ILogger _logger;
        private readonly IMemoryCache _cache;
        private readonly IAmazonS3 _s3Client;

        private const string BUCKET_NAME = "trodo-scraper";

        public S3Service(ILogger<S3Service> logger, IMemoryCache cache)
        {
            //Loading acess keys from environment variables
            DotEnv.Load();
            accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

            _cache = cache;
            _logger = logger;

            //Setting s3 client options
            _s3Client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
            {
                ServiceURL = scrapeUrl,
                RegionEndpoint = RegionEndpoint.EUNorth1
            });
        }

        /// <summary>
        /// Gets the latest batch of products scraped by Zyte.com
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task<GetObjectResponse> GetLatestS3Object()
        {
            _logger.LogInformation("Trying to get all product data");

            var cacheKey = "LatestS3Object";

            // Check cache for latest S3 object
            if (_cache.TryGetValue(cacheKey, out GetObjectResponse cachedResponse))
            {
                _logger.LogInformation("Got product data from cache");
                return cachedResponse;
            }

            _logger.LogInformation("Trying to get all product data from server");

            // Retrieve all objects from the S3 bucket
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = BUCKET_NAME
            };

            ListObjectsV2Response response = await _s3Client.ListObjectsV2Async(request);

            //Sort array in order based on what object was last modified
            S3Object[] objectsArray = response.S3Objects.ToArray();

            if (objectsArray.Length <= 0)
            {
                _logger.LogError("Did not find any products");
                throw new FileNotFoundException("No objects found in the S3 bucket");
            }

            S3Object latestObject = objectsArray.Aggregate((j, k) => k.LastModified > j.LastModified ? k : j);

            //Get the content of the latest object
            var getObjectRequest = new GetObjectRequest
            {
                BucketName = BUCKET_NAME,
                Key = latestObject.Key
            };

            var objectResponse = await _s3Client.GetObjectAsync(getObjectRequest).ConfigureAwait(false);

            // Store response in cache
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(300))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(1024);

            _cache.Set(cacheKey, objectResponse, cacheEntryOptions);

            return objectResponse;
        }

        /// <summary>
        /// Deserializes the a response from AWS to Product[]
        /// </summary>
        /// <param name="objectResponse"></param>
        /// <returns></returns>
        public async Task<Product[]> DeserializeS3Object(GetObjectResponse objectResponse)
        {
            var cacheKey = "LatestS3ObjectDeserialized";

            // Check cache for latest S3 object
            if (_cache.TryGetValue(cacheKey, out Product[] cachedResponse))
            {
                _logger.LogInformation("Got deserialized products from cache");
                return cachedResponse;
            }

            //Deserialize object to Product model and return
            using (var streamReader = new StreamReader(objectResponse.ResponseStream))
            {
                var products = new List<Product>();
                while (!streamReader.EndOfStream)
                {
                    string? line = await streamReader.ReadLineAsync().ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        Product? product = JsonConvert.DeserializeObject<Product>(line);
                        if (product != null) products.Add(product);
                    }
                }

                // Store response in cache
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(1024);

                _cache.Set(cacheKey, products.ToArray(), cacheEntryOptions);

                return products.ToArray();
            }
        }
    }
}
