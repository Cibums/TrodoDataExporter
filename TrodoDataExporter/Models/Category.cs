namespace TrodoDataExporter.Models
{
    public class Category
    {
        public string? Name { get; set; }
        public List<Category> Children { get; set; } = new List<Category>();
        public List<string> Items { get; set; } = new List<string>();
    }
}
