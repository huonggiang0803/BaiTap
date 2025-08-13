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
       
    }
}
