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
        [HttpGet]
        public async Task<JsonResult> CheckDuplicate(string productCode)
        {
            var exists = await _productService.CheckDuplicateAsync(productCode);
            return Json(exists);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("Create", new MasterProductCreateDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(MasterProductCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return PartialView("Create", createDto);

            bool isDuplicate = await _productService.CheckDuplicateAsync(createDto.ProductCode);
            if (isDuplicate)
            {
                ModelState.AddModelError("ProductCode", $"Mã sản phẩm '{createDto.ProductCode}' đã tồn tại.");
                return PartialView("Create", createDto); // ✅ Trả về PartialView
            }
            var product = await _productService.CreateMasterProductAsync(createDto);
            return Json(new { success = true, productId = product.Id });
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

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            var updateDto = new MasterProductUpdateDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Specification = product.Specification,
                QuantityPerBox = product.QuantityPerBox ?? 0,
                ProductWeight = product.ProductWeight ?? 0
            };
            return PartialView("Edit", updateDto);
        }
        [HttpPost]
        public async Task<IActionResult>
            Edit(MasterProductUpdateDto updateDto) 
        { if (!ModelState.IsValid) return PartialView("Edit", updateDto);
            var result = await _productService.UpdateMasterProductAsync
                (updateDto.Id, updateDto); if (result != null) 
                return Json(new { success = true }); return Json(new { success = false }); 
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

