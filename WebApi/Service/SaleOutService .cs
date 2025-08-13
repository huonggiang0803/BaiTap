using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Reflection;
using WebApi.Data;
using WebApi.Models.DTOs;
using WebApi.Models.Entities;
using WebApi.Service;

namespace WebApi.Services
{
    public class SaleOutService : ISaleOutService
    {
        private readonly ApplicationDbContext _context;

        public SaleOutService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<SaleOutDto>> GetAllSaleOutAsync(string field = null, string keyword = null)
        {
            var query = _context.SaleOuts
                .Include(s => s.Product) // Join MasterProduct
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();

                var fieldMap = new Dictionary<string, Func<SaleOut, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "customerpono", s => s.CustomerPoNo },
            { "orderdate", s => s.OrderDate.ToString() },
            { "customername", s => s.CustomerName },
            { "productcode", s => s.Product.ProductCode },
            { "productname", s => s.Product.ProductName },
            { "unit", s => s.Product.Unit },
            { "quantity", s => s.Quantity.ToString() },
            { "quantityperbox", s => s.QuantityPerBox.ToString() },
            { "boxquantity", s => s.BoxQuantity.ToString() },
            { "price", s => s.Price.ToString() },
            { "amount", s => s.Amount.ToString() }
        };

                if (!string.IsNullOrWhiteSpace(field) && fieldMap.ContainsKey(field))
                {
                    // Tìm theo 1 trường
                    query = query.AsEnumerable()
                                 .Where(s => fieldMap[field](s)?.ToLower().Contains(keyword) ?? false)
                                 .AsQueryable();
                }
                else
                {
                    // Tìm trên nhiều trường string
                    query = query.AsEnumerable()
                                 .Where(s =>
                                     (s.CustomerPoNo?.ToLower().Contains(keyword) ?? false) ||
                                     s.OrderDate.ToString().Contains(keyword) ||
                                     (s.CustomerName?.ToLower().Contains(keyword) ?? false) ||
                                     (s.Product?.ProductCode?.ToLower().Contains(keyword) ?? false) ||
                                     (s.Product?.ProductName?.ToLower().Contains(keyword) ?? false) ||
                                     (s.Product?.Unit?.ToLower().Contains(keyword) ?? false) ||
                                     s.Quantity.ToString().Contains(keyword) ||
                                     s.QuantityPerBox.ToString().Contains(keyword) ||
                                     s.BoxQuantity.ToString().Contains(keyword) ||
                                     s.Price.ToString().Contains(keyword) ||
                                     s.Amount.ToString().Contains(keyword)
                                 )
                                 .AsQueryable();
                }
            }

            return query.Select(s => new SaleOutDto
            {
                Id = s.Id,
                CustomerPoNo = s.CustomerPoNo,
                OrderDate = s.OrderDate,
                CustomerName = s.CustomerName,
                ProductId = s.ProductId,
                ProductCode = s.Product.ProductCode,
                ProductName = s.Product.ProductName,
                Unit = s.Product.Unit,
                Quantity = s.Quantity,
                QuantityPerBox = s.QuantityPerBox,
                BoxQuantity = s.BoxQuantity,
                Price = s.Price,
                Amount = s.Amount
            }).ToList();
        }


        public async Task<bool> IsDuplicatePoNoAsync(string poNo)
        {
            return await _context.SaleOuts.AnyAsync(s => s.CustomerPoNo == poNo);
        }
        public async Task<SaleOutDto> CreateSaleOutAsync(SaleOutCreateDto dto)
        {
            if (await IsDuplicatePoNoAsync(dto.CustomerPoNo))
                throw new Exception("Số PO khách hàng đã tồn tại.");

            var product = await _context.MasterProducts
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
                throw new Exception("Không tìm thấy sản phẩm");

            var entity = new SaleOut
            {
                Id = Guid.NewGuid(),
                CustomerPoNo = dto.CustomerPoNo,
                OrderDate = dto.OrderDate,
                CustomerName = dto.CustomerName,
                ProductId = product.Id,
                Quantity = dto.Quantity,
                Price = dto.Price,
                Amount = dto.Quantity * dto.Price,
                QuantityPerBox = product.QuantityPerBox,
                BoxQuantity = Math.Ceiling(dto.Quantity / product.QuantityPerBox)
            };

            _context.SaleOuts.Add(entity);
            await _context.SaveChangesAsync();

            return new SaleOutDto
            {
                Id = entity.Id,
                CustomerPoNo = entity.CustomerPoNo,
                OrderDate = entity.OrderDate,
                CustomerName = entity.CustomerName,
                ProductId = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Quantity = entity.Quantity,
                QuantityPerBox = product.QuantityPerBox,
                BoxQuantity = entity.BoxQuantity,
                Price = entity.Price,
                Amount = entity.Amount
            };
        }
        public async Task<SaleOutDto> UpdateSaleOutAsync(Guid id, SaleOutUpdateDto dto)
        {
            var saleOut = await _context.SaleOuts
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (saleOut == null)
                throw new Exception("Không tìm thấy SaleOut");

            // Chỉ update Quantity & Price
            saleOut.Quantity = dto.Quantity;
            saleOut.Price = dto.Price;

            // Tính lại Amount và BoxQuantity
            if (saleOut.Product != null && saleOut.Product.QuantityPerBox > 0)
            {
                saleOut.BoxQuantity = Math.Ceiling(dto.Quantity / saleOut.Product.QuantityPerBox);
            }
            else
            {
                saleOut.BoxQuantity = 0;
            }

            saleOut.Amount = dto.Quantity * dto.Price;

            await _context.SaveChangesAsync();

            return new SaleOutDto
            {
                Id = saleOut.Id,
                CustomerPoNo = saleOut.CustomerPoNo,
                OrderDate = saleOut.OrderDate,
                CustomerName = saleOut.CustomerName,
                ProductId = saleOut.ProductId,
                ProductCode = saleOut.Product?.ProductCode,
                ProductName = saleOut.Product?.ProductName,
                Unit = saleOut.Product?.Unit,
                Quantity = saleOut.Quantity,
                QuantityPerBox = saleOut.Product?.QuantityPerBox ?? 0,
                BoxQuantity = saleOut.BoxQuantity,
                Price = saleOut.Price,
                Amount = saleOut.Amount
            };
        }




    }
}
