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
            await MongoManager.SaveQuizToSession(currentQuiz, token.Username);
            return RedirectToAction("EditQuiz");
        }

        [HttpGet]
        public IActionResult AddSong()
        {
            return View();
        }

        [HttpPost]
        public async void AddSingleSong(string href)
        {

            var token = AgonManager.GetSpotifyTokens(this);
            var currentQuiz = await MongoManager.GetQuizFromSession(token.Username);

            var newSong = await Task.Run(async () => await AgonManager.AddSongToQuiz(token, href));

            try
            {
                var newQuiz = JsonConvert.DeserializeObject<Quiz>(currentQuiz);
                newQuiz.Songs.Add(newSong);

                var quizToStore = JsonConvert.SerializeObject(newQuiz);

                await MongoManager.SaveQuizToSession(quizToStore, token.Username);
            }
            catch (Exception)
            {
                throw;
            }
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
            var jsonQuiz = await MongoManager.GetQuizFromSession(HttpContext.User.Identity.Name);

            var updatedQuiz = AgonManager.UpdateQuestions(questionText, answerText, jsonQuiz, id);

            await MongoManager.ReplaceOneQuizAsync(updatedQuiz.Owner, updatedQuiz._id, JsonConvert.SerializeObject(updatedQuiz), "Quizzes");

            var currentQuiz = JsonConvert.SerializeObject(updatedQuiz);
            await MongoManager.SaveQuizToSession(currentQuiz, HttpContext.User.Identity.Name);

            return RedirectToAction("EditQuiz", "Quiz");
        }
        [HttpGet]
        public async Task<IActionResult> EditQuiz() // 2016-11-25 21:31 - Här kan man ta in _id och få det från Home/ViewPlaylists, om man vill och behöver
        {
            var checkCurrentQuiz = await MongoManager.GetQuizFromSession(HttpContext.User.Identity.Name);
            if (checkCurrentQuiz != null && checkCurrentQuiz != "")
            {
                var jsonQuiz = await MongoManager.GetQuizFromSession(HttpContext.User.Identity.Name);
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
            var quiz = await MongoManager.GetOneQuizAsync(_id, "Quizzes");
            await MongoManager.SaveQuizToSession(quiz, HttpContext.User.Identity.Name);


            return RedirectToAction("EditQuiz");
        }
        public async Task SaveQuiz()
        {
            var jsonQuiz = await MongoManager.GetQuizFromSession(HttpContext.User.Identity.Name);
            var quiz = JsonConvert.DeserializeObject<Quiz>(jsonQuiz);

            if (await MongoManager.CheckIfDocumentExistsAsync(quiz.Owner, quiz.Name, "Quizzes"))
            {
                await MongoManager.ReplaceOneQuizAsync(quiz.Owner, quiz._id, jsonQuiz, "Quizzes");
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


        public async Task DropPin(string id)
        {
            await MongoManager.RemovePinFromQuiz(id, "runningQuizzes");
        }
        [AllowAnonymous]
        public async Task<IActionResult> PlayQuiz(string pin)
        {
            QuizPlayerVM quizPlayerVM = null;
            if (await MongoManager.CheckIfPinExistsAsync(pin, "runningQuizzes"))
            {
                quizPlayerVM = await AgonManager.CreateQuizPlayerVM(pin);
                return View(quizPlayerVM);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<bool> CheckPin(string pin)
        {
            return await MongoManager.CheckIfPinExistsAsync(pin, "runningQuizzes");
        }
    }
}
