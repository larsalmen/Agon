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

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Agon.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public async Task<IActionResult> UserLoggedIn()
        {
            var username = User.Identity.Name;

            var userVM = await AgonManager.GetUserVMAsync("User", username, User.Identity.IsAuthenticated);

            return View(userVM);
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(UserLoggedIn));
            }

            return View();
        }

        public async Task<IActionResult> ViewPlaylists()
        {
            var token = AgonManager.GetSpotifyTokens(this);

            var viewmodel = await AgonManager.GetPlaylists(token);

            return View(viewmodel);
        }

    }
}
