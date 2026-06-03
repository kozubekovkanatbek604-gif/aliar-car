using System.ComponentModel.DataAnnotations;

namespace Aliyar.Web.Pages.Cars;

public sealed class CarInputModel
{
    [Required, StringLength(100)]
    public string Make { get; set; } = "";

    [Required, StringLength(100)]
    public string Model { get; set; } = "";

    [Range(1900, 2100)]
    public int Year { get; set; }

    [Range(0, 999_999_999)]
    public int Price { get; set; }

    [Range(0, 999_999_999)]
    [Display(Name = "Цена закупки")]
    public int PurchasePrice { get; set; }

    [StringLength(32)]
    public string? Vin { get; set; }
}

