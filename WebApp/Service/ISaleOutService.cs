using WebApp.Models.DTOs;

namespace WebApp.Service
{
    public interface ISaleOutService
    {
        Task<List<ProductDropdownDto>> GetProductDropdownAsync();
        Task<bool> CheckDuplicatePoNoAsync(string poNo);
        Task<SaleOutDto?> CreateSaleOutAsync(SaleOutCreateDto dto);
        Task<List<SaleOutDto>> GetAllAsync();
    }
}
