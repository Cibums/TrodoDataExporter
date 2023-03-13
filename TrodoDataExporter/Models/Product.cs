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

        public bool? IsInStock()
        {
            return offers?.FirstOrDefault()?.availability?.Equals("InStock");
        }

        public decimal? Price()
        {
            string? priceString = offers?.FirstOrDefault()?.price?.Replace(".", ",");
            return decimal.TryParse(priceString, out decimal priceDecimal) ? priceDecimal : null;
        }

        public string Manufacturer()
        {
            return SingleProperty("tillverkare");
        }

        public string ArticleNumber()
        {
            return SingleProperty("art. nr.");
        }

        public string Id()
        {
            return SingleProperty("id");
        }

        public string EAN()
        {
            return SingleProperty("ean");
        }

        /// <summary>
        /// Gets the value of a specific property from the produc'ts additional properties
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private string SingleProperty(string propertyName)
        {
            AdditionalProperty? property = additionalProperty?.FirstOrDefault(prop => prop.name?.ToLower() == propertyName.ToLower());

            if (property != null)
            {
                return property.value?.ToLower() ?? string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
