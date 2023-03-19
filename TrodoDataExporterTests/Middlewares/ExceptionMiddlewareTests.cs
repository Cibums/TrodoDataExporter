using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrodoDataExporter.Middlewares;

namespace TrodoDataExporterTests.Middlewares
{
    public class ExceptionMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_NoError_ReturnsNextMiddleware()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var nextMiddleware = new Mock<RequestDelegate>();
            var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            var middleware = new ExceptionMiddleware(nextMiddleware.Object, loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextMiddleware.Verify(x => x(context), Times.Once);
            loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task InvokeAsync_CatchesAmazonS3Exception_Returns500StatusCode()
        {
            // Arrange
            var expectedErrorMessage = "An S3 error occurred.";
            var exception = new AmazonS3Exception(expectedErrorMessage);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();

            var middleware = new ExceptionMiddleware(
                innerContext => throw exception,
                loggerMock.Object
            );

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = new StreamReader(context.Response.Body).ReadToEnd();
            Assert.Equal("AmazonS3Exception: " + expectedErrorMessage, responseText);
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_CatchesException_Returns500StatusCode()
        {
            // Arrange
            var expectedErrorMessage = "An error occurred.";
            var exception = new Exception(expectedErrorMessage);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();

            var middleware = new ExceptionMiddleware(
                innerContext => throw exception,
                loggerMock.Object
            );

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = new StreamReader(context.Response.Body).ReadToEnd();
            Assert.Equal("An error occurred: " + expectedErrorMessage, responseText);
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        }
    }
}
