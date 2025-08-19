using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Migrations;
using WebApi.Models.DTOs;
using WebApi.Service;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterProductController : ControllerBase
    {
        private readonly IMasterProductService masterProductService;
        private readonly ApplicationDbContext _context;
        public MasterProductController(IMasterProductService masterProductService, ApplicationDbContext context)
        {
            this.masterProductService = masterProductService;
            _context = context;

        }

        // GET: api/MasterProduct
        [HttpGet]
        public async Task<IActionResult> GetAll(string field = null, string keyword = null)
        {
            var products = await masterProductService.GetAllMasterProductsAsync(field, keyword);
            return Ok(products);
        }

        //GET: api/MasterProduct/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await masterProductService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // POST: api/MasterProduct
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MasterProductCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool isDuplicate = await masterProductService.CheckDuplicateAsync(createDto.ProductCode);
            if (isDuplicate)
            {
                return Conflict(new { productCode = $"Mã sản phẩm '{createDto.ProductCode}' đã tồn tại." });
            }

            var product = await masterProductService.CreateMasterProductAsync(createDto);
            return CreatedAtAction(nameof(Create), new { id = product.Id }, product);
        }
        [HttpGet("check-duplicate")]
        public async Task<IActionResult> CheckDuplicate(string code)
        {
            var isDuplicate = await masterProductService.CheckDuplicateAsync(code);
            return Ok(isDuplicate);
        }

        // PUT: api/MasterProduct/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MasterProductUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedProduct = await masterProductService.UpdateMasterProductAsync(id, updateDto);
            if (updatedProduct == null)
                return NotFound();

            return Ok(updatedProduct);
        }

        // DELETE: api/MasterProduct/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await masterProductService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        [HttpGet("download-template")]
        public IActionResult DownloadTemplate()
        {
            var fileBytes = masterProductService.GenerateExcelTemplate();
            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ProductTemplate.xlsx");
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            var (isSuccess, errors) = await masterProductService.ImportFromExcelAsync(file);

            if (!isSuccess)
            {
                var errorText = string.Join(Environment.NewLine, errors);
                // Chuyển chuỗi thành byte để trả về file
                var errorBytes = System.Text.Encoding.UTF8.GetBytes(errorText);
                var fileName = $"UploadErrors_{DateTime.Now:yyyyMMdd}.txt";
                // Trả về file txt để client tải
                return File(errorBytes, "text/plain", fileName);
            }

            return Ok(new { Message = "Upload dữ liệu thành công" });
        }

    }
}
