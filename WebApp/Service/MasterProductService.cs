using ClosedXML.Excel;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using WebApi.Models.DTOs;
using WebApp.Models.DTOs;
using WebApp.Models.Entities;

namespace WebApp.Service
{
    public class MasterProductService : IMasterPorductService
    {
        private readonly HttpClient httpClient;

        public MasterProductService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.httpClient.BaseAddress = new Uri("https://localhost:44367/api/");
        }
        public async Task<IEnumerable<MasterProductDto>> GetAllMasterProductsAsync()
        {
            // gọi API dropdown (theo API bạn đã viết: GET api/MasterProduct/dropdown)
            var resp = await httpClient.GetAsync("api/MasterProduct/dropdown");
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            // map JSON -> MasterProductDto (chú ý cấu trúc JSON trả về)
            return JsonConvert.DeserializeObject<IEnumerable<MasterProductDto>>(json);
        }

        // Lấy danh sách sản phẩm
        public async Task<List<MasterProductDto>> GetAllMasterProductsAsync(string? searchKeyword = null)
        {
            string url = "MasterProduct";
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                url += $"?searchKeyword={Uri.EscapeDataString(searchKeyword)}";
            }

            var products = await httpClient.GetFromJsonAsync<List<MasterProductDto>>(url);
            return products ?? new List<MasterProductDto>();
        }

        // Thêm sản phẩm
        public async Task<MasterProductDto?> CreateMasterProductAsync(MasterProductCreateDto createDto)
        {
            var response = await httpClient.PostAsJsonAsync("MasterProduct", createDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MasterProductDto>();
            }
            return null;
        }

        // Sửa sản phẩm
        public async Task<MasterProductDto?> UpdateMasterProductAsync(Guid id, MasterProductUpdateDto updateDto)
        {
            var response = await httpClient.PutAsJsonAsync($"MasterProduct/{id}", updateDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MasterProductDto>();
            }
            return null;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await httpClient.DeleteAsync($"MasterProduct/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task DeleteMasterProductAsync(Guid id)
        {
            var response = await httpClient.DeleteAsync($"MasterProduct/{id}");
            response.EnsureSuccessStatusCode();
        }

        public bool Exists(string productCode)
        {
            var response = httpClient.GetAsync($"api/MasterProduct/Exists?productCode={productCode}").Result;
            if (!response.IsSuccessStatusCode) return false;
            var result = response.Content.ReadAsStringAsync().Result;
            return bool.TryParse(result, out var exists) && exists;
        }
        public async Task<IEnumerable<MasterProductDto>> GetAllAsync()
        {
            var response = await httpClient.GetAsync("api/MasterProduct");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<MasterProductDto>>(json);
        }

        public Task<MasterProductDto> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }
        public async Task<byte[]> DownloadTemplateAsync()
        {
            var response = await httpClient.GetAsync("MasterProduct/download-template");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsByteArrayAsync();
        }
        public async Task<(bool IsSuccess, List<string> Errors)> UploadExcelAsync(IFormFile file)
        {
            using var form = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            form.Add(streamContent, "file", file.FileName);

            var response = await httpClient.PostAsync("MasterProduct/upload", form);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorObj = JsonConvert.DeserializeObject<UploadErrorResponse>(json);
                return (false, errorObj.Errors);
            }

            return (true, new List<string>());
        }
    }
}
