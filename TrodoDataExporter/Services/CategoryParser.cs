using System.Collections.Generic;
using System.Linq;
using TrodoDataExporter.Models;

namespace TrodoDataExporter.Services
{
    public static class CategoryParser
    {
        public static List<Category> Parse(List<string> categories)
        {
            var rootCategories = new List<Category>();

            foreach (var category in categories)
            {
                var parts = category.Split('/');
                var currentNode = rootCategories;

                for (int i = 0; i < parts.Length; i++)
                {
                    var existingNode = currentNode.FirstOrDefault(c => c.Name == parts[i]);
                    if (existingNode == null)
                    {
                        var newNode = new Category { Name = parts[i] };
                        currentNode.Add(newNode);
                        currentNode = newNode.Children;
                    }
                    else
                    {
                        currentNode = existingNode.Children;
                    }

                    if (i == parts.Length - 1)
                    {
                        existingNode?.Items.Add(parts.Last());
                    }
                }
            }

            return rootCategories;
        }
    }
}
