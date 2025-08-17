using WebApp.Models.DTOs;

namespace WebApp.Service
{
    public interface ISaleOutService
    {
        Task<List<ProductDropdownDto>> GetProductDropdownAsync();
        Task<bool> CheckDuplicatePoNoAsync(string poNo);
        Task<SaleOutDto?> CreateSaleOutAsync(SaleOutCreateDto dto);
        Task<List<SaleOutDto>> GetAllAsync();
        Task<bool> DeleteAsync(Guid id);
        Task<SaleOutDto?> GetByIdAsync(Guid id);
        Task<bool> UpdateAsync(SaleOutUpdateDto dto);
        Task<byte[]> DownloadTemplateAsync();
        Task<byte[]?> DownloadRevenueReportAsync(int startDate, int endDate);
        Task<(bool IsSuccess, List<string> Errors)> UploadExcelAsync(IFormFile file);
    }
}
