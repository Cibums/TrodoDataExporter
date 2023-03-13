namespace TrodoDataExporter.Models
{
    public class ProductSimplified
    {
        public string CategoryPath { get; set; }
        public string MostSpecificCategory { get; set; }
        public string Brand { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string EanNumber { get; set; }
        public string ArticleNumber { get; set; }
        public decimal? Price { get; set; }
        public bool? IsInStock { get; set; }
        public string Image { get; set; }

        public ProductSimplified(Product product)
        {
            CategoryPath = product.CategoryPath();
            MostSpecificCategory = product.MostSpecificCategory() ?? string.Empty;
            Brand = product.brand ?? string.Empty;
            Title = product.name ?? string.Empty;
            Description = product.description ?? string.Empty;
            EanNumber = product.EAN();
            ArticleNumber = product.ArticleNumber();
            Price = product.Price();
            IsInStock = product.IsInStock();
            Image = product.mainImage ?? string.Empty;
        }
    }
}
