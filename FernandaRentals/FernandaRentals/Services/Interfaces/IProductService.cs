﻿using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.Products;

namespace FernandaRentals.Services.Interfaces
{
    public interface IProductService
    {
        Task<ResponseDto<List<ProductDto>>> GetProductsListByCategoryIdAsync(Guid id);
        Task<ResponseDto<ProductDto>> GetProductByIdAsync(Guid id);
        Task<ResponseDto<ProductDto>> CreateProductAsync(ProductCreateDto dto);
        Task<ResponseDto<ProductDto>> EditProductAsync(ProductEditDto dto, Guid id);
        Task<ResponseDto<ProductDto>> DeleteProductAsync(Guid id);
        Task<ResponseDto<PaginationDto<List<ProductDto>>>> GetProductsListAsync(string searchTerm ="", string category ="", int page =1);
    }
}
