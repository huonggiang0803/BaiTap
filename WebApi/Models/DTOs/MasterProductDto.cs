﻿namespace WebApi.Models.DTOs
{
    public class MasterProductDto
    {
        public Guid Id { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Unit { get; set; }
        public string Specification { get; set; }
        public decimal QuantityPerBox { get; set; }
        public decimal ProductWeight { get; set; }

    }
}
