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
        private readonly IS3Service _s3Service;

        public ProductsController(ILogger<ProductsController> logger, IS3Service s3Service)
        {
            _logger = logger;
            _s3Service = s3Service;
        }

        [HttpGet("Get")]
        public async Task<ActionResult<Product[]>> GetProducts()
        {
            try
            {
                GetObjectResponse response = await _s3Service.GetLatestS3Object();
                return await _s3Service.DeserializeS3Object(response);
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