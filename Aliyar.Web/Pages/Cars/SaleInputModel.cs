using System.ComponentModel.DataAnnotations;
using Aliyar.Web.Models;

namespace Aliyar.Web.Pages.Cars;

public sealed class SaleInputModel
{
    [Required]
    [Display(Name = "Клиент")]
    public int ClientId { get; set; }

    [Range(1, 999_999_999)]
    [Display(Name = "Цена продажи")]
    public int SalePrice { get; set; }

    [Display(Name = "Способ оплаты")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}
