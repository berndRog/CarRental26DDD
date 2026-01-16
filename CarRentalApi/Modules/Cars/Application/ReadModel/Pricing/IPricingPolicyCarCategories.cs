using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
namespace CarRentalApi.Modules.Cars.Ports.Inbound;

public interface IPricingPolicyCarCategories { 
   PricingQuote Calculate(
      CarCategory category, 
      DateTimeOffset start, 
      DateTimeOffset end
   );
}