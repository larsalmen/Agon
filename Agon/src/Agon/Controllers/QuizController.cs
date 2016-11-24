using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Agon.Models;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Agon.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        // GET: /<controller>/
        [HttpPost]
        public async Task<IActionResult> Create(PlaylistVM viewModel)
        {
            var token = AgonManager.GetSpotifyTokens(this);

            var newQuiz = await AgonManager.GenerateQuiz(token, viewModel);
            var jsonhej = JsonConvert.SerializeObject(newQuiz, Formatting.Indented);
            return View(newQuiz);
        }

        public IActionResult AddSong()
        {
            return View();
        }
    }
}
