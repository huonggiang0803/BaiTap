using WebApi.Models.DTOs;
using WebApp.Models.DTOs;

namespace WebApi.Service
{
    public interface ISaleOutService
    {
        Task<IEnumerable<SaleOutDto>> GetAllSaleOutAsync(string field = null, string keyword = null);
        Task<SaleOutDto> CreateSaleOutAsync(SaleOutCreateDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> IsDuplicatePoNoAsync(string poNo);
        Task<SaleOutDto> UpdateSaleOutAsync(Guid id, SaleOutUpdateDto dto);
        Task<List<SaleOutReportDto>> GetSaleOutReportAsync(int startDate, int endDate);
        Task<(bool IsSuccess, List<string> Errors)> ImportFromExcelAsync(IFormFile file);
        byte[] GenerateExcelTemplate();
    }
}
