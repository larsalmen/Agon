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
        [HttpGet]
        public IActionResult Index()
        {
            var username = User.Identity.Name;

            var user = new IndexVM("Agon") { Username = username, LoggedIn = User.Identity.IsAuthenticated, Quizzes = new List<Quiz> { new Quiz { Name = "Mitt Quiz 1" }, new Quiz { Name = "Aqua-quiz" } } };

            return View(user);
        }

        public async Task<IActionResult> ViewPlaylists()
        {
            var token = AgonManager.GetSpotifyTokens(this);

            var viewmodel = await AgonManager.GetPlaylists(token);

            return View(viewmodel);
        }
    }
}
