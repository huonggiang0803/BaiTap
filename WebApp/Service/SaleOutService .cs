using Newtonsoft.Json;
using System.Net.Http;
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

    public async Task<MasterProductDto> GetProductByCodeAsync(string productCode)
    {
        var response = await _httpClient.GetAsync($"/api/MasterProduct/code/{productCode}");
        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<MasterProductDto>(content);
    }
    public async Task<SaleOutDto> GetSaleOutByIdAsync(Guid id)
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync($"api/SaleOut/{id}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<SaleOutDto>(content);
    }

    public async Task<SaleOutDto> UpdateSaleOutAsync(Guid id, SaleOutUpdateDto dto)
    {
        var client = httpClientFactory.CreateClient();
        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"https://localhost:44367/api/SaleOut/{id}", content);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<SaleOutDto>(result);
    }


}