namespace WebApp.Models.DTOs
{
    public class ProductDropdownDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string Unit { get; set; }
        public decimal QuantityPerBox { get; set; }
    }
}
