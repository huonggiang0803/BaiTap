namespace WebApi.Models.Entities
{
    public class SaleOut
    {
        public Guid Id { get; set; } 
        public string CustomerPoNo { get; set; }
        public int OrderDate { get; set; } 
        public string CustomerName { get; set; }
        public Guid ProductId { get; set; } 
        public decimal Quantity { get; set; } 
        public decimal Price { get; set; } 
        public decimal Amount { get; set; } 
        public decimal QuantityPerBox { get; set; } 
        public decimal BoxQuantity { get; set; }

        // Quan hệ n-1 với MasterProduct
        public MasterProduct Product { get; set; }
    }
}
