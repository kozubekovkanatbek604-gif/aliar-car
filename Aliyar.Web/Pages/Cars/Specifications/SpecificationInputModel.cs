using System.ComponentModel.DataAnnotations;
using Aliyar.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Aliyar.Web.Pages.Cars.Specifications;

public sealed class SpecificationInputModel
{
    [Display(Name = "Тип кузова")]
    public BodyType BodyType { get; set; }

    [Display(Name = "Пробег (км)")]
    public int Mileage { get; set; }

    [StringLength(50)]
    [Display(Name = "Цвет")]
    public string Color { get; set; } = "";

    [Range(2, 7)]
    [Display(Name = "Количество дверей")]
    public int? Doors { get; set; }

    [Range(2, 9)]
    [Display(Name = "Количество мест")]
    public int? Seats { get; set; }

    [Display(Name = "Объём двигателя (л)")]
    public decimal? EngineVolumeLiters { get; set; }

    [Display(Name = "Мощность (л.с.)")]
    public int? EnginePowerHp { get; set; }

    [Display(Name = "Тип двигателя")]
    public EngineType EngineType { get; set; }

    [Display(Name = "Коробка передач")]
    public TransmissionType Transmission { get; set; }

    [Display(Name = "Привод")]
    public CarDriveType Drive { get; set; }

    [Display(Name = "Расход топлива (л/100 км)")]
    public decimal? FuelConsumptionL100Km { get; set; }

    [StringLength(20)]
    [Display(Name = "Экологический класс")]
    public string? EmissionClass { get; set; }

    [Display(Name = "ABS")]
    public bool HasAbs { get; set; }

    [Display(Name = "ESP")]
    public bool HasEsp { get; set; }

    [Display(Name = "Подушки безопасности")]
    public bool HasAirbags { get; set; }

    [Display(Name = "Кондиционер")]
    public bool HasAirConditioning { get; set; }

    [Display(Name = "Климат-контроль")]
    public bool HasClimateControl { get; set; }

    [Display(Name = "Bluetooth")]
    public bool HasBluetooth { get; set; }

    [Display(Name = "USB")]
    public bool HasUsb { get; set; }

    [Display(Name = "Навигация")]
    public bool HasNavigation { get; set; }

    [Display(Name = "Парктроник")]
    public bool HasParkingSensors { get; set; }

    [Display(Name = "Камера заднего вида")]
    public bool HasRearCamera { get; set; }

    [StringLength(300)]
    [Display(Name = "Документы")]
    public string? Documents { get; set; }

    [Range(1, 20)]
    [Display(Name = "Количество владельцев")]
    public int? OwnerCount { get; set; }

    [StringLength(400)]
    [Display(Name = "Состояние")]
    public string? Condition { get; set; }

    public static SpecificationInputModel FromEntity(CarSpecification entity) => new()
    {
        BodyType = entity.BodyType,
        Mileage = entity.Mileage,
        Color = entity.Color,
        Doors = entity.Doors,
        Seats = entity.Seats,
        EngineVolumeLiters = entity.EngineVolumeLiters,
        EnginePowerHp = entity.EnginePowerHp,
        EngineType = entity.EngineType,
        Transmission = entity.Transmission,
        Drive = entity.Drive,
        FuelConsumptionL100Km = entity.FuelConsumptionL100Km,
        EmissionClass = entity.EmissionClass,
        HasAbs = entity.HasAbs,
        HasEsp = entity.HasEsp,
        HasAirbags = entity.HasAirbags,
        HasAirConditioning = entity.HasAirConditioning,
        HasClimateControl = entity.HasClimateControl,
        HasBluetooth = entity.HasBluetooth,
        HasUsb = entity.HasUsb,
        HasNavigation = entity.HasNavigation,
        HasParkingSensors = entity.HasParkingSensors,
        HasRearCamera = entity.HasRearCamera,
        Documents = entity.Documents,
        OwnerCount = entity.OwnerCount,
        Condition = entity.Condition,
    };

    public bool HasRequiredFields() =>
        BodyTypeCatalog.IsSelectable(BodyType)
        && Mileage >= 0
        && !string.IsNullOrWhiteSpace(Color)
        && EngineVolumeLiters is > 0
        && EnginePowerHp is > 0
        && EngineType != EngineType.Unknown
        && Transmission != TransmissionType.Unknown
        && Drive != CarDriveType.Unknown;

    public void AddRequiredFieldErrors(ModelStateDictionary modelState, string prefix)
    {
        if (!BodyTypeCatalog.IsSelectable(BodyType))
            modelState.AddModelError($"{prefix}.{nameof(BodyType)}", "Укажите тип кузова.");

        if (Mileage < 0)
            modelState.AddModelError($"{prefix}.{nameof(Mileage)}", "Укажите пробег.");

        if (string.IsNullOrWhiteSpace(Color))
            modelState.AddModelError($"{prefix}.{nameof(Color)}", "Укажите цвет.");

        if (EngineVolumeLiters is null or <= 0)
            modelState.AddModelError($"{prefix}.{nameof(EngineVolumeLiters)}", "Укажите объём двигателя.");

        if (EnginePowerHp is null or <= 0)
            modelState.AddModelError($"{prefix}.{nameof(EnginePowerHp)}", "Укажите мощность двигателя.");

        if (EngineType == EngineType.Unknown)
            modelState.AddModelError($"{prefix}.{nameof(EngineType)}", "Укажите тип двигателя.");

        if (Transmission == TransmissionType.Unknown)
            modelState.AddModelError($"{prefix}.{nameof(Transmission)}", "Укажите коробку передач.");

        if (Drive == CarDriveType.Unknown)
            modelState.AddModelError($"{prefix}.{nameof(Drive)}", "Укажите привод.");
    }

    public void ApplyTo(CarSpecification entity)
    {
        entity.BodyType = BodyType;
        entity.Mileage = Mileage;
        entity.Color = Color.Trim();
        entity.Doors = Doors;
        entity.Seats = Seats;
        entity.EngineVolumeLiters = EngineVolumeLiters;
        entity.EnginePowerHp = EnginePowerHp;
        entity.EngineType = EngineType;
        entity.Transmission = Transmission;
        entity.Drive = Drive;
        entity.FuelConsumptionL100Km = FuelConsumptionL100Km;
        entity.EmissionClass = string.IsNullOrWhiteSpace(EmissionClass) ? null : EmissionClass.Trim();
        entity.HasAbs = HasAbs;
        entity.HasEsp = HasEsp;
        entity.HasAirbags = HasAirbags;
        entity.HasAirConditioning = HasAirConditioning;
        entity.HasClimateControl = HasClimateControl;
        entity.HasBluetooth = HasBluetooth;
        entity.HasUsb = HasUsb;
        entity.HasNavigation = HasNavigation;
        entity.HasParkingSensors = HasParkingSensors;
        entity.HasRearCamera = HasRearCamera;
        entity.Documents = string.IsNullOrWhiteSpace(Documents) ? null : Documents.Trim();
        entity.OwnerCount = OwnerCount;
        entity.Condition = string.IsNullOrWhiteSpace(Condition) ? null : Condition.Trim();
        entity.UpdatedAtUtc = DateTime.UtcNow;
    }
}
