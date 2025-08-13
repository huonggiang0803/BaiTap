﻿namespace WebApi.Models.DTOs
{
    public class SaleOutDto
    {
        public Guid Id { get; set; }
        public string CustomerPoNo { get; set; }
        public int OrderDate { get; set; } // có thể đổi sang DateTime nếu DB lưu ngày
        public string CustomerName { get; set; }

        public Guid ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Unit { get; set; }

        public decimal Quantity { get; set; }
        public decimal QuantityPerBox { get; set; }
        public decimal BoxQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
    }
}
