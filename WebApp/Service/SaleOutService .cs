using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using WebApi.Models.DTOs;
using WebApp.Models.DTOs;
using WebApp.Service;

public class SaleOutService : ISaleOutService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory httpClientFactory;
    public SaleOutService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://localhost:44367/"); // base URL API
    }
    public async Task<List<SaleOutDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/SaleOut");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        // Dữ liệu json sẽ được deserialize thành danh sách SaleOutDto
        return JsonConvert.DeserializeObject<List<SaleOutDto>>(json);
    }

    // Lấy dropdown sản phẩm từ API
    public async Task<List<ProductDropdownDto>> GetProductDropdownAsync()
    {
        var response = await _httpClient.GetAsync("api/SaleOut/dropdown");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var products = JsonConvert.DeserializeObject<List<ProductDropdownDto>>(json);

        return products ?? new List<ProductDropdownDto>();
    }

    public async Task<bool> CheckDuplicatePoNoAsync(string poNo)
    {
        var res = await _httpClient.GetAsync($"api/SaleOut/check-duplicate?poNo={poNo}");
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<bool>(json);
    }

    public async Task<SaleOutDto?> CreateSaleOutAsync(SaleOutCreateDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/SaleOut", dto);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<SaleOutDto>();
    }

    public async Task<SaleOutDto?> GetByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/SaleOut/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<SaleOutDto>(json);
    }

    public async Task<bool> UpdateAsync(SaleOutUpdateDto dto)
    {
        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"api/SaleOut/{dto.Id}", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var res = await _httpClient.DeleteAsync($"api/SaleOut/{id}");
        return res.IsSuccessStatusCode;
    }

    public async Task<byte[]?> DownloadTemplateAsync()
    {
        var res = await _httpClient.GetAsync("api/SaleOut/download-template");
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadAsByteArrayAsync();
    }

    public async Task<(bool IsSuccess, List<string> Errors)> UploadExcelAsync(IFormFile file)
    {
        using var form = new MultipartFormDataContent();
        var streamContent = new StreamContent(file.OpenReadStream());
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        form.Add(streamContent, "file", file.FileName);

        var response = await _httpClient.PostAsync("api/SaleOut/upload", form);
        var contentType = response.Content.Headers.ContentType?.MediaType;

        if (response.IsSuccessStatusCode)
        {
            if (contentType == "application/json")
            {
                return (true, new List<string>());
            }
            else if (contentType == "text/plain")
            {
                var errorText = await response.Content.ReadAsStringAsync();
                var errors = errorText.Split(Environment.NewLine).ToList();
                return (false, errors);
            }
        }

        var json = await response.Content.ReadAsStringAsync();
        var errorObj = JsonConvert.DeserializeObject<UploadErrorResponse>(json);
        return (false, errorObj.Errors);
    }

    public async Task<byte[]?> DownloadRevenueReportAsync(int startDate, int endDate)
    {
        var url = $"api/SaleOut/revenue-report?startDate={startDate}&endDate={endDate}";
        var res = await _httpClient.GetAsync(url);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadAsByteArrayAsync();
    }

    public bool Exists(string productCode)
    {
        throw new NotImplementedException();
    }

}