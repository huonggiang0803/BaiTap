namespace WebApp.Models.DTOs
{
    public class MasterProductDtoSaleOut
    {
        public Guid Id { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Unit { get; set; }
        public decimal QuantityPerBox { get; set; }
    }
}
