using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using dotenv.net;
using Newtonsoft.Json;
using TrodoDataExporter.Models;

namespace TrodoDataExporter.Services
{
    public class S3Service : IS3Service
    {
        private readonly string? accessKey;
        private readonly string? secretKey;
        private readonly string scrapeUrl = @"https://www.trodo.se/";
        private const string BUCKET_NAME = "trodo-scraper";
        private IAmazonS3 s3Client;

        public S3Service()
        {
            //Loading acess keys from environment variables
            DotEnv.Load();
            accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

            //Setting s3 client options
            s3Client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
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
            // Retrieve all objects from the S3 bucket
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = BUCKET_NAME
            };

            ListObjectsV2Response response = await s3Client.ListObjectsV2Async(request);

            //Sort array in order based on what object was last modified
            S3Object[] objectsArray = response.S3Objects.ToArray();

            if (objectsArray.Length <= 0)
            {
                throw new FileNotFoundException("No objects found in the S3 bucket");
            }

            S3Object latestObject = objectsArray.Aggregate((j, k) => k.LastModified > j.LastModified ? k : j);

            //Get the content of the latest object
            var getObjectRequest = new GetObjectRequest
            {
                BucketName = BUCKET_NAME,
                Key = latestObject.Key
            };

            var result = await s3Client.GetObjectAsync(getObjectRequest).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Deserializes the a response from AWS to Product[]
        /// </summary>
        /// <param name="objectResponse"></param>
        /// <returns></returns>
        public async Task<Product[]> DeserializeS3Object(GetObjectResponse objectResponse)
        {
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
                return products.ToArray();
            }
        }
    }
}
