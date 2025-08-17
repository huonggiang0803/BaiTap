using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models.DTOs;
using WebApi.Models.Entities;

namespace WebApi.Service
{
    public interface IMasterProductService
    {
        Task<IEnumerable<MasterProductDto>> GetAllMasterProductsAsync(string field = null, string keyword = null);
        Task<MasterProductDto> CreateMasterProductAsync(MasterProductCreateDto createDto);
        Task<bool> CheckDuplicateAsync(string code);
        Task<MasterProductDto> UpdateMasterProductAsync(Guid id, MasterProductUpdateDto updateDto);
        //Task DeleteMasterProductAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        byte[] GenerateExcelTemplate();
        Task<(bool IsSuccess, List<string> Errors)> ImportFromExcelAsync(IFormFile file);


    }
}
