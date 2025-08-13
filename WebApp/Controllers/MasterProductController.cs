using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net.Http;
using WebApi.Models.DTOs;
using WebApp.Models.DTOs;
using WebApp.Service;
namespace WebApp.Controllers
{
    public class MasterProductController : Controller
    {
        private readonly IMasterPorductService _productService;
        private readonly IHttpClientFactory _httpClientFactory;

        public MasterProductController(IMasterPorductService productService, IHttpClientFactory httpClientFactory)
        {
            _productService = productService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(string field, string keyword, int page = 1, int pageSize = 10)
        {
            var client = _httpClientFactory.CreateClient();
            var apiUrl = $"https://localhost:44367/api/MasterProduct?field={field}&keyword={keyword}";

            var response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không thể tải danh sách sản phẩm.";
                return View(new List<MasterProductDto>());
            }

            var content = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<MasterProductDto>>(content);
            int totalItems = products.Count;
            var pagedData = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchField = field;
            ViewBag.SearchTerm = keyword;
            return View(pagedData);
        }

        [HttpPost]
        public async Task<IActionResult> Create(MasterProductCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existing = await _productService.GetAllMasterProductsAsync(model.ProductCode);
            if (existing.Any(p => p.ProductCode.Equals(model.ProductCode, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("ProductCode", $"Mã sản phẩm {model.ProductCode} đã tồn tại, vui lòng nhập lại.");
                return View(model); 
            }

            var result = await _productService.CreateMasterProductAsync(model);
            if (result != null)
            {
                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Lỗi khi thêm sản phẩm.");
            return View(model);
        }
        // GET: MasterProduct/ConfirmDelete/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _productService.DeleteAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Xóa sản phẩm thất bại!";
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var products = await _productService.GetAllMasterProductsAsync();
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var updateDto = new MasterProductUpdateDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Specification = product.Specification,
                QuantityPerBox = product.QuantityPerBox ?? 0,
                ProductWeight = product.ProductWeight ?? 0,
            };

            return View(updateDto);
        }

        // POST: MasterProduct/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MasterProductUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return View(updateDto);
            }

            var updatedProduct = await _productService.UpdateMasterProductAsync(updateDto.Id, updateDto);
            if (updatedProduct == null)
            {
                ModelState.AddModelError("", "Không thể cập nhật sản phẩm. Vui lòng thử lại.");
                return View(updateDto);
            }

            TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }
       
        public async Task<IActionResult> DownloadTemplate()
        {
            var fileBytes = await _productService.DownloadTemplateAsync();

            if (fileBytes == null)
            {
                TempData["Error"] = "Không thể tải file mẫu.";
                return RedirectToAction("Index");
            }

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ProductTemplate.xlsx");
        }
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file Excel.");
                return View();
            }

            var (isSuccess, errors) = await _productService.UploadExcelAsync(file);

            if (!isSuccess)
            {
                ViewBag.Errors = errors;
                return View();
            }

            TempData["SuccessMessage"] = "Upload thành công!";
            return RedirectToAction("Index");
        }
    }



}

