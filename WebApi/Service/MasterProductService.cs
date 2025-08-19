using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Reflection;
using WebApi.Data;
using WebApi.Migrations;
using WebApi.Models.DTOs;
using WebApi.Models.Entities;

namespace WebApi.Service
{
    public class MasterProductService : IMasterProductService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly HttpClient _httpClient;
        public MasterProductService(ApplicationDbContext dbContext, HttpClient httpClient)
        {
            this.dbContext = dbContext;
            _httpClient = httpClient;
        }
        private MasterProductDto MapToDto(WebApi.Models.Entities.MasterProduct e)
        {
            return new MasterProductDto
            {
                Id = e.Id,
                ProductCode = e.ProductCode,
                ProductName = e.ProductName,
                Unit = e.Unit,
                Specification = e.Specification,
                QuantityPerBox = e.QuantityPerBox,
                ProductWeight = e.ProductWeight
            };
        }
        public async Task<MasterProductDto> CreateMasterProductAsync(MasterProductCreateDto createDto)
        {
            var entity = new MasterProduct
            {
                Id = Guid.NewGuid(),
                ProductCode = createDto.ProductCode,
                ProductName = createDto.ProductName,
                Unit = createDto.Unit,
                Specification = createDto.Specification,
                QuantityPerBox = createDto.QuantityPerBox,
                ProductWeight = createDto.ProductWeight
            };

            dbContext.MasterProducts.Add(entity);
            await dbContext.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await dbContext.MasterProducts.FindAsync(id);
            if (entity == null)
                return false;

            dbContext.MasterProducts.Remove(entity);
            await dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<MasterProductDto> UpdateMasterProductAsync(Guid id, MasterProductUpdateDto updateDto)
        {
            var entity = await dbContext.MasterProducts.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm với Id {id}");

            entity.ProductCode = updateDto.ProductCode;
            entity.ProductName = updateDto.ProductName;
            entity.Unit = updateDto.Unit;
            entity.Specification = updateDto.Specification;
            entity.QuantityPerBox = updateDto.QuantityPerBox;
            entity.ProductWeight = updateDto.ProductWeight;

            await dbContext.SaveChangesAsync();
            return MapToDto(entity);
        }

        public async Task<bool> CheckDuplicateAsync(string code)
        {
            return await dbContext.MasterProducts
                .AnyAsync(p => p.ProductCode == code);
        }

        public async Task<IEnumerable<MasterProductDto>> GetAllMasterProductsAsync(string field = null, string keyword = null)
        {
            var query = dbContext.MasterProducts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();

                var fieldMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "productcode", "ProductCode" },
                    { "productname", "ProductName" },
                    { "unit", "Unit" },
                    { "specification", "Specification" },
                    { "quantityperbox", "QuantityPerBox" },
                    { "productweight", "ProductWeight" },
                };

