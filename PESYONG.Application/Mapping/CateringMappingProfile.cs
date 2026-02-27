using AutoMapper;
using PESYONG.Service.DTOs;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Service.Mapping;
public class CateringMappingProfile : Profile
{
    public CateringMappingProfile()
    {
        // Map from your Domain Entity to the UI DTO
        CreateMap<Meal, MealSelectionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MealID))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MealName))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.MealPrice));

        // Map from MealProduct to the Package DTO
        CreateMap<MealProduct, PackageDisplayDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MealProductID))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductName))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.ProductBasePrice));

        CreateMap<MealSelectionDto, MealProductItem>()
         .ForMember(d => d.MealID, o => o.MapFrom(s => s.Id))
         .ForMember(d => d.ItemPrice, o => o.MapFrom(s => s.Price))
         .ForMember(d => d.Quantity, o => o.MapFrom(s => 1));
    }
}