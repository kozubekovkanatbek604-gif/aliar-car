using System.ComponentModel.DataAnnotations;
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
public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _db;
    private readonly ManagerPhotoStorage _photoStorage;

    public IndexModel(UserManager<IdentityUser> userManager, AppDbContext db, ManagerPhotoStorage photoStorage)
    {
        _userManager = userManager;
        _db = db;
        _photoStorage = photoStorage;
    }

    public List<ManagerItem> Managers { get; private set; } = [];

    [BindProperty]
    public CreateManagerInput Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        Managers = await LoadManagersAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        Managers = await LoadManagersAsync();

        if (!ModelState.IsValid)
            return Page();

        if (Input.Photo is null)
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Photo)}", "Загрузите фото.");
            return Page();
        }

        var photoError = _photoStorage.Validate(Input.Photo);
        if (photoError is not null)
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Photo)}", photoError);
            return Page();
        }

        var email = Input.Email.Trim();
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            if (await _userManager.IsInRoleAsync(existing, AppRoles.Manager))
            {
                ModelState.AddModelError(nameof(Input.Email), "Пользователь с этим email уже является менеджером.");
                return Page();
            }

            if (await _userManager.IsInRoleAsync(existing, AppRoles.Admin))
            {
                ModelState.AddModelError(nameof(Input.Email), "Этот пользователь уже администратор.");
                return Page();
            }

            if (await _db.ManagerProfiles.AnyAsync(x => x.UserId == existing.Id))
            {
                ModelState.AddModelError(nameof(Input.Email), "Для этого пользователя анкета менеджера уже существует.");
                return Page();
            }

            var photoPathExisting = await _photoStorage.SaveAsync(existing.Id, Input.Photo, HttpContext.RequestAborted);
            _db.ManagerProfiles.Add(new ManagerProfile
            {
                UserId = existing.Id,
                PassportNumber = Input.PassportNumber.Trim(),
                Address = Input.Address.Trim(),
                PhoneNumber = Input.PhoneNumber.Trim(),
                Age = Input.Age,
                Gender = Input.Gender,
                PhotoPath = photoPathExisting,
            });
            await _db.SaveChangesAsync(HttpContext.RequestAborted);

            var roleResult = await _userManager.AddToRoleAsync(existing, AppRoles.Manager);
            if (!roleResult.Succeeded)
            {
                _photoStorage.DeleteIfExists(photoPathExisting);
                _db.ManagerProfiles.RemoveRange(_db.ManagerProfiles.Where(x => x.UserId == existing.Id));
                await _db.SaveChangesAsync(HttpContext.RequestAborted);
                AddIdentityErrors(roleResult);
                return Page();
            }

            TempData["StatusMessage"] = $"Роль менеджера назначена пользователю {email}.";
            return RedirectToPage();
        }

        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var createResult = await _userManager.CreateAsync(user, Input.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return Page();
        }

        var photoPathNew = await _photoStorage.SaveAsync(user.Id, Input.Photo, HttpContext.RequestAborted);
        _db.ManagerProfiles.Add(new ManagerProfile
        {
            UserId = user.Id,
            PassportNumber = Input.PassportNumber.Trim(),
            Address = Input.Address.Trim(),
            PhoneNumber = Input.PhoneNumber.Trim(),
            Age = Input.Age,
            Gender = Input.Gender,
            PhotoPath = photoPathNew,
        });
        await _db.SaveChangesAsync(HttpContext.RequestAborted);

        var addRoleResult = await _userManager.AddToRoleAsync(user, AppRoles.Manager);
        if (!addRoleResult.Succeeded)
        {
            _photoStorage.DeleteIfExists(photoPathNew);
            _db.ManagerProfiles.RemoveRange(_db.ManagerProfiles.Where(x => x.UserId == user.Id));
            await _db.SaveChangesAsync(HttpContext.RequestAborted);
            await _userManager.DeleteAsync(user);
            AddIdentityErrors(addRoleResult);
            return Page();
        }

        TempData["StatusMessage"] = $"Менеджер {email} создан.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return RedirectToPage();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            TempData["StatusMessage"] = "Пользователь не найден.";
            return RedirectToPage();
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["StatusMessage"] = "Нельзя снять роль менеджера с самого себя.";
            return RedirectToPage();
        }

        if (!await _userManager.IsInRoleAsync(user, AppRoles.Manager))
        {
            TempData["StatusMessage"] = "Пользователь не является менеджером.";
            return RedirectToPage();
        }

        var result = await _userManager.RemoveFromRoleAsync(user, AppRoles.Manager);
        TempData["StatusMessage"] = result.Succeeded
            ? $"Роль менеджера снята с {user.Email}."
            : "Не удалось снять роль менеджера.";

        return RedirectToPage();
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
    }

    private async Task<List<ManagerItem>> LoadManagersAsync()
    {
        var users = await _userManager.GetUsersInRoleAsync(AppRoles.Manager);
        var userIds = users.Select(x => x.Id).ToList();
        var phones = await _db.ManagerProfiles.AsNoTracking()
            .Where(x => userIds.Contains(x.UserId))
            .ToDictionaryAsync(x => x.UserId, x => x.PhoneNumber);

        return users
            .OrderBy(x => x.Email)
            .Select(x => new ManagerItem(
                x.Id,
                x.Email ?? x.UserName ?? "",
                phones.GetValueOrDefault(x.Id) ?? ""))
            .ToList();
    }

    public sealed record ManagerItem(string Id, string Email, string PhoneNumber);

    public sealed class CreateManagerInput
    {
        [Required(ErrorMessage = "Укажите email.")]
        [EmailAddress(ErrorMessage = "Некорректный email.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Укажите пароль.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не короче {2} символов.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Подтвердите пароль.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = "";

        [Required(ErrorMessage = "Укажите паспорт.")]
        [StringLength(50, ErrorMessage = "Паспорт не должен превышать {1} символов.")]
        [Display(Name = "Паспорт")]
        public string PassportNumber { get; set; } = "";

        [Required(ErrorMessage = "Укажите адрес проживания.")]
        [StringLength(400, ErrorMessage = "Адрес не должен превышать {1} символов.")]
        [Display(Name = "Адрес проживания")]
        public string Address { get; set; } = "";

        [Required(ErrorMessage = "Укажите телефон.")]
        [StringLength(50, ErrorMessage = "Телефон не должен превышать {1} символов.")]
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; } = "";

        [Range(18, 120, ErrorMessage = "Возраст должен быть от {1} до {2}.")]
        [Display(Name = "Возраст")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Укажите пол.")]
        [Display(Name = "Пол")]
        public ManagerGender Gender { get; set; } = ManagerGender.Unknown;

        [Display(Name = "Фото")]
        public IFormFile? Photo { get; set; }
    }
}
