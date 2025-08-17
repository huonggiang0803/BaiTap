namespace WebApp.Models.DTOs
{
    public class SaleOutUpdateDto
    {
        public Guid Id { get; set; }
        public string CustomerPoNo { get; set; }
        public int OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Unit { get; set; }

        // cho phép chỉnh sửa
        public decimal Quantity { get; set; }
        public decimal QuantityPerBox { get; set; }
        public decimal Price { get; set; }


    }
}
