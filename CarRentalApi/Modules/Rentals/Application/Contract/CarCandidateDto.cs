using CarRentalApi.BuildingBlocks.Enums;
namespace CarRentalApi.Modules.Rentals.Application.Contract;

public sealed record CarCandidateDto(
   Guid CarId,
   CarCategory Category
);
