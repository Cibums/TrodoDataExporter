namespace TrodoDataExporter.Models
{
    public class Product
    {
        public string? url { get; set; }
        public string? canonicalUrl { get; set; }
        public double probability { get; set; }
        public string? name { get; set; }
        public List<Offer>? offers { get; set; }
        public string? sku { get; set; }
        public string? mpn { get; set; }
        public string? brand { get; set; }
        public List<Breadcrumb>? breadcrumbs { get; set; }
        public string? mainImage { get; set; }
        public List<string>? images { get; set; }
        public string? description { get; set; }
        public string? descriptionHtml { get; set; }
        public List<AdditionalProperty>? additionalProperty { get; set; }
    }
}
