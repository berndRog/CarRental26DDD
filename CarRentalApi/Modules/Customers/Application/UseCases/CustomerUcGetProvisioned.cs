using CarRentalApi.BuildingBlocks.Infrastructure.Persistence;
using CarRentalApi.BuildingBlocks.Ports.Outbound;
using CarRentalApi.Domain;
namespace CarRentalApi.Modules.Customers.Application.UseCases;

public sealed class CustomerUcGetProvisioned(
   ICustomerRepository repo,
   IIdentityGateway identity,
   IUnitOfWork uow
) {
   
}
