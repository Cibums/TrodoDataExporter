using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using dotenv.net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TrodoDataExporter.Models;
using TrodoDataExporter.Services;

namespace TrodoDataExporter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        public IS3Service _s3Service;

        public ProductsController(ILogger<ProductsController> logger, IS3Service s3Service)
        {
            _logger = logger;
            _s3Service = s3Service;
        }

        /// <summary>
        /// Gets a JSON of all products from Trodo.se
        /// </summary>
        /// <returns></returns>
        [HttpGet("Get")]
        public async Task<ActionResult<ProductSimplified[]>> GetProducts()
        {
            try
            {
                GetObjectResponse response = await _s3Service.GetLatestS3Object();
                Product[] products = await _s3Service.DeserializeS3Object(response);
                return products.Select(product => new ProductSimplified(product)).ToArray();
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

        /// <summary>
        /// Gets a JSON of all products from Trodo.se in a certain category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpGet("GetByCategory/{category}")]
        public async Task<ActionResult<Product[]>> GetProductsFromBrand(string category)
        {
            try
            {
                GetObjectResponse response = await _s3Service.GetLatestS3Object();
                Product[] allProducts = await _s3Service.DeserializeS3Object(response);

                return allProducts.Where(p => p.breadcrumbs.Any(b => b.name?.ToLower() == category.ToLower())).ToArray();
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

        /// <summary>
        /// Gets a JSON of all products from Trodo.se after going through a filtering process
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <param name="ean"></param>
        /// <param name="articleNumber"></param>
        /// <param name="minPrice"></param>
        /// <param name="maxPrice"></param>
        /// <param name="isInStock"></param>
        /// <returns></returns>
        [HttpGet("GetFiltered")]
        public async Task<ActionResult<Product[]>> GetFiltered(
            string? manufacturer = null,
            string? ean = null,
            string? articleNumber = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isInStock = null
        )
        {
            try
            {
                GetObjectResponse response = await _s3Service.GetLatestS3Object();
                Product[] allProducts = await _s3Service.DeserializeS3Object(response);
                Func<Product, bool> filter = p =>
                    (manufacturer == null || p.Manufacturer().Equals(manufacturer.ToLower())) &&
                    (ean == null || p.EAN().ToLower().Equals(ean.ToLower())) &&
                    (articleNumber == null || p.ArticleNumber().ToLower().Equals(articleNumber.ToLower())) &&
                    (isInStock == null || p.IsInStock().Equals(isInStock)) &&
                    (minPrice == null || p.Price() >= minPrice) &&
                    (maxPrice == null || p.Price() <= maxPrice);

                return allProducts.Where(filter).ToArray();
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
    }
}