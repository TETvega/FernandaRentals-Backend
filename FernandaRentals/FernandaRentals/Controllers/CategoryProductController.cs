﻿using FernandaRentals.Database.Entities;
using FernandaRentals.Dtos.CategoriesProduct;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.Products;
using FernandaRentals.Services;
using FernandaRentals.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FernandaRentals.Controllers
{
    [ApiController]
    [Route("api/categoriesproducts")]
    public class CategoryProductController : ControllerBase
    {

        private readonly ICategoryProductService _categoryProductService;

        public CategoryProductController(ICategoryProductService categoryProductService)
        {
            _categoryProductService = categoryProductService;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDto<CategoryProductEntity>>> GetAll()
        {
            var response = await _categoryProductService.GetCategoriesProductListAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{Id}")]
        public async Task<ActionResult<ResponseDto<CategoryProductDto>>> Get(Guid id)
        {
            var response = await _categoryProductService.GetCategoryProductAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDto<CategoryProductDto>>> Create(CategoryProductCreateDto dto)
        {
            var response = await _categoryProductService.CreateCategoryProductAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{Id}")]
        public async Task<ActionResult<ResponseDto<CategoryProductDto>>> Edit(CategoryProductEditDto dto, Guid id)
        {
            var response = await _categoryProductService.EditCategoryProductAsync(dto, id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{Id}")]
        public async Task<ActionResult<ResponseDto<CategoryProductDto>>> Delete(Guid id)
        {
            var response = await _categoryProductService.DeleteCategoryProductAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
