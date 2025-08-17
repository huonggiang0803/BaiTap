using WebApi.Models.DTOs;
using WebApp.Models.Entities;

namespace WebApp.Service
{
    public interface IMasterPorductService
    {
        Task<MasterProductDto?> CreateMasterProductAsync(MasterProductCreateDto createDto);
        Task<bool> CheckDuplicateAsync(string code);
        Task<MasterProductDto?> UpdateMasterProductAsync(Guid id, MasterProductUpdateDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<MasterProductDto> GetByIdAsync(Guid id);
        Task<byte[]> DownloadTemplateAsync();
        Task<(bool IsSuccess, List<string> Errors)> UploadExcelAsync(IFormFile file);
    }
}
