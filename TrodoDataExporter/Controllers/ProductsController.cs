using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using dotenv.net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TrodoDataExporter.Models;

namespace TrodoDataExporter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly string? accessKey;
        private readonly string? secretKey;
        private readonly string scrapeUrl = @"https://www.trodo.se/";
        private const string BUCKET_NAME = "trodo-scraper";
        private IAmazonS3 s3Client;

        public ProductsController(ILogger<ProductsController> logger)
        {
            _logger = logger;

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

        [HttpGet(Name = "GetProducts")]
        public async Task<ActionResult<Product[]>> GetProducts()
        {
            try
            {
                GetObjectResponse response = await GetLatestS3Object();
                return await DeserializeS3Object(response);
            }
            catch (AmazonS3Exception e)
            {
                return StatusCode(500, $"AmazonS3Exception: {e.Message}");
            }
            catch (Exception e)
            {
                // Log the exception and return an error response
                _logger.LogError(e, "An error occurred while retrieving the S3 object.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        //[HttpGet(Name = "GetFilteredProducts")]
        //public async Task<ActionResult<Product[]>> GetProductsFromBrand(string brand)
        //{
        //    try
        //    {
        //        GetObjectResponse response = await GetLatestS3Object();
        //        Product[] allProducts = await DeserializeS3Object(response);
        //        return allProducts.Where(p => p.brand?.ToLower() == brand.ToLower()).ToArray();
        //    }
        //    catch (AmazonS3Exception e)
        //    {
        //        return StatusCode(500, $"AmazonS3Exception: {e.Message}");
        //    }
        //    catch (Exception e)
        //    {
        //        // Log the exception and return an error response
        //        _logger.LogError(e, "An error occurred while retrieving the S3 object.");
        //        return StatusCode(StatusCodes.Status500InternalServerError);
        //    }
        //}

        private async Task<GetObjectResponse> GetLatestS3Object()
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

        private async Task<Product[]> DeserializeS3Object(GetObjectResponse objectResponse)
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