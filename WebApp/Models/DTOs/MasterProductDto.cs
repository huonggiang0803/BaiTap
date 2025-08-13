using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.DTOs
{
    public class MasterProductDto
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã sản phẩm.")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đơn vị.")]
        public string Unit { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập quy cách.")]
        public string Specification { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng/thùng.")]
        public decimal? QuantityPerBox { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập trọng lượng.")]
        public decimal? ProductWeight { get; set; }

    }
}
