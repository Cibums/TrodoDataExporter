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
        private readonly ILogger<CategoriesController> _logger;
        public IS3Service _s3Service;

        public CategoriesController(ILogger<CategoriesController> logger, IS3Service s3Service)
        {
            _logger = logger;
            _s3Service = s3Service;
        }

        [HttpGet("CategoryTree")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategoryTree()
        {
            _logger.LogInformation("Getting all categories");
            var products = await _s3Service.GetLatestS3ObjectDeserialized();

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
            _logger.LogInformation("Getting most specific categories");
            var products = await _s3Service.GetLatestS3ObjectDeserialized();

            var specificCategories = products
            .Select(product => product.MostSpecificCategory())
            .Where(specificCategory => !string.IsNullOrEmpty(specificCategory))
            .ToHashSet();

            return Ok(specificCategories);
        }
    }
}
