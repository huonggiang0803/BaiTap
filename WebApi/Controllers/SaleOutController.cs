using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using WebApi.Data;
using WebApi.Models.DTOs;
using WebApi.Service;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaleOutController : ControllerBase
    {
        private readonly ISaleOutService _saleOutService;
        private readonly ApplicationDbContext _context;
        public SaleOutController(ISaleOutService saleOutService, ApplicationDbContext context)
        {
            _saleOutService = saleOutService;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(string field = null, string keyword = null)
        {
            var products = await _saleOutService.GetAllSaleOutAsync(field, keyword);
            return Ok(products);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaleOutCreateDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

            try
            {
                var createdSaleOut = await _saleOutService.CreateSaleOutAsync(dto);

                if (createdSaleOut == null)
                {
                    return Ok(new { success = false, message = "Không thể lưu SaleOut vào DB" });
                }

                return Ok(new { success = true, data = createdSaleOut });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("dropdown")]
        public async Task<ActionResult<IEnumerable<object>>> GetDropdown()
        {
            var products = await _context.MasterProducts
                .Select(p => new
                {
                    Id = p.Id,
                    Code = p.ProductCode,
                    DisplayName = p.ProductCode + " (" + p.ProductName + ")",
                    Unit = p.Unit,
                    QuantityPerBox = p.QuantityPerBox
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("code/{productCode}")]
        public async Task<ActionResult<MasterProductDto>> GetByCode(string productCode)
        {
            var product = await _context.MasterProducts
                .Where(p => p.ProductCode == productCode)
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

            if (product == null)
                return NotFound();

            return Ok(product);
        }
        [HttpGet("check-duplicate")]
        public async Task<IActionResult> CheckDuplicate(string poNo)
        {
            var exists = await _saleOutService.IsDuplicatePoNoAsync(poNo);
            return Ok(exists);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var products = await _saleOutService.GetAllSaleOutAsync();
            var product = products.FirstOrDefault(x => x.Id == id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _saleOutService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSaleOut(Guid id, [FromBody] SaleOutUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _saleOutService.UpdateSaleOutAsync(id, dto);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("download-template")]
        public IActionResult DownloadTemplate()
        {
            var fileBytes = _saleOutService.GenerateExcelTemplate();
            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "SaleOutTemplate.xlsx");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            var (isSuccess, errors) = await _saleOutService.ImportFromExcelAsync(file);

            if (!isSuccess)
                return BadRequest(new { Message = "Upload thất bại", Errors = errors });

            return Ok(new { Message = "Upload dữ liệu thành công" });
        }

        [HttpGet("revenue-report")]
        public async Task<IActionResult> GetRevenueReportExcel(int startDate, int endDate)
        {
            var reportData = await _saleOutService.GetSaleOutReportAsync(startDate, endDate);

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Revenue Report");

                worksheet.Cell(1, 1).Value = "Mã sản phẩm";
                worksheet.Cell(1, 2).Value = "Tên sản phẩm";
                worksheet.Cell(1, 3).Value = "Số lượng";
                worksheet.Cell(1, 4).Value = "Đơn giá";
                worksheet.Cell(1, 5).Value = "Thành tiền";

                // Fill data
                int row = 2;
                foreach (var item in reportData)
                {
                    worksheet.Cell(row, 1).Value = item.ProductCode;
                    worksheet.Cell(row, 2).Value = item.ProductName;
                    worksheet.Cell(row, 3).Value = item.Quantity;
                    worksheet.Cell(row, 4).Value = item.Price;
                    worksheet.Cell(row, 5).Value = item.Amount;
                    row++;
                }
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "RevenueReport.xlsx");
                }
            }
        }

    }
}
