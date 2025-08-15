﻿using Microsoft.EntityFrameworkCore;

[Keyless]
public class SaleOutReportDto
{
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Amount => Quantity * Price;
}
