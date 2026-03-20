using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FxWebPortal.Pages.Account;

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
        if (HttpContext.Session.GetString("UserAuth") == "true")
            return RedirectToPage("/Account/Index");
        return Page();
    }

    public IActionResult OnPost(string username, string password)
    {
        var userUser = _config["User:Username"] ?? "trader";
        var userPass = _config["User:Password"] ?? "fx2026";

        if (string.Equals(username, userUser, StringComparison.Ordinal)
            && string.Equals(password, userPass, StringComparison.Ordinal))
        {
            HttpContext.Session.SetString("UserAuth", "true");
            HttpContext.Session.SetString("UserDisplayName", _config["User:DisplayName"] ?? username);
            return RedirectToPage("/Account/Index");
        }

        Error = "Invalid username or password.";
        return Page();
    }
}
