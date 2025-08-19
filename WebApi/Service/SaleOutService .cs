using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Reflection;
using WebApi.Data;
using WebApi.Models.DTOs;
using WebApi.Models.Entities;
using WebApi.Service;
using WebApp.Models.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApi.Services
{
    public class SaleOutService : ISaleOutService
    {
        private readonly ApplicationDbContext _context;

        public SaleOutService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<SaleOutDto>> GetAllSaleOutAsync(string field = null, string keyword = null)
        {
            var query = _context.SaleOuts
                .Include(s => s.Product) // Join MasterProduct
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();

                var fieldMap = new Dictionary<string, Func<SaleOut, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "customerpono", s => s.CustomerPoNo },
            { "orderdate", s => s.OrderDate.ToString() },
            { "customername", s => s.CustomerName },
            { "productcode", s => s.Product.ProductCode },
            { "productname", s => s.Product.ProductName },
            { "unit", s => s.Product.Unit },
            { "quantity", s => s.Quantity.ToString() },
            { "quantityperbox", s => s.QuantityPerBox.ToString() },
            { "boxquantity", s => s.BoxQuantity.ToString() },
            { "price", s => s.Price.ToString() },
            { "amount", s => s.Amount.ToString() }
        };

                if (!string.IsNullOrWhiteSpace(field) && fieldMap.ContainsKey(field))
                {
                    // Tìm theo 1 trường
                    query = query.AsEnumerable()
                                 .Where(s => fieldMap[field](s)?.ToLower().Contains(keyword) ?? false)
                                 .AsQueryable();
                }
                else
                {
                    // Tìm trên nhiều trường string
                    query = query.AsEnumerable()
                                 .Where(s =>
                                     (s.CustomerPoNo?.ToLower().Contains(keyword) ?? false) ||
                                     s.OrderDate.ToString().Contains(keyword) ||
                                     (s.CustomerName?.ToLower().Contains(keyword) ?? false) ||
                                     (s.Product?.ProductCode?.ToLower().Contains(keyword) ?? false) ||
                                     (s.Product?.ProductName?.ToLower().Contains(keyword) ?? false) ||
                                     (s.Product?.Unit?.ToLower().Contains(keyword) ?? false) ||
                                     s.Quantity.ToString().Contains(keyword) ||
                                     s.QuantityPerBox.ToString().Contains(keyword) ||
                                     s.BoxQuantity.ToString().Contains(keyword) ||
                                     s.Price.ToString().Contains(keyword) ||
                                     s.Amount.ToString().Contains(keyword)
                                 )
                                 .AsQueryable();
                }
            }

            return query.Select(s => new SaleOutDto
            {
                Id = s.Id,
                CustomerPoNo = s.CustomerPoNo,
                OrderDate = s.OrderDate,
                CustomerName = s.CustomerName,
                ProductId = s.ProductId,
                ProductCode = s.Product.ProductCode,
                ProductName = s.Product.ProductName,
                Unit = s.Product.Unit,
                Quantity = s.Quantity,
                QuantityPerBox = s.QuantityPerBox,
                BoxQuantity = s.BoxQuantity,
                Price = s.Price,
                Amount = s.Amount
            }).ToList();
        }


        public async Task<bool> IsDuplicatePoNoAsync(string poNo)
        {
            return await _context.SaleOuts.AnyAsync(s => s.CustomerPoNo == poNo);
        }
        public async Task<SaleOutDto> CreateSaleOutAsync(SaleOutCreateDto dto)
        {
            if (await IsDuplicatePoNoAsync(dto.CustomerPoNo))
                throw new Exception("Số PO khách hàng đã tồn tại.");

            var product = await _context.MasterProducts
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
                throw new Exception("Không tìm thấy sản phẩm");

            var entity = new SaleOut
            {
                Id = Guid.NewGuid(),
                CustomerPoNo = dto.CustomerPoNo,
                OrderDate = dto.OrderDate,
                CustomerName = dto.CustomerName,
                ProductId = product.Id,
                Quantity = dto.Quantity,
                Price = dto.Price,
                Amount = dto.Quantity * dto.Price,
                QuantityPerBox = product.QuantityPerBox,
                BoxQuantity = Math.Ceiling(dto.Quantity / product.QuantityPerBox)
            };

            _context.SaleOuts.Add(entity);
            await _context.SaveChangesAsync();

            return new SaleOutDto
            {
                Id = entity.Id,
                CustomerPoNo = entity.CustomerPoNo,
                OrderDate = entity.OrderDate,
                CustomerName = entity.CustomerName,
                ProductId = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Quantity = entity.Quantity,
                QuantityPerBox = product.QuantityPerBox,
                BoxQuantity = entity.BoxQuantity,
                Price = entity.Price,
                Amount = entity.Amount
            };
        }
        public async Task<SaleOutDto> UpdateSaleOutAsync(Guid id, SaleOutUpdateDto dto)
        {
            var saleOut = await _context.SaleOuts
                .FirstOrDefaultAsync(s => s.Id == id);

            if (saleOut == null)
                throw new Exception("Không tìm thấy SaleOut");

            saleOut.Quantity = dto.Quantity;
            saleOut.Price = dto.Price;
            saleOut.QuantityPerBox = dto.QuantityPerBox;
            saleOut.BoxQuantity = dto.QuantityPerBox > 0
                ? Math.Ceiling(dto.Quantity / dto.QuantityPerBox)
                : 0;
            saleOut.Amount = dto.Quantity * dto.Price;

            await _context.SaveChangesAsync();
            return new SaleOutDto
            {
                Id = saleOut.Id,
                CustomerPoNo = saleOut.CustomerPoNo,
                OrderDate = saleOut.OrderDate,
                CustomerName = saleOut.CustomerName,
                ProductId = saleOut.ProductId,
                ProductCode = saleOut.Product?.ProductCode,
                ProductName = saleOut.Product?.ProductName,
                Unit = saleOut.Product?.Unit,
                Quantity = saleOut.Quantity,
                QuantityPerBox = saleOut.QuantityPerBox,
                BoxQuantity = saleOut.BoxQuantity,
                Price = saleOut.Price,
                Amount = saleOut.Amount
            };
        }
        public async Task<List<SaleOutReportDto>> GetSaleOutReportAsync(int startDate, int endDate)
        {
            return await _context.SaleOutReport
                .FromSqlInterpolated($"SELECT * FROM fnSaleOutReport({startDate}, {endDate})")
                .ToListAsync();
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.SaleOuts.FindAsync(id);
            if (entity == null)
                return false;

            _context.SaleOuts.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<(bool IsSuccess, List<string> Errors)> ImportFromExcelAsync(IFormFile file)
        {
            var errors = new List<string>();
            if (file == null || file.Length == 0)
            {
                errors.Add("File tải lên rỗng hoặc không tồn tại.");
                return (false, errors);
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header

            // Lấy tất cả sản phẩm trong DB
            var productsInDb = await _context.MasterProducts.ToListAsync();
            var productDict = productsInDb.ToDictionary(p => p.ProductCode, p => p);

            // Lưu PO + ProductId đã có
            var existingSaleOuts = await _context.SaleOuts.ToListAsync();
            //kiểm tra trùng dữ liệu PO +sản phẩm.
            var existingKeys = existingSaleOuts.Select(s => (s.CustomerPoNo, s.ProductId)).ToHashSet();
            int rowIndex = 1; // 1 là header, nên khi skip thì dòng đầu tiên dữ liệu = 2

            foreach (var row in rows.Select((value, index) => new { value, index }))
            {
                int currentRow = row.index + 2; // +2 vì index=0 => dòng Excel số 2
                var poNo = row.value.Cell(1).GetString().Trim();
                var orderDateStr = row.value.Cell(2).GetString().Trim();
                var customerName = row.value.Cell(3).GetString().Trim();
                var productCode = row.value.Cell(4).GetString().Trim();
                var unit = row.value.Cell(5).GetString().Trim();
                var quantityStr = row.value.Cell(6).GetString().Trim();
                var priceStr = row.value.Cell(7).GetString().Trim();
                var quantityPerBoxStr = row.value.Cell(8).GetString().Trim();

                var rowErrors = new List<string>();

                // Validate
                if (string.IsNullOrWhiteSpace(poNo))
                    rowErrors.Add("Trường 'Số PO khách hàng' không được để trống");

                if (!DateTime.TryParse(orderDateStr, out DateTime orderDate))
                    rowErrors.Add("Trường 'Ngày đặt hàng' phải là ngày hợp lệ");

                if (string.IsNullOrWhiteSpace(customerName))
                    rowErrors.Add("Trường 'Khách hàng' không được để trống");

                if (string.IsNullOrWhiteSpace(productCode))
                    rowErrors.Add("Trường 'Mã sản phẩm' không được để trống");

                if (string.IsNullOrWhiteSpace(unit))
                    rowErrors.Add("Trường 'Đơn vị tính' không được để trống");

                if (!decimal.TryParse(quantityStr, out decimal quantity) || quantity <= 0)
                    rowErrors.Add("Trường 'Số lượng' phải lớn hơn 0");

                if (!decimal.TryParse(priceStr, out decimal price) || price < 0)
                    rowErrors.Add("Trường 'Price' phải >= 0");

                if (!decimal.TryParse(quantityPerBoxStr, out decimal quantityPerBox) || quantityPerBox <= 0)
                    rowErrors.Add("Trường 'Số lượng/thùng' phải lớn hơn 0");

                // Check sản phẩm tồn tại
                if (!productDict.ContainsKey(productCode))
                    rowErrors.Add($"Sản phẩm '{productCode}' không tồn tại trong hệ thống");

                // Check PO + Product trùng
                if (productDict.TryGetValue(productCode, out var product) &&
                    existingKeys.Contains((poNo, product.Id)))
                {
                    rowErrors.Add($"Số PO '{poNo}' và Mã sản phẩm '{productCode}' đã có trên hệ thống");
                }

                // Gom tất cả lỗi của dòng lại
                if (rowErrors.Any())
                    errors.Add($"Dòng {currentRow}: {string.Join(", ", rowErrors)}");
            }

            if (errors.Any())
                return (false, errors);

            // Insert dữ liệu
            foreach (var row in rows)
            {
                var poNo = row.Cell(1).GetString().Trim();
                var orderDate = DateTime.Parse(row.Cell(2).GetString().Trim());
                var customerName = row.Cell(3).GetString().Trim();
                var productCode = row.Cell(4).GetString().Trim();
                var unit = row.Cell(5).GetString().Trim();
                var quantity = decimal.Parse(row.Cell(6).GetString().Trim());
                var price = decimal.Parse(row.Cell(7).GetString().Trim());
                var quantityPerBox = decimal.Parse(row.Cell(8).GetString().Trim());

                var product = productDict[productCode];

                var saleOut = new SaleOut
                {
                    Id = Guid.NewGuid(),
                    CustomerPoNo = poNo,
                    OrderDate = int.Parse(orderDate.ToString("yyyyMMdd")),
                    CustomerName = customerName,
                    ProductId = product.Id,
                    Quantity = quantity,
                    Price = price,
                    Amount = quantity * price,
                    QuantityPerBox = quantityPerBox,
                    BoxQuantity = Math.Ceiling(quantity / quantityPerBox),
                };

                _context.SaleOuts.Add(saleOut);
                existingKeys.Add((poNo, product.Id));
            }

            await _context.SaveChangesAsync();
            return (true, errors);
        }

        public byte[] GenerateExcelTemplate()
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("SaleOutTemplate");

            ws.Cell(1, 1).Value = "Số PO khách hàng";
            ws.Cell(1, 2).Value = "Ngày đặt hàng (date)";
            ws.Cell(1, 3).Value = "Khách hàng";
            ws.Cell(1, 4).Value = "Mã sản phẩm";
            ws.Cell(1, 5).Value = "Đơn vị tính";
            ws.Cell(1, 6).Value = "Số lượng";
            ws.Cell(1, 7).Value = "Đơn giá";
            ws.Cell(1, 8).Value = "Số lượng/thùng";
            var headerRange = ws.Range("A1:I1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

    }
}

