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
      
        // GET: api/MasterProduct/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var products = await masterProductService.GetAllMasterProductsAsync();
            var product = products.FirstOrDefault(x => x.Id == id);
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

            var products = await masterProductService.GetAllMasterProductsAsync();
            var exists = products.Any(p =>
                p.ProductCode.Equals(createDto.ProductCode, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                return Conflict(new { message = "Mã sản phẩm đã tồn tại." });
            }

            var createdProduct = await masterProductService.CreateMasterProductAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }
        [HttpGet("check-duplicate/{code}")]
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
                return BadRequest(new { Message = "Upload thất bại", Errors = errors });

            return Ok(new { Message = "Upload dữ liệu thành công" });
        }

    }
}
