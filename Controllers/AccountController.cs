using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.AccountVM;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var link = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);
                    await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                        $"<p>Click <a href='{link}'>here</a> to confirm your account.</p>");

                    SetFlashMessage("Registration successful! Please check your email to confirm your account.", "success");
                    return RedirectToAction("Login");
                }

                SetFlashMessage(string.Join("<br>", result.Errors.Select(e => e.Description)), "error");
            }
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new EditProfileViewModel
            {
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                SetFlashMessage("Profile updated successfully!", "success");
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError(string.Empty, "Failed to update profile");
            return View(model);
        }





        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                SetFlashMessage("Email confirmed! You may now log in.", "success");
                return RedirectToAction("Login");
            }

            SetFlashMessage("Email confirmation failed.", "error");
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Login() => View();
        // {
        //     return View();
        // }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                SetFlashMessage("You must confirm your email before logging in.", "error");
                return View(model);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

            if (result.Succeeded)
            {

                var randomcode = new Random().Next(100000, 999999).ToString();


                TempData["2fa_code"] = randomcode;
                TempData["2fa_user"] = user.Email;
                TempData["rememberMe"] = model.RememberMe;

                await _emailSender.SendEmailAsync(user.Email, "Your 2FA Code",
                    $"Your security code is: <strong>{randomcode}</strong>");

                return RedirectToAction("Verify2fa");
            }
            else if (result.IsLockedOut)
            {
                SetFlashMessage("Account is locked. Try again later.", "error");
                return View(model);
            }

            SetFlashMessage("Invalid login attempt.", "error");
            return View(model);
        }



        [HttpGet]
        public IActionResult Verify2fa(bool rememberMe)
        {
            return View(new Verify2faViewModel { RememberMe = rememberMe });
        }

        [HttpPost]
        public async Task<IActionResult> Verify2fa(Verify2faViewModel model)
        {
            var expectedCode = TempData["2fa_code"]?.ToString();
            var email = TempData["2fa_user"]?.ToString();
            var rememberMe = TempData["rememberMe"] != null && Convert.ToBoolean(TempData["rememberMe"]);

            if (string.IsNullOrEmpty(expectedCode) || string.IsNullOrEmpty(email))
            {

                SetFlashMessage("Session expired. Please log in again.", "error");
                return RedirectToAction("Login");
            }

            if (expectedCode == model.Code)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    SetFlashMessage("User not found.", "error");
                    return RedirectToAction("Login");
                }

                await _signInManager.SignInAsync(user, rememberMe);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("Code", "Invalid code. Please try again.");
            return View(model);
        }



        [HttpGet]
        public IActionResult ForgotPassword() => View();
        // {
        //     return View(); 
        // }


        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                SetFlashMessage("User not found or email not confirmed.", "error");
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, "Reset your password",
                $"<p>Click <a href='{link}'>here</a> to reset your password.</p>");

            SetFlashMessage("Reset link sent. Check your inbox.", "success");
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                SetFlashMessage("User not found.", "error");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                SetFlashMessage("Password reset successful.", "success");
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

    }
}
