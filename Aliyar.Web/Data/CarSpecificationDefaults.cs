using Aliyar.Web.Models;

namespace Aliyar.Web.Data;

public static class CarSpecificationDefaults
{
    public static CarSpecification ForStoreCar(Car car)
    {
        var ageYears = Math.Max(0, DateTime.UtcNow.Year - car.Year);
        var mileage = ageYears * 12_000;

        return new CarSpecification
        {
            CarId = car.Id,
            BodyType = InferBodyType(car),
            Mileage = mileage,
            Color = InferColor(car.Id),
            Doors = 4,
            Seats = 5,
            EngineVolumeLiters = InferEngineVolume(car),
            EnginePowerHp = InferEnginePower(car),
            EngineType = EngineType.Petrol,
            Transmission = TransmissionType.Automatic,
            Drive = InferDrive(car),
            FuelConsumptionL100Km = IsPremiumBrand(car.Make) ? 7.5m : 6.5m,
            EmissionClass = "Euro-5",
            HasAbs = true,
            HasEsp = true,
            HasAirbags = true,
            HasAirConditioning = true,
            HasClimateControl = false,
            HasBluetooth = true,
            HasUsb = true,
            HasNavigation = false,
            HasParkingSensors = true,
            HasRearCamera = false,
            Documents = string.IsNullOrWhiteSpace(car.Vin) ? "ПТС" : $"ПТС, VIN: {car.Vin}",
            OwnerCount = 1,
            Condition = $"Автосалон. {car.Make} {car.Model}, {car.Year} г.",
            UpdatedAtUtc = DateTime.UtcNow,
        };
    }

    private static BodyType InferBodyType(Car car)
    {
        var model = car.Model.ToUpperInvariant();
        if (model.Contains("SPORTAGE", StringComparison.Ordinal)
            || model.Contains("TIGUAN", StringComparison.Ordinal)
            || model.Contains("RAV4", StringComparison.Ordinal)
            || model.Contains("X1", StringComparison.Ordinal)
            || model.Contains("X3", StringComparison.Ordinal)
            || model.Contains("Q5", StringComparison.Ordinal))
            return BodyType.Suv;

        if (model.Contains("GOLF", StringComparison.Ordinal)
            || model.Contains("FOCUS", StringComparison.Ordinal)
            || model.Contains("POLO", StringComparison.Ordinal))
            return BodyType.Hatchback;

        return BodyType.Sedan;
    }

    private static string InferColor(int carId) => (carId % 5) switch
    {
        0 => "Белый",
        1 => "Чёрный",
        2 => "Серебристый",
        3 => "Серый",
        _ => "Синий",
    };

    private static decimal InferEngineVolume(Car car) =>
        IsPremiumBrand(car.Make) ? 2.0m : 1.6m;

    private static int InferEnginePower(Car car) =>
        IsPremiumBrand(car.Make) ? 184 : 130;

    private static CarDriveType InferDrive(Car car) =>
        IsPremiumBrand(car.Make) ? CarDriveType.Rear : CarDriveType.Front;

    private static bool IsPremiumBrand(string make) =>
        make is "BMW" or "Mercedes-Benz" or "Audi";
}
