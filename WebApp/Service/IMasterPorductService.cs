using WebApi.Models.DTOs;
using WebApp.Models.Entities;

namespace WebApp.Service
{
    public interface IMasterPorductService
    {
        Task<List<MasterProductDto>> GetAllMasterProductsAsync(string? searchKeyword = null);
        Task<MasterProductDto?> CreateMasterProductAsync(MasterProductCreateDto createDto);
        Task<MasterProductDto?> UpdateMasterProductAsync(Guid id, MasterProductUpdateDto updateDto);
        Task DeleteMasterProductAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        bool Exists(string productCode);
        Task<MasterProductDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MasterProductDto>> GetAllAsync();
        Task<byte[]> DownloadTemplateAsync();
        Task<(bool IsSuccess, List<string> Errors)> UploadExcelAsync(IFormFile file);
    }
}
