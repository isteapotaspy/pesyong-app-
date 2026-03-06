using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using PESYONG.ApplicationLogic.ViewModels.ObjectModels;
using PESYONG.Domain.Entities.Meals.MealItem;

namespace PESYONG.Presentation.Profiler;

public class MealMappingProfile : Profile
{
    public MealMappingProfile()
    {
        // Meal → MealViewModel
        CreateMap<Meal, MealViewModel>()
            .ForMember(dest => dest.MealID, opt => opt.MapFrom(src => src.MealID))
            .ForMember(dest => dest.OperatorID, opt => opt.MapFrom(src => src.OperatorID))
            .ForMember(dest => dest.MealTags, opt => opt.MapFrom(src =>
                new ObservableCollection<string>(src.MealTags))) // Convert ICollection to ObservableCollection
            .ForMember(dest => dest.MealName, opt => opt.MapFrom(src => src.MealName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.MealPrice, opt => opt.MapFrom(src => src.MealPrice))
            .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.StockQuantity))
            .ForMember(dest => dest.MinOrderQuantity, opt => opt.MapFrom(src => src.MinOrderQuantity))
            .ForMember(dest => dest.DeliveryType, opt => opt.MapFrom(src => src.DeliveryType))
            .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreationDate))
            .ForMember(dest => dest.LastModifiedByOperatorID, opt => opt.MapFrom(src => src.LastModifiedByOperatorID))
            .ForMember(dest => dest.LastModifiedDate, opt => opt.MapFrom(src => src.LastModifiedDate))
            .ForMember(dest => dest.ImageSourceString, opt => opt.MapFrom(src => src.ImageSourceString))
            // Ignore properties that shouldn't come from entity
            .ForMember(dest => dest.SelectedTags, opt => opt.Ignore())
            .ForMember(dest => dest.AvailableTags, opt => opt.Ignore())
            .ForMember(dest => dest.ImageBytes, opt => opt.Ignore())
            .ForMember(dest => dest.HasValidationErrors, opt => opt.Ignore())
            .ForMember(dest => dest.ValidationErrors, opt => opt.Ignore())
            .ForMember(dest => dest.SaveCommand, opt => opt.Ignore())
            .ForMember(dest => dest.LoadCommand, opt => opt.Ignore())
            .ForMember(dest => dest.DeleteCommand, opt => opt.Ignore())
            .ForMember(dest => dest.AddTagCommand, opt => opt.Ignore())
            .ForMember(dest => dest.RemoveTagCommand, opt => opt.Ignore())
            .ForMember(dest => dest.UploadImageCommand, opt => opt.Ignore());

        // MealViewModel → Meal
        CreateMap<MealViewModel, Meal>()
            .ForMember(dest => dest.MealID, opt => opt.MapFrom(src => src.MealID))
            .ForMember(dest => dest.OperatorID, opt => opt.MapFrom(src => src.OperatorID ?? 0)) // Handle nullable
            .ForMember(dest => dest.MealTags, opt => opt.MapFrom(src =>
                src.MealTags.ToList()))  // Convert ObservableCollection to List
            .ForMember(dest => dest.MealName, opt => opt.MapFrom(src => src.MealName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.Description) ? null : src.Description))
            .ForMember(dest => dest.MealPrice, opt => opt.MapFrom(src => src.MealPrice))
            .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.StockQuantity))
            .ForMember(dest => dest.MinOrderQuantity, opt => opt.MapFrom(src => src.MinOrderQuantity))
            .ForMember(dest => dest.DeliveryType, opt => opt.MapFrom(src => src.DeliveryType))
            .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreationDate))
            .ForMember(dest => dest.LastModifiedByOperatorID, opt => opt.MapFrom(src => src.LastModifiedByOperatorID))
            .ForMember(dest => dest.LastModifiedDate, opt => opt.MapFrom(src => src.LastModifiedDate))
            .ForMember(dest => dest.ImageSourceString, opt => opt.MapFrom(src => src.ImageSourceString))
            // Ignore navigation properties and methods
            //.ForMember(dest => dest.Operator, opt => opt.Ignore())
            //.ForMember(dest => dest.ModifiedByOperator, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                // Ensure MealID is preserved (important for updates)
                if (src.MealID.HasValue)
                {
                    dest.MealID = src.MealID.Value;
                }
            });
    }
}