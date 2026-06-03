using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Admin.Clients;

[Authorize(Policy = AppPolicies.AdminOnly)]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public List<ClientItem> Clients { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Clients = await (
                from client in _db.Clients.AsNoTracking()
                join user in _db.Users.AsNoTracking() on client.UserId equals user.Id
                orderby client.FullName
                select new ClientItem(
                    client.Id,
                    client.FullName,
                    user.UserName ?? user.Email ?? ""))
            .ToListAsync();
    }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnPostDeleteAsync(int clientId)
    {
        var client = await _db.Clients.SingleOrDefaultAsync(x => x.Id == clientId);
        if (client is null)
        {
            return NotFound();
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var listings = await _db.Cars
            .Where(x => x.Kind == ListingKind.Customer && x.OwnerUserId == client.UserId)
            .ToListAsync();

        if (listings.Count > 0)
        {
            _db.Cars.RemoveRange(listings);
            await _db.SaveChangesAsync();
        }

        var user = await _userManager.FindByIdAsync(client.UserId);
        if (user is not null)
        {
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await tx.RollbackAsync();
                await OnGetAsync();
                return Page();
            }
        }
        else
        {
            _db.Clients.Remove(client);
            await _db.SaveChangesAsync();
        }

        await tx.CommitAsync();

        StatusMessage = "Клиент удалён.";
        return RedirectToPage();
    }

    public sealed record ClientItem(int Id, string FullName, string Login);
}
