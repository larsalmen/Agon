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


            var currentQuiz = JsonConvert.SerializeObject(newQuiz);
            HttpContext.Session.SetString("currentQuiz", currentQuiz);

            return RedirectToAction("EditQuiz");
        }

        public IActionResult AddSong()
        {
            return View();
        }

        public void AddSingleSong(string href)
        {
            //this method is an ajax call from javascript SearchSpotifyForAlbumAndPlay30Sec.js
            //the incoming variable href is the full href to a song (example: https://api.spotify.com/v1/tracks/0niC3Stpj4rX4Ul3udkbUO)

            //[] run method for extracting the song id from the last part of href-string

            //add this single song to quiz
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

            var updatedQuiz = AgonManager.UpdateQuestions(questionText, answerText, jsonQuiz, id);

            await MongoManager.ReplaceOneQuizAsync(updatedQuiz.Owner, updatedQuiz._id, JsonConvert.SerializeObject(updatedQuiz));

            var currentQuiz = JsonConvert.SerializeObject(updatedQuiz, Formatting.Indented);
            HttpContext.Session.SetString("currentQuiz", currentQuiz);

            return RedirectToAction("EditQuiz", "Quiz");
        }
        [HttpGet]
        public IActionResult EditQuiz() // 2016-11-25 21:31 - Här kan man ta in _id och få det från Home/ViewPlaylists, om man vill och behöver
        {
            if (HttpContext.Session.GetString("currentQuiz") != null && HttpContext.Session.GetString("currentQuiz") != "")
            {
                var jsonQuiz = HttpContext.Session.GetString("currentQuiz");
                var quizToEdit = JsonConvert.DeserializeObject<Quiz>(jsonQuiz);

                return View(quizToEdit);
            }
            else
            {
                return RedirectToAction("UserLoggedIn", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditQuiz(string _id)
        {
            var quiz = await MongoManager.GetOneQuizAsync(_id);
            HttpContext.Session.SetString("currentQuiz", quiz);

            return RedirectToAction("EditQuiz");
        }
        public async Task SaveQuiz()
        {
            var jsonQuiz = HttpContext.Session.GetString("currentQuiz");
            var quiz = JsonConvert.DeserializeObject<Quiz>(jsonQuiz);

            if (await MongoManager.CheckIfDocumentExistsAsync(quiz.Owner, quiz.Name))
            {
                await MongoManager.ReplaceOneQuizAsync(quiz.Owner, quiz._id, jsonQuiz);
            }
            else
            {
                await MongoManager.SaveDocumentAsync(jsonQuiz);
            }


        }

        public async Task<IActionResult> StartQuiz(string _id)
        {
            QuizMasterVM quizMasterVM = await AgonManager.StartQuiz(_id);
            return View(quizMasterVM);
        }
    }
}