                if (!string.IsNullOrWhiteSpace(field))
                {
                    if (fieldMap.TryGetValue(field, out var mappedField))
                        field = mappedField;

                    var property = typeof(MasterProduct)
                        .GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (property != null && property.PropertyType == typeof(string))
                    {
                        query = query.Where(p =>
                            EF.Functions.Like(
                                EF.Property<string>(p, property.Name).ToLower(),
                                $"%{keyword}%"
                            ));
                    }
                }
                else
                {
                    // Search trong tất cả các cột string
                    query = query.Where(p =>
                        EF.Functions.Like(p.ProductCode.ToLower(), $"%{keyword}%") ||
                        EF.Functions.Like(p.ProductName.ToLower(), $"%{keyword}%") ||
                        EF.Functions.Like(p.Unit.ToLower(), $"%{keyword}%") ||
                        EF.Functions.Like(p.Specification.ToLower(), $"%{keyword}%")
                    );
                }
            }
            return await query
                .Select(p => new MasterProductDto
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    Unit = p.Unit,
                    Specification = p.Specification,
                    QuantityPerBox = p.QuantityPerBox,
                    ProductWeight = p.ProductWeight
                })
                .ToListAsync();
        }
        public async Task<MasterProductDto?> GetByIdAsync(Guid id)
        {
            return await dbContext.MasterProducts
                .Where(p => p.Id == id)
                .Select(p => new MasterProductDto
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    Unit = p.Unit, 
                    Specification = p.Specification,
                    QuantityPerBox = p.QuantityPerBox,
                    ProductWeight = p.ProductWeight
                })
                .FirstOrDefaultAsync();
        }
        public byte[] GenerateExcelTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ProductTemplate");

                worksheet.Cell(1, 1).Value = "ProductCode";
                worksheet.Cell(1, 2).Value = "ProductName";
                worksheet.Cell(1, 3).Value = "Unit";
                worksheet.Cell(1, 4).Value = "Specification";
                worksheet.Cell(1, 5).Value = "QuantityPerBox";
                worksheet.Cell(1, 6).Value = "ProductWeight";

                var headerRange = worksheet.Range("A1:F1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray(); 
                }
            }
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
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // bỏ dòng tiêu đề

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber(); // lấy số dòng trong Excel
                var productCode = row.Cell(1).GetString().Trim();
                var productName = row.Cell(2).GetString().Trim();
                var unit = row.Cell(3).GetString().Trim();
                var specification = row.Cell(4).GetString().Trim();
                var quantityPerBoxStr = row.Cell(5).GetString().Trim();
                var productWeightStr = row.Cell(6).GetString().Trim();

                var rowErrors = new List<string>();

                // Kiểm tra không để trống
                if (string.IsNullOrWhiteSpace(productCode))
                    rowErrors.Add("Trường 'Mã sản phẩm' không được để trống");
                if (string.IsNullOrWhiteSpace(productName))
                    rowErrors.Add("Trường 'Tên sản phẩm' không được để trống");
                if (string.IsNullOrWhiteSpace(unit))
                    rowErrors.Add("Trường 'Đơn vị' không được để trống");
                if (string.IsNullOrWhiteSpace(specification))
                    rowErrors.Add("Trường 'Quy cách' không được để trống");
                if (string.IsNullOrWhiteSpace(quantityPerBoxStr))
                    rowErrors.Add("Trường 'Số lượng/Thùng' không được để trống");
                if (string.IsNullOrWhiteSpace(productWeightStr))
                    rowErrors.Add("Trường 'Trọng lượng' không được để trống");

                // Kiểm tra kiểu dữ liệu decimal
                if (!string.IsNullOrWhiteSpace(quantityPerBoxStr) && !decimal.TryParse(quantityPerBoxStr, out _))
                    rowErrors.Add("Trường 'Số lượng/Thùng' phải là số hợp lệ");
                if (!string.IsNullOrWhiteSpace(productWeightStr) && !decimal.TryParse(productWeightStr, out _))
                    rowErrors.Add("Trường 'Trọng lượng' phải là số hợp lệ");

                // Kiểm tra trùng mã sản phẩm trong DB
                if (!string.IsNullOrWhiteSpace(productCode) &&
                    await dbContext.MasterProducts.AnyAsync(p => p.ProductCode == productCode))
                {
                    rowErrors.Add($"Mã sản phẩm '{productCode}' đã có trên hệ thống");
                }

                // Nếu có lỗi của dòng, gom thành 1 chuỗi và thêm vào errors
                if (rowErrors.Any())
                    errors.Add($"Dòng {rowNumber}: {string.Join(", ", rowErrors)}");
            }

            // Nếu có lỗi, trả về luôn mà không thêm dữ liệu
            if (errors.Any())
                return (false, errors);

            // Thêm dữ liệu vào DB
            foreach (var row in rows)
            {
                var product = new MasterProduct
                {
                    Id = Guid.NewGuid(),
                    ProductCode = row.Cell(1).GetString().Trim(),
                    ProductName = row.Cell(2).GetString().Trim(),
                    Unit = row.Cell(3).GetString().Trim(),
                    Specification = row.Cell(4).GetString().Trim(),
                    QuantityPerBox = decimal.Parse(row.Cell(5).GetString().Trim()),
                    ProductWeight = decimal.Parse(row.Cell(6).GetString().Trim())
                };

                dbContext.MasterProducts.Add(product);
            }

            await dbContext.SaveChangesAsync();
            return (true, errors);
        }

    }
}

