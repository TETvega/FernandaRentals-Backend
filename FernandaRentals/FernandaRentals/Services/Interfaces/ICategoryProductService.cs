using FernandaRentals.Dtos.CategoriesProduct;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.Products;
using Microsoft.AspNetCore.Mvc;

namespace FernandaRentals.Services.Interfaces
{
    public interface ICategoryProductService
    {
        Task<ResponseDto<CategoryProductDto>> CreateCategoryProductAsync(CategoryProductCreateDto dto);
        Task<ResponseDto<CategoryProductDto>> DeleteCategoryProductAsync(Guid id);
        Task<ResponseDto<CategoryProductDto>> EditCategoryProductAsync(CategoryProductEditDto dto, Guid id);
        Task<ResponseDto<List<CategoryProductDto>>> GetCategoriesProductListAsync();
        Task<ResponseDto<CategoryProductDto>> GetCategoryProductAsync(Guid id);

        
    }
}
