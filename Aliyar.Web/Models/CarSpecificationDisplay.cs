namespace Aliyar.Web.Models;

public static class CarSpecificationDisplay
{
    public static string ToDisplayName(this BodyType value) => value switch
    {
        BodyType.Sedan => "Седан",
        BodyType.Hatchback => "Хэтчбек",
        BodyType.Suv => "Внедорожник / кроссовер",
        BodyType.Coupe => "Купе",
        BodyType.Wagon => "Универсал",
        BodyType.Van => "Минивэн / фургон",
        BodyType.Pickup => "Пикап",
        _ => "Не указан",
    };

    public static string ToDisplayName(this EngineType value) => value switch
    {
        EngineType.Petrol => "Бензин",
        EngineType.Diesel => "Дизель",
        EngineType.Hybrid => "Гибрид",
        EngineType.Electric => "Электро",
        _ => "Не указан",
    };

    public static string ToDisplayName(this TransmissionType value) => value switch
    {
        TransmissionType.Manual => "Механика",
        TransmissionType.Automatic => "Автомат",
        TransmissionType.Cvt => "Вариатор",
        TransmissionType.Robot => "Робот",
        _ => "Не указана",
    };

    public static string ToDisplayName(this CarDriveType value) => value switch
    {
        CarDriveType.Front => "Передний",
        CarDriveType.Rear => "Задний",
        CarDriveType.All => "Полный",
        _ => "Не указан",
    };
}
