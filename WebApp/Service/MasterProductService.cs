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
        { var response = await httpClient.PutAsJsonAsync($"MasterProduct/{id}", updateDto);
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

        public async Task<MasterProductDto> GetByIdAsync(Guid id) 
        { var response = await httpClient.GetAsync($"MasterProduct/{id}");
            if (!response.IsSuccessStatusCode) return null; 
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MasterProductDto>(json); }

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

        public async Task<bool> CheckDuplicateAsync(string productCode)
        {
            var response = await httpClient.GetAsync($"MasterProduct/check-duplicate?code={productCode}");
            if (!response.IsSuccessStatusCode) return false;

            var content = await response.Content.ReadAsStringAsync();
            return bool.Parse(content);
        }

    }
}
