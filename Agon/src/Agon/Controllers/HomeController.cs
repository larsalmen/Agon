using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Agon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using SpotifyUtils;
using Microsoft.AspNetCore.Http;

namespace Agon.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Error()
        {
            var errorVM = new ErrorVM();
            errorVM.ErrorMessage = HttpContext.Session.GetString("error");

            return View(errorVM);
        }
        public async Task<IActionResult> UserLoggedIn()
        {
            var username = User.Identity.Name;

            var userVM = await AgonManager.GetUserVMAsync(username, User.Identity.IsAuthenticated);

            return View(userVM);
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated && HttpContext.Session.GetString("access_token") != null)
            {
                return RedirectToAction(nameof(UserLoggedIn));
            }
            else
                return View();
        }

        public async Task<IActionResult> ViewPlaylists()
        {
            var token = AgonManager.GetSpotifyTokens(this);
            List<PlaylistVM> viewModel;
            try
            {
                viewModel = await AgonManager.GetPlaylists(token);
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                return RedirectToAction("Error", "Home");
            }

            return View(viewModel);
        }


    }
}
