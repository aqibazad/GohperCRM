// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using LegalAndGeneralConsultantCRM.Areas.Identity.Data;

namespace LegalAndGeneralConsultantCRM.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<LegalAndGeneralConsultantCRMUser> _signInManager;
        private readonly UserManager<LegalAndGeneralConsultantCRMUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<LegalAndGeneralConsultantCRMUser> _userStore;
        private readonly IUserEmailStore<LegalAndGeneralConsultantCRMUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<LegalAndGeneralConsultantCRMUser> userManager,
            IUserStore<LegalAndGeneralConsultantCRMUser> userStore,
            SignInManager<LegalAndGeneralConsultantCRMUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            public string UserName { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser(Input.UserName);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var roleExists = await _roleManager.RoleExistsAsync("Employee");
                    if (!roleExists)
                    {
                        var roleResult = await _roleManager.CreateAsync(new IdentityRole("Employee"));
                        if (!roleResult.Succeeded)
                        {
                            ModelState.AddModelError(string.Empty, "Failed to create Employee role");
                            TempData["Error"] = "Failed to create Employee role.";
                            return Page();
                        }
                    }

                    var roleAssignResult = await _userManager.AddToRoleAsync(user, "Employee");
                    if (!roleAssignResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Failed to assign Employee role");
                        TempData["Error"] = "Failed to assign Employee role.";
                        return Page();
                    }

                    TempData["Success"] = "Congrats, you are successfully registered!";
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Redirect("/Identity/Account/Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                TempData["Error"] = "Failed to create user account.";
            }

            return Page();
        }
        private LegalAndGeneralConsultantCRMUser CreateUser(string userName)
		{
			try
			{
				// Create a new instance of the user
				var user = Activator.CreateInstance<LegalAndGeneralConsultantCRMUser>();

				// Set the UserName on the user object
				user.UserName = userName;

				return user;
			}
			catch
			{
				throw new InvalidOperationException($"Can't create an instance of '{nameof(LegalAndGeneralConsultantCRMUser)}'. " +
					$"Ensure that '{nameof(LegalAndGeneralConsultantCRMUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
					$"provide a constructor that allows the creation of '{nameof(LegalAndGeneralConsultantCRMUser)}'.");
			}
		}

		private IUserEmailStore<LegalAndGeneralConsultantCRMUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<LegalAndGeneralConsultantCRMUser>)_userStore;
        }
    }
}
