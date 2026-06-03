using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Aliyar.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Admin.Managers;

[Authorize(Policy = AppPolicies.AdminOnly)]
public class ProfileModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public ProfileModel(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public ManagerProfile? Profile { get; private set; }

    public string Email { get; private set; } = "";

    public string PhotoUrl { get; private set; } = "";

    public string GenderLabel { get; private set; } = "";

    public async Task<IActionResult> OnGetAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return RedirectToPage("/Admin/Managers/Index");

        var user = await _userManager.FindByIdAsync(userId);
        Email = user?.Email ?? user?.UserName ?? userId;

        Profile = await _db.ManagerProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
        if (Profile is null)
            return Page();

        PhotoUrl = ManagerPhotoStorage.ToPublicUrl(Profile.PhotoPath);
        GenderLabel = Profile.Gender switch
        {
            ManagerGender.Male => "Мужской",
            ManagerGender.Female => "Женский",
            _ => "Не указан",
        };

        return Page();
    }
}

