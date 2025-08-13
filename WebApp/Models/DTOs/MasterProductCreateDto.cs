using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.DTOs
{
    public class MasterProductCreateDto
    {
        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Đơn vị là bắt buộc")]
        public string Unit { get; set; }

        [Required(ErrorMessage = "Quy cách là bắt buộc")]
        public string Specification { get; set; }

        [Required(ErrorMessage = "Số lượng/Thùng là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Số lượng/Thùng phải >= 0")]
        public decimal QuantityPerBox { get; set; }


        [Required(ErrorMessage = "Trọng lượng là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Trọng lượng phải lớn hơn hoặc bằng 0")]
        public decimal ProductWeight { get; set; }
    }
}
