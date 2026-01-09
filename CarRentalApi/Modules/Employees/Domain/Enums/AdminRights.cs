namespace CarRentalApi.Modules.Employees.Domain.Enums;

[Flags]
public enum AdminRights {
   // rights can be combined using bitwise operations for example:
   // AdminRights rights = AdminRights.ManageFleet | AdminRights.Reservations;
   None = 0,
   ViewReports = 1,
   ManageFleet = 2,
   ManageReservations = 4,
   ManageRentals = 8,
   ManageUsers = 16,

}