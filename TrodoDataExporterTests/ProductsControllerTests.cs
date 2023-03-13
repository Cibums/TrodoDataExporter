using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Text;
using TrodoDataExporter.Controllers;
using TrodoDataExporter.Models;
using TrodoDataExporter.Services;

namespace TrodoDataExporterTests
{
    public class ProductsControllerTests
    {
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            // Arrange
            var logger = new Mock<ILogger<ProductsController>>();
            var s3Service = new Mock<IS3Service>();
            _controller = new ProductsController(logger.Object, s3Service.Object);
        }

        [Fact]
        public async Task GetProducts_Returns_Products()
        {
            // Arrange
            var products = new[] { new Product(), new Product() };
            var response = new GetObjectResponse { ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(products))) };
            var s3Service = new Mock<IS3Service>();
            s3Service.Setup(s => s.GetLatestS3Object()).ReturnsAsync(response);
            _controller._s3Service = s3Service.Object;

            // Act
            var result = await _controller.GetProducts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProducts = Assert.IsAssignableFrom<Product[]>(okResult.Value);
            Assert.Equal(products.Length, returnedProducts.Length);
        }
    }
}