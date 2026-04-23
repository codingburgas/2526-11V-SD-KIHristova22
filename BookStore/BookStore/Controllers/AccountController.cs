using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers;

// Minimal controller to provide stable /Account/* routes while using Identity UI Razor Pages under /Identity/Account/*.
public class AccountController(SignInManager<ApplicationUser> signInManager) : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

    [AllowAnonymous]
    [HttpGet("/Account/Login")]
    public IActionResult Login()
    {
        return Redirect($"/Identity/Account/Login?returnUrl={Uri.EscapeDataString("/Home/Index")}");
    }

    [AllowAnonymous]
    [HttpGet("/Account/Register")]
    public IActionResult Register()
    {
        return Redirect($"/Identity/Account/Register?returnUrl={Uri.EscapeDataString("/Home/Index")}");
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("/Account/Logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}