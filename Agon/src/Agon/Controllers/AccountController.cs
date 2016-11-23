using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Agon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.AspNetCore.Http;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Agon.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        UserManager<IdentityUser> userManager;
        SignInManager<IdentityUser> signInManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string str = null)
        {
            return View(new IndexVM("Login") { Username = "KalleKula", LoggedIn = true, Quizzes = new List<Quiz> { new Quiz { Name = "Mitt Quiz 1" }, new Quiz { Name = "Aqua-quiz" } } });
        }

        [AllowAnonymous]
        [HttpPost]
        public string Login()
        {
            return "Inloggad";
        }

        [AllowAnonymous]
        [HttpPost]
        public string Fail()
        {
            return "Fail";
        }

        /*[AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(AccountLoginVM viewModel, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            // Skapa DB-schemat
            //await identityContext.Database.EnsureCreatedAsync();

            // Create user
            //var user = new IdentityUser("pontus");
            //var result = await userManager.CreateAsync(user, "Pontus_1234");

            var result = await signInManager.PasswordSignInAsync(
                viewModel.Username, viewModel.Password, false, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(nameof(AccountLoginVM.Username),
                    "Incorrect login credentials");
                return View(viewModel);
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
                return RedirectToAction(nameof(AccountController.Index));
            else
                return Redirect(returnUrl);
        }*/


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {


            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Fail));
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Fail));
            }

            //Sign in the user with this external login provider if the user already has a login.
            var signinresult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (signinresult.Succeeded)
            {
                SetTokensInSession(info);


                return RedirectToLocal(returnUrl);
            }
            else if (signinresult.IsNotAllowed || signinresult.IsLockedOut)
            {
                return RedirectToAction(nameof(Fail));
            }
            else
            {
                // If the user does not exist, create it.
                var user = new IdentityUser { UserName = info.ProviderKey };

                var result = await userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    result = await userManager.AddLoginAsync(user, info);

                    if (result.Succeeded)
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        SetTokensInSession(info);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                        return RedirectToAction(nameof(Fail));
                }
                else
                    return RedirectToAction(nameof(Fail));
            }

        }
        private void SetTokensInSession(ExternalLoginInfo info)
        {
            string access_token = "";
            string refresh_token = "";
            string recieved_at = "";

            access_token = info.AuthenticationTokens.Where(x => x.Name == "access_token").Select(y => y.Value).FirstOrDefault();
            HttpContext.Session.SetString("access_token", access_token);

            refresh_token = info.AuthenticationTokens.Where(x => x.Name == "refresh_token").Select(y => y.Value).FirstOrDefault();
            HttpContext.Session.SetString("refresh_token", refresh_token);

            recieved_at = info.AuthenticationTokens.Where(x => x.Name == "expires_at").Select(y => y.Value).FirstOrDefault();
            HttpContext.Session.SetString("expires_at", recieved_at);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
