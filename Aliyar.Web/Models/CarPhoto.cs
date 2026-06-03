namespace Aliyar.Web.Models;

public sealed class CarPhoto
{
    public int Id { get; set; }

    public int CarId { get; set; }

    public Car Car { get; set; } = null!;

    /// <summary>Относительный путь в wwwroot, например uploads/cars/1_abc.jpg.</summary>
    public string Path { get; set; } = "";

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
