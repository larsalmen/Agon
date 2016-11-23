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
        
        public async Task GetPlayLists()
        {
            //Flytta tokens till SpotifyManager med en CheckToken-metod

            var access_token = HttpContext.Session.GetString("access_token");
            var refresh_token = HttpContext.Session.GetString("refresh_token");
            var recieved_at = HttpContext.Session.GetString("expires_at");
            var userName = User.Identity.Name;

            var allReturnedPlaylists = await SpotifyManager.GetAllUserPlaylists(access_token, refresh_token, recieved_at, userName);
            
        }      
    }
}
