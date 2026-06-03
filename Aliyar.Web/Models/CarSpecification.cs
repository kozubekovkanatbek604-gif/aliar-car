namespace Aliyar.Web.Models;

public sealed class CarSpecification
{
    public int Id { get; set; }

    public int CarId { get; set; }

    public Car Car { get; set; } = null!;

    public BodyType BodyType { get; set; }

    public int Mileage { get; set; }

    public string Color { get; set; } = "";

    public int? Doors { get; set; }

    public int? Seats { get; set; }

    public decimal? EngineVolumeLiters { get; set; }

    public int? EnginePowerHp { get; set; }

    public EngineType EngineType { get; set; }

    public TransmissionType Transmission { get; set; }

    public CarDriveType Drive { get; set; }

    public decimal? FuelConsumptionL100Km { get; set; }

    public string? EmissionClass { get; set; }

    public bool HasAbs { get; set; }

    public bool HasEsp { get; set; }

    public bool HasAirbags { get; set; }

    public bool HasAirConditioning { get; set; }

    public bool HasClimateControl { get; set; }

    public bool HasBluetooth { get; set; }

    public bool HasUsb { get; set; }

    public bool HasNavigation { get; set; }

    public bool HasParkingSensors { get; set; }

    public bool HasRearCamera { get; set; }

    public string? Documents { get; set; }

    public int? OwnerCount { get; set; }

    public string? Condition { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public bool HasRequiredFields() =>
        BodyType != BodyType.Unknown
        && Mileage >= 0
        && !string.IsNullOrWhiteSpace(Color)
        && EngineVolumeLiters is > 0
        && EnginePowerHp is > 0
        && EngineType != EngineType.Unknown
        && Transmission != TransmissionType.Unknown
        && Drive != CarDriveType.Unknown;
}
