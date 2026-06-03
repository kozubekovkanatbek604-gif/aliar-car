namespace Aliyar.Web.Models;

public sealed class ManagerProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public string PassportNumber { get; set; } = "";

    public string Address { get; set; } = "";

    public string PhoneNumber { get; set; } = "";

    public int Age { get; set; }

    public ManagerGender Gender { get; set; }

    public string PhotoPath { get; set; } = "";
}

