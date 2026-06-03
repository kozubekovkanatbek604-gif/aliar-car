using System.Security.Claims;
using Aliyar.Web.Models;

namespace Aliyar.Web.Security;

public static class AuthorizationExtensions
{
    public static bool CanManageCars(this ClaimsPrincipal user) =>
        user.IsInRole(AppRoles.Admin) || user.IsInRole(AppRoles.Manager);

    public static bool CanUseCart(this ClaimsPrincipal user) =>
        user.CanManageCars() && !user.IsAppAdmin();

    public static bool IsAppAdmin(this ClaimsPrincipal user) =>
        user.IsInRole(AppRoles.Admin);

    public static bool IsCustomer(this ClaimsPrincipal user) =>
        user.IsInRole(AppRoles.Customer);

    public static bool IsClient(this ClaimsPrincipal user) =>
        user.IsInRole(AppRoles.Client);

    /// <summary>Пользователь может размещать и управлять своими объявлениями.</summary>
    public static bool CanPostListings(this ClaimsPrincipal user) =>
        user.IsCustomer() || user.IsClient();

    public static string? GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier);

    public static bool IsCustomerListing(this Car car) =>
        car.Kind == ListingKind.Customer;

    public static bool IsStoreListing(this Car car) =>
        car.Kind == ListingKind.Store;

    public static bool IsOwnedBy(this Car car, ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        return userId is not null
            && car.OwnerUserId is not null
            && car.OwnerUserId == userId;
    }

    public static bool CanManageCustomerListing(this ClaimsPrincipal user, Car car) =>
        car.IsCustomerListing() && car.IsOwnedBy(user);

    public static bool CanDeleteCar(this ClaimsPrincipal user, Car car) =>
        user.IsAppAdmin() || user.CanManageCustomerListing(car);

    public static bool CanArchiveCar(this ClaimsPrincipal user, Car car) =>
        !car.IsArchived && (
            (car.IsStoreListing() && user.IsAppAdmin()) ||
            user.CanManageCustomerListing(car));

    public static bool CanRestoreCar(this ClaimsPrincipal user, Car car) =>
        car.IsArchived && (
            user.IsAppAdmin() ||
            user.CanManageCustomerListing(car));

    public static bool CanEditCarSpecifications(this ClaimsPrincipal user, Car car) =>
        user.CanManageCars() || user.CanManageCustomerListing(car);
}
