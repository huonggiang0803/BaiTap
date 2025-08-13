namespace WebApi.Models.Entities
{
    public class MasterProduct
    {
        public Guid Id { get; set; } 
        public string ProductCode { get; set; } 
        public string ProductName { get; set; } 
        public string Unit { get; set; } 
        public string Specification { get; set; } 
        public decimal QuantityPerBox { get; set; } 
        public decimal ProductWeight { get; set; }

        // Quan hệ 1-n với SaleOut
        public ICollection<SaleOut> SaleOuts { get; set; }
    }
}
