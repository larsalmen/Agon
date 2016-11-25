using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Agon.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using MongoUtils;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Agon.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {

        [HttpPost]
        public async Task<IActionResult> Create(PlaylistVM viewModel)
        {
            var token = AgonManager.GetSpotifyTokens(this);
            var newQuiz = await AgonManager.GenerateQuiz(token, viewModel);

            var currentQuiz = JsonConvert.SerializeObject(newQuiz, Formatting.Indented);
            HttpContext.Session.SetString("currentQuiz", currentQuiz);

            return View(newQuiz);
        }

        public IActionResult AddSong()
        {
            return View();
        }
        [HttpPost]
        public IActionResult EditSong(string id)
        {
            var viewModel = AgonManager.CreateEditSongVM(id);

            return View(viewModel);
        }

        [HttpPost]

        public async Task<IActionResult> UpdateQuestions(string id)
        {
            var questionText = Request.Form["item.Text"];
            var answerText = Request.Form["item.CorrectAnswer"];
            var jsonQuiz = HttpContext.Session.GetString("currentQuiz");

            var updatedQuiz = AgonManager.UpdateQuestions(questionText, answerText, jsonQuiz,id);

            await MongoManager.SaveQuizAsync(JsonConvert.SerializeObject(updatedQuiz));

            return RedirectToAction("ViewPlaylists", "Home");
        }
    }
}
