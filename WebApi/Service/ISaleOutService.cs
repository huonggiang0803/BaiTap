using WebApi.Models.DTOs;

namespace WebApi.Service
{
    public interface ISaleOutService
    {
        Task<IEnumerable<SaleOutDto>> GetAllSaleOutAsync(string field = null, string keyword = null);
        Task<SaleOutDto> CreateSaleOutAsync(SaleOutCreateDto dto);
        Task<bool> IsDuplicatePoNoAsync(string poNo);
        Task<SaleOutDto> UpdateSaleOutAsync(Guid id, SaleOutUpdateDto dto);
    }
}
