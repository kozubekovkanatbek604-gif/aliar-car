using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aliyar.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterClientModel : PageModel
{
    private static readonly Regex LoginPattern = new(@"^[a-zA-Z0-9._@-]+$", RegexOptions.Compiled);

    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _db;
    private readonly ILogger<RegisterClientModel> _logger;

    public RegisterClientModel(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        AppDbContext db,
        ILogger<RegisterClientModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
            return Page();

        var login = Input.Login.Trim();
        if (!LoginPattern.IsMatch(login))
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Login)}",
                "Логин может содержать только латинские буквы, цифры, точку, дефис, подчёркивание и символ @.");
            return Page();
        }

        var user = new IdentityUser
        {
            UserName = login,
            EmailConfirmed = true,
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }

        await _userManager.AddToRoleAsync(user, AppRoles.Client);

        _db.Clients.Add(new Client
        {
            FullName = Input.FullName.Trim(),
            UserId = user.Id,
        });
        await _db.SaveChangesAsync();

        _logger.LogInformation("Client {Login} registered.", login);
        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(returnUrl);
    }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Укажите ФИО.")]
        [StringLength(200, ErrorMessage = "ФИО не должно превышать {1} символов.")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Укажите логин.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от {2} до {1} символов.")]
        [Display(Name = "Логин")]
        public string Login { get; set; } = "";

        [Required(ErrorMessage = "Укажите пароль.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не короче {2} символов.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Подтвердите пароль.")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
