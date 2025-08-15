using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models.DTOs
{
    public class SaleOutReportDto
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }

        [Column("TotalQuantity")]
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount => Quantity * Price;
    }
}
