using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

[Keyless]
public class SaleOutReportDto
{
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Amount => Quantity * Price;
}
//CREATE FUNCTION fnSaleOutReport
//(
//    @StartDate INT,
//    @EndDate INT
//)
//RETURNS TABLE
//AS
//RETURN
//(
//    SELECT  
//        mp.ProductCode,
//        mp.ProductName,
//        AVG(so.Price) AS Price,
//        SUM(so.Quantity) AS Quantity, -- alias đúng tên DTO
//        SUM(so.Amount) AS TotalAmount
//    FROM SaleOuts so
//    INNER JOIN MasterProducts mp ON so.ProductId = mp.Id
//    WHERE so.OrderDate BETWEEN @StartDate AND @EndDate
//    GROUP BY mp.ProductCode, mp.ProductName
//);
