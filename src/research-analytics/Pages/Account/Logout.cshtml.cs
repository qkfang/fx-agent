using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FxWebPortal.Pages.Account;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        HttpContext.Session.Remove("UserAuth");
        HttpContext.Session.Remove("UserDisplayName");
        return RedirectToPage("/Index");
    }
}
