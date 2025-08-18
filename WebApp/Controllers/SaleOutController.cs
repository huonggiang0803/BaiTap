using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using WebApp.Models.DTOs;
using WebApp.Service;

namespace WebApp.Controllers
{
    public class SaleOutController : Controller
    {
        private readonly ISaleOutService _saleOutService;
        private readonly IHttpClientFactory _httpClientFactory;


        public SaleOutController(ISaleOutService saleOutService, IHttpClientFactory httpClientFactory)
        {
            _saleOutService = saleOutService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(string field, string keyword, int page = 1, int pageSize = 10)
        {
            var client = _httpClientFactory.CreateClient();
            var apiUrl = $"https://localhost:44367/api/SaleOut?field={field}&keyword={keyword}";

            var response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không thể tải danh sách sản phẩm.";
                return View(new List<SaleOutDto>());
            }

            var content = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<SaleOutDto>>(content);
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
        public async Task<IActionResult> CheckDuplicatePoNo(string poNo)
        {
            var exists = await _saleOutService.CheckDuplicatePoNoAsync(poNo);
            return Json(new { exists });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var products = await _saleOutService.GetProductDropdownAsync();
            ViewBag.Products = products;
            return PartialView("Create", new SaleOutCreateDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaleOutCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerPoNo) ||
               dto.OrderDate <= 0 ||
               string.IsNullOrWhiteSpace(dto.CustomerName) ||
               dto.ProductId == Guid.Empty ||
               string.IsNullOrWhiteSpace(dto.Unit) ||
               dto.Price <= 0 ||
               dto.Quantity <= 0)
            {
            return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });
            }
            var duplicate = await _saleOutService.CheckDuplicatePoNoAsync(dto.CustomerPoNo);
            if (duplicate)
            {
                return Json(new { success = false, message = "Số PO khách hàng đã tồn tại." });
            }

            if (!ModelState.IsValid)
            {
                var products = await _saleOutService.GetProductDropdownAsync();
                ViewBag.Products = products;
                return PartialView("Create", dto);
            }
            // Lưu dữ liệu
            var newRecord = await _saleOutService.CreateSaleOutAsync(dto);

            if (newRecord != null)
            {
                return Json(new
                {
                    success = true,
                    data = newRecord
                });
            }
            var productsReload = await _saleOutService.GetProductDropdownAsync();
            ViewBag.Products = productsReload;
            return PartialView("Create", dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return BadRequest();

            var saleOut = await _saleOutService.GetByIdAsync(id.Value);
            if (saleOut == null) return NotFound();

            var updateDto = new SaleOutUpdateDto
            {
                Id = saleOut.Id,
                CustomerPoNo = saleOut.CustomerPoNo,
                OrderDate = saleOut.OrderDate,
                CustomerName = saleOut.CustomerName,
                ProductCode = saleOut.ProductCode,
                ProductName = saleOut.ProductName,
                Unit = saleOut.Unit,
                Quantity = saleOut.Quantity,
                QuantityPerBox = saleOut.QuantityPerBox,
                Price = saleOut.Price
            };

            return PartialView("Edit", updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _saleOutService.DeleteAsync(id);

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

        [HttpPost]
        public async Task<IActionResult> Edit(SaleOutUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return PartialView("Edit", dto);

            var success = await _saleOutService.UpdateAsync(dto);
            if (!success)
            {
                ModelState.AddModelError("", "Cập nhật thất bại!");
                return PartialView("Edit", dto);
            }

            return Json(new { success = true, message = "Cập nhật thành công" });
        }


        public async Task<IActionResult> DownloadTemplate()
        {
            var fileBytes = await _saleOutService.DownloadTemplateAsync();

            if (fileBytes == null)
            {
                TempData["Error"] = "Không thể tải file mẫu.";
                return RedirectToAction("Index");
            }

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "SaleOutTemp.xlsx");
        }
        //[HttpPost]
        //public async Task<IActionResult> Upload(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        ModelState.AddModelError("", "Vui lòng chọn file Excel.");
        //        return View();
        //    }

        //    var (isSuccess, errors) = await _saleOutService.UploadExcelAsync(file);

        //    if (!isSuccess)
        //    {
        //        ViewBag.Errors = errors;
        //        return View();
        //    }

        //    TempData["SuccessMessage"] = "Upload thành công!";
        //    return RedirectToAction("Index");
        //}
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var (isSuccess, errors) = await _saleOutService.UploadExcelAsync(file);

            if (errors.Any())
            {
                TempData["UploadErrors"] = string.Join(";", errors);
            }
            else
            {
                TempData["SuccessMessage"] = "Upload thành công!";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueReport(int startDate, int endDate)
        {
            var fileBytes = await _saleOutService.DownloadRevenueReportAsync(startDate, endDate);
            if (fileBytes == null || fileBytes.Length == 0)
                return NotFound("Không tạo được báo cáo");

            return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"BaoCaoDoanhThu_{startDate}_{endDate}.xlsx");
        }
    }
}
