using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FxWebPortal.Pages.Admin;

public class LoginModel : PageModel
{
    private readonly IConfiguration _config;

    public LoginModel(IConfiguration config)
    {
        _config = config;
    }

    public string Error { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("AdminAuth") == "true")
            return RedirectToPage("/Admin/Index");
        return Page();
    }

    public IActionResult OnPost(string username, string password)
    {
        var adminUser = _config["Admin:Username"] ?? "admin";
        var adminPass = _config["Admin:Password"] ?? "fx@dmin2026";

        if (string.Equals(username, adminUser, StringComparison.Ordinal)
            && string.Equals(password, adminPass, StringComparison.Ordinal))
        {
            HttpContext.Session.SetString("AdminAuth", "true");
            return RedirectToPage("/Admin/Index");
        }

        Error = "Invalid username or password.";
        return Page();
    }
}
