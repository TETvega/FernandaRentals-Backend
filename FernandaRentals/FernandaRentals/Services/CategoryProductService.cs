using AutoMapper;
using FernandaRentals.Database;
using FernandaRentals.Database.Entities;
using FernandaRentals.Dtos.CategoriesProduct;
using FernandaRentals.Dtos.CategoriesProduct.HelperDto;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.Products;
using FernandaRentals.Services.Interfaces;
using InmobiliariaUNAH.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FernandaRentals.Services
{
    public class CategoryProductService : ICategoryProductService
    {
        private readonly FernandaRentalsContext _context;
        private readonly IMapper _mapper;

        public CategoryProductService(FernandaRentalsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ResponseDto<List<CategoryProductDto>>> GetCategoriesProductListAsync()
        {
            var categoriesproductsEntity = await _context.CategoryProducts.ToArrayAsync();


            var categoriesproductsDtos = _mapper.Map<List<CategoryProductDto>>(categoriesproductsEntity);

            return new ResponseDto<List<CategoryProductDto>>
            {
                StatusCode = 200,
                Status = true,
                Message = "Listado de categorias de producto obtenida correctamente.",
                Data = categoriesproductsDtos
            };
        }

        public async Task<ResponseDto<CategoryProductDto>> GetCategoryProductAsync(Guid id)
        {
            var categoryProductEntity = await _context.CategoryProducts.FirstOrDefaultAsync(p => p.Id == id);

            if (categoryProductEntity == null) return ResponseHelper.ResponseError<CategoryProductDto>(404, "No se encontró la categoría del producto");

            var productsEntitiesByCategoryId = await _context.Products.Where(p => p.CategoryId == categoryProductEntity.Id).ToListAsync();

            var productsForCategoriesDto = _mapper.Map<List<ProductDtoForCategoryProduct>>(productsEntitiesByCategoryId);

            categoryProductEntity.ProductsOfCategory = productsForCategoriesDto;

            var categoryProductDto = _mapper.Map<CategoryProductDto>(categoryProductEntity);

            return new ResponseDto<CategoryProductDto>
            {
                StatusCode = 200,
                Status = true,
                Message = "Listado de categorias de productos obtenida correctamente",
                Data = categoryProductDto
            };
        }
        public async Task<ResponseDto<CategoryProductDto>> CreateCategoryProductAsync(CategoryProductCreateDto dto)
        {
           
            var categoryProductEntity = _mapper.Map<CategoryProductEntity>(dto);
            _context.CategoryProducts.Add(categoryProductEntity);
            await _context.SaveChangesAsync();

            var categoryProductDto = _mapper.Map<CategoryProductDto>(categoryProductEntity);

            return new ResponseDto<CategoryProductDto>
            {
                StatusCode = 201,
                Status = true,
                Message = "Producto creado correctamente.",
                Data = categoryProductDto,
            };

        }

        public async Task<ResponseDto<CategoryProductDto>> EditCategoryProductAsync(CategoryProductEditDto dto, Guid id)
        {
            var categoryProductEntity = await _context.CategoryProducts.FirstOrDefaultAsync(c => c.Id == id);
            if (categoryProductEntity == null)
            {
                return new ResponseDto<CategoryProductDto>
                {
                    StatusCode = 404,
                    Status = false,
                    Message = "No se encontró el producto la categoría de producto especificada.",
                };
            }

            _mapper.Map<CategoryProductEditDto, CategoryProductEntity>(dto, categoryProductEntity);
            _context.CategoryProducts.Update(categoryProductEntity);
            await _context.SaveChangesAsync();

            var categoryProductDto = _mapper.Map<CategoryProductDto>(categoryProductEntity);

            return new ResponseDto<CategoryProductDto>
            {
                StatusCode = 200,
                Status = true,
                Message = "Categoría del Producto modificado correctamente.",
                Data = categoryProductDto
            };
        }
        public async Task<ResponseDto<CategoryProductDto>> DeleteCategoryProductAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var categoryProductEntity = await _context.CategoryProducts.FirstOrDefaultAsync(p => p.Id == id);
                if (categoryProductEntity is null) return ResponseHelper.ResponseError<CategoryProductDto>(404, "No se encontró la categoría del producto");          

                //  productos asociados a la categoria a eliminar
                var productsOfCategory = await _context.Products.Where(p => p.CategoryId == id).ToListAsync();
                

                // ids de los productos de esta categoría.
                var productIds = productsOfCategory.Select(p => p.Id).ToList();

                // Eliminar los registros en EventDetails relacionados con los productos
                var eventDetails = await _context.Details
                    .Where(ed => productIds.Contains(ed.ProductId))
                    .ToListAsync();

                _context.Details.RemoveRange(eventDetails);

                // Eliminar los registros en ReservationEntity relacionados con los productos
                var reservations = await _context.Reservations
                    .Where(r => productIds.Contains(r.ProductId))
                    .ToListAsync();

                _context.Reservations.RemoveRange(reservations);

                // Eliminar los productos de la categoría.
                var productsToDelete = await _context.Products
                    .Where(p => p.CategoryId == id)
                    .ToListAsync();

                _context.Products.RemoveRange(productsToDelete);

                // Verificar si los eventos asociados a los productos eliminados son los últimos eventos con productos.
                var eventIdsToCheck = eventDetails.Select(ed => ed.EventId).Distinct().ToList();

                foreach (var eventId in eventIdsToCheck)
                {
                    // Verificar si el evento tiene más productos asociados después de eliminar los productos de esta categoría.
                    var remainingProductsInEvent = await _context.Details
                        .Where(ed => ed.EventId == eventId)
                        .Where(ed => !productIds.Contains(ed.ProductId)) // Excluir los productos eliminados
                        .ToListAsync();

                    if (remainingProductsInEvent.Count == 0) // Si no quedan productos asociados, eliminar el evento.
                    {
                        var eventToDelete = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
                        if (eventToDelete != null)
                        {
                            _context.Events.Remove(eventToDelete);
                        }
                    }
                }

                _context.CategoryProducts.Remove(categoryProductEntity); // ahora si se elimno

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return ResponseHelper.ResponseSuccess<CategoryProductDto>(200, "Categoría de producto y sus registros asociados eliminados correctamente.");
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                return ResponseHelper.ResponseError<CategoryProductDto>(500, $"Ocurrió un error al eliminar la categoría de producto: {e.Message}");
            }
        }



    }
}
