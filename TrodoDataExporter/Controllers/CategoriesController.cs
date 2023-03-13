using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using TrodoDataExporter.Models;
using TrodoDataExporter.Services;

namespace TrodoDataExporter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        public IS3Service _s3Service;

        public CategoriesController(ILogger<ProductsController> logger, IS3Service s3Service)
        {
            _logger = logger;
            _s3Service = s3Service;
        }

        [HttpGet("CategoryTree")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategoryTree()
        {
            var response = await _s3Service.GetLatestS3Object();
            var products = await _s3Service.DeserializeS3Object(response);

            var categoryPaths = products
            .Select(product => product.CategoryPath())
            .Where(category => !string.IsNullOrEmpty(category))
            .ToList();

            var categoryTree = CategoryParser.Parse(categoryPaths);
            return Ok(categoryTree);
        }

        [HttpGet("MostSpecificCategories")]
        public async Task<ActionResult<HashSet<string>>> GetMostSpecificCategories()
        {
            var response = await _s3Service.GetLatestS3Object();
            var products = await _s3Service.DeserializeS3Object(response);

            var specificCategories = products
            .Select(product => product.MostSpecificCategory())
            .Where(specificCategory => !string.IsNullOrEmpty(specificCategory))
            .ToHashSet();

            return Ok(specificCategories);
        }
    }
}
