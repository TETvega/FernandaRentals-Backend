using AutoMapper;
using FernandaRentals.Database.Entities;
using FernandaRentals.Dtos.CategoriesProduct.HelperDto;
using FernandaRentals.Dtos.CategoriesProduct;
using FernandaRentals.Dtos.ClientType;
using FernandaRentals.Dtos.Events.Helper_Dto;
using FernandaRentals.Dtos.Events;
using FernandaRentals.Dtos.Notes;
using FernandaRentals.Dtos.Products;
using FernandaRentals.Dtos.Client;

namespace FernandaRentals.Helpers
{
    public class AutoMapperProfile :Profile
    {

        public AutoMapperProfile()
        {
            MapsForProducts();
            MapsForCategoriesProducts();
            MapsForNotes();
            MapsForClientsTypes();
            MapsForEvents();
        }

        private void MapsForEvents()
        {
            CreateMap<EventCreateDto, EventEntity>()
                .ForMember(dest => dest.EventDetails, opt => opt.Ignore());// esta ignorando por que cuando lo crea se guarda solamente los detalles primeros y despues se le anexa

            CreateMap<EventEditDto, EventEntity>()
                .ForMember(dest => dest.EventDetails, opt => opt.Ignore());

            CreateMap<EventEntity, EventDto>()
       .ForMember(dest => dest.EventDetails, opt => opt.MapFrom(src => src.EventDetails))
       .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client));

            CreateMap<ClientEntity, ClientDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.ClientType, opt => opt.MapFrom(src => src.ClientType.Description))
                .ForMember(dest => dest.ClientTypeDiscount, opt => opt.MapFrom(src => src.ClientType.Discount));


            // para poder ver los detalles no se si los podia poner aqui pero los meti
            CreateMap<DetailEntity, DetailDto>()
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));
              //  .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice));
            //.ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));

        }

        private void MapsForProducts()
        {
            CreateMap<ProductEntity, ProductDto>()
           .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category)); // Mapea la propiedad Category para mostrar en un {}

            CreateMap<ProductEntity, ProductDtoForCategoryProduct>(); // Se usó este Dto Helper para evitar mostrar información repetida

            // para las demas normales
            CreateMap<ProductCreateDto, ProductEntity>();
            CreateMap<ProductEditDto, ProductEntity>();
        }

        private void MapsForCategoriesProducts()
        {
            CreateMap<CategoryProductEntity, CategoryProductDto>()
           .ForMember(dest => dest.ProductsOfCategory, opt => opt.Ignore()); // Ignorar la propiedad ProductsOfCategory en 'CategoryProductEntity' 

            CreateMap<CategoryProductEntity, CategoryProductCreateDto>();
            CreateMap<CategoryProductEntity, CategoryProductEditDto>();

            CreateMap<CategoryProductCreateDto, CategoryProductEntity>();
            CreateMap<CategoryProductEditDto, CategoryProductEntity>();
        }

        private void MapsForClientsTypes()
        {
            CreateMap<ClientTypeEntity, ClientTypeDto>();
            CreateMap<ClientTypeEntity, ClientTypeCreateDto>();
            CreateMap<ClientTypeEntity, ClientTypeEditDto>();

            CreateMap<ClientTypeCreateDto, ClientTypeEntity>();
            CreateMap<ClientTypeEditDto, ClientTypeEntity>();
        }

        private void MapsForNotes()
        {
            CreateMap<NoteEntity, NoteDto>();
            CreateMap<NoteCreateDto, NoteEntity>();
            CreateMap<NoteEditDto, NoteEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
