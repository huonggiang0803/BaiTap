namespace WebApi.Models.DTOs
{
    public class SaleOutCreateDto
    {
        public string CustomerPoNo { get; set; }
        public int OrderDate { get; set; }
        public string CustomerName { get; set; }
        public Guid ProductId { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityPerBox { get; set; }
    }
}
