using System.ComponentModel.DataAnnotations;
namespace WebApp.Models.DTOs
{
    public class SaleOutCreateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập số PO khách hàng")]
        public string CustomerPoNo { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày đặt hàng")]

        public int OrderDate { get; set; }
        public string CustomerName { get; set; }
        public Guid ProductId { get; set; }

        public decimal BoxQuantity { get; set; }     
        public decimal Amount { get; set; }
        public string Unit { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đơn giá")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityPerBox { get; set; }
    }
}
