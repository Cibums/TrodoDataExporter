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
    [Route("api/[controller]")]
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
        [HttpGet("All")]
        public async Task<ActionResult<ProductSimplified[]>> GetProductsSimplified()
        {
            _logger.LogInformation("Getting all products");
            Product[] products = await _s3Service.GetLatestS3ObjectDeserialized();
            return products.Select(product => new ProductSimplified(product)).ToArray();
        }

        /// <summary>
        /// Gets a JSON of all products from Trodo.se
        /// </summary>
        /// <returns></returns>
        [HttpGet("AllExtended")]
        public async Task<ActionResult<Product[]>> GetProductsExtended()
        {
            _logger.LogInformation("Getting all products");
            return await _s3Service.GetLatestS3ObjectDeserialized();
        }

        /// <summary>
        /// Gets a JSON of all products from Trodo.se after going through a filtering process
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <param name="ean"></param>
        /// <param name="articleNumber"></param>
        /// <param name="category"></param>
        /// <param name="minPrice"></param>
        /// <param name="maxPrice"></param>
        /// <param name="isInStock"></param>
        /// <returns></returns>
        [HttpGet("Filter")]
        public async Task<ActionResult<ProductSimplified[]>> GetFilteredSimplified(
            string? manufacturer = null,
            string? ean = null,
            string? articleNumber = null,
            string? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isInStock = null
        )
        {
            var products = await FilterProducts(manufacturer, ean, articleNumber, category, minPrice, maxPrice, isInStock);
            return products.Select(product => new ProductSimplified(product)).ToArray();
        }

        /// <summary>
        /// Gets a JSON of all products from Trodo.se after going through a filtering process
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <param name="ean"></param>
        /// <param name="articleNumber"></param>
        /// <param name="category"></param>
        /// <param name="minPrice"></param>
        /// <param name="maxPrice"></param>
        /// <param name="isInStock"></param>
        /// <returns></returns>
        [HttpGet("FilterExtended")]
        public async Task<ActionResult<Product[]>> GetFilteredExtended(
            string? manufacturer = null,
            string? ean = null,
            string? articleNumber = null,
            string? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isInStock = null
        )
        {
            return await FilterProducts(manufacturer, ean, articleNumber, category, minPrice, maxPrice, isInStock);
        }

        [NonAction]
        public async Task<Product[]> FilterProducts(
            string? manufacturer = null,
            string? ean = null,
            string? articleNumber = null,
            string? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isInStock = null
        )
        {
            _logger.LogInformation("Getting filtered products");
            Product[] objectResponse = await _s3Service.GetLatestS3ObjectDeserialized();

            Func<Product, bool> filter = p =>
                (manufacturer == null || p.Manufacturer().Equals(manufacturer.ToLower())) &&
                (ean == null || p.EAN().ToLower().Equals(ean.ToLower())) &&
                (articleNumber == null || p.ArticleNumber().ToLower().Equals(articleNumber.ToLower())) &&
                (isInStock == null || p.IsInStock().Equals(isInStock)) &&
                (category == null || p.breadcrumbs.Any(b => b.name?.ToLower() == category.ToLower())) &&
                (minPrice == null || p.Price() >= minPrice) &&
                (maxPrice == null || p.Price() <= maxPrice);

            return objectResponse.Where(filter).ToArray();
        }
    }
}