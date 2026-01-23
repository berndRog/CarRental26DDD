using CarRentalApi._2_Modules.Cars._3_Domain.Enums;
using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
namespace CarRentalApi._2_Modules.Cars._2_Application.Dtos.Contracts;

public sealed record CarContractDto(
   Guid CarId,
   string Manufacturer,
   string Model,
   string LicensePlate,
   CarCategory Category,
   CarStatus Status
);


