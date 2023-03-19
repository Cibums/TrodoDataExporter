using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrodoDataExporter.Models;
using TrodoDataExporter.Services;

namespace TrodoDataExporterTests.Services
{
    public class CategoryParserTests
    {
        [Fact]
        public void Parse_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var categories = new List<string>();

            // Act
            var result = CategoryParser.Parse(categories);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Parse_ReturnsExpectedResult()
        {
            // Act
            var actualCategories = CategoryParser.Parse(new List<string>()
            {
                "Tools/Handheld/Hammer",
                "Tools/Handheld/Saw",
                "Tools/Machines"
            });

            // Assert
            Assert.Single(actualCategories);

            Assert.Contains(actualCategories, c => c.Name == "Tools");

            Assert.Contains(actualCategories[0].Children, c => c.Name == "Handheld");
            Assert.Contains(actualCategories[0].Children, c => c.Name == "Machines");

            Assert.Contains(actualCategories[0].Children[0].Children, c => c.Name == "Hammer");
            Assert.Contains(actualCategories[0].Children[0].Children, c => c.Name == "Saw");

            Assert.Empty(actualCategories[0].Children[1].Children);
        }
    }
}
