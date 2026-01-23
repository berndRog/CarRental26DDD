using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
namespace CarRentalApi._2_Modules.Cars._2_Application.Pricing;

public interface IPricingPolicyCarCategories { 
   PricingQuote Calculate(
      CarCategory category, 
      DateTimeOffset start, 
      DateTimeOffset end
   );
}