using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BookSwap.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BookSwap.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Imi� jest wymagane")]
            [Display(Name = "Imi�")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Nazwisko jest wymagane")]
            [Display(Name = "Nazwisko")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "{0} musi mie� co najmniej {2} znak�w.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Has�o")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Potwierd� has�o")]
            [Compare("Password", ErrorMessage = "Has�a si� nie zgadzaj�.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (ModelState.IsValid)
            {
                // Tworzymy nowego u�ytkownika z zadeklarowanymi polami
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Utworzono nowe konto u�ytkownika.");

                    // Przypisz rol� "User" do nowego konta
                    await _userManager.AddToRoleAsync(user, "User");

                    // Zaloguj od razu nowo utworzonego u�ytkownika:
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Je�li walidacja ModelState nie powiedzie si�, poka� formularz ponownie
            return Page();
        }
    }
}
