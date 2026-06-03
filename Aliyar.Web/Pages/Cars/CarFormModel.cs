namespace Aliyar.Web.Pages.Cars;

public interface ICarFormModel
{
    CarInputModel Car { get; set; }

    bool ShowPurchasePrice => false;
}


