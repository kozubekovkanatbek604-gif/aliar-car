using Aliyar.Web.Models;

namespace Aliyar.Web.Pages.Shared;

public sealed class CarCardViewModel
{
    public required Car Car { get; init; }

    public string? CoverPhotoUrl { get; init; }

    public bool ShowStatus { get; init; }

    public bool ShowManageActions { get; init; }
}
