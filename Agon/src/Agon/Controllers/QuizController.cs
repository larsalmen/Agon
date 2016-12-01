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
using Microsoft.Extensions.Caching.Memory;

namespace Agon.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        IMemoryCache _memoryCache;
        public QuizController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        [HttpPost]
        public async Task<IActionResult> Create(PlaylistVM viewModel)
        {
            try
            {
                var token = AgonManager.GetSpotifyTokens(this);
                var newQuiz = await AgonManager.GenerateQuiz(token, viewModel);
                var currentQuiz = JsonConvert.SerializeObject(newQuiz);
                await MongoManager.SaveQuizToSession(currentQuiz, token.Username);
                return RedirectToAction("EditQuiz");
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                return RedirectToAction("Error", "Home");
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpGet]
        public IActionResult AddSong()
        {
            return View();
        }
        public async Task RemoveSongFromQuiz(int index)
        {
            var token = AgonManager.GetSpotifyTokens(this);
            try
            {
                await AgonManager.RemoveSongFromQuiz(token, index);
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                RedirectToError();
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                RedirectToError();
            }
        }
        [HttpPost]
        public async Task AddSingleSong(string href)
        {
            try
            {
                var token = AgonManager.GetSpotifyTokens(this);
                var currentQuiz = await MongoManager.GetQuizFromSession(token.Username);
                var newSong = await Task.Run(async () => await AgonManager.AddSongToQuiz(token, href));
                var newQuiz = JsonConvert.DeserializeObject<Quiz>(currentQuiz);
                newQuiz.Songs.Add(newSong);

                var quizToStore = JsonConvert.SerializeObject(newQuiz);

                await MongoManager.SaveQuizToSession(quizToStore, token.Username);
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                RedirectToError();
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                RedirectToError();
            }
        }
        [HttpPost]
        public IActionResult EditSong(string id)
        {
            try
            {
                var viewModel = AgonManager.CreateEditSongVM(id);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                return RedirectToError();
            }

        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuestions(string id)
        {
            var questionText = Request.Form["item.Text"];
            var answerText = Request.Form["item.CorrectAnswer"];


            Quiz updatedQuiz;
            try
            {
                var jsonQuiz = await MongoManager.GetQuizFromSession(HttpContext.User.Identity.Name);
                updatedQuiz = AgonManager.UpdateQuestions(questionText, answerText, jsonQuiz, id);
                await MongoManager.ReplaceOneQuizAsync(updatedQuiz.Owner, updatedQuiz.Name, JsonConvert.SerializeObject(updatedQuiz), "Quizzes");

                var currentQuiz = JsonConvert.SerializeObject(updatedQuiz);
                await MongoManager.SaveQuizToSession(currentQuiz, HttpContext.User.Identity.Name);
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                return RedirectToAction("Error", "Home");
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                return RedirectToAction("Error", "Home");
            }


            return RedirectToAction("EditQuiz", "Quiz");
        }
        [HttpGet]
        public async Task<IActionResult> EditQuiz() // 2016-11-25 21:31 - Här kan man ta in _id och få det från Home/ViewPlaylists, om man vill och behöver
        {
            try
            {
                var currentQuiz = await MongoManager.GetQuizFromSession(HttpContext.User.Identity.Name);
                if (currentQuiz != null && currentQuiz != "")
                {

                    var quizToEdit = JsonConvert.DeserializeObject<Quiz>(currentQuiz);

                    await SaveQuiz();

                    return View(quizToEdit);
                }

                else
                {
                    return RedirectToAction("UserLoggedIn", "Home");
                }
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                return RedirectToAction("Error", "Home");
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditQuiz(string _id)
        {
            try
            {
                var quiz = await MongoManager.GetOneQuizAsync(_id, "Quizzes");
                await MongoManager.SaveQuizToSession(quiz, HttpContext.User.Identity.Name);
                return RedirectToAction("EditQuiz");
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                return RedirectToAction("Error", "Home");
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task RemoveQuiz(string id)
        {
            try
            {
                await MongoManager.DeleteDocument(id, "Quizzes");

            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                RedirectToError();
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                RedirectToError();
            }

        }
        public async Task SaveQuiz(string quizName = null)
        {

            try
            {
                var jsonQuiz = await MongoManager.GetQuizFromSession(HttpContext.User.Identity.Name);
                var quiz = JsonConvert.DeserializeObject<Quiz>(jsonQuiz);
                if (quizName != null)
                {
                    quiz.Name = quizName;

                    jsonQuiz = JsonConvert.SerializeObject(quiz);

                    await MongoManager.ReplaceOneQuizAsync(quiz._id, jsonQuiz, "Quizzes");
                }
                else if (await MongoManager.CheckIfDocumentExistsAsync(quiz.Owner, quiz.Name, "Quizzes"))
                {
                    await MongoManager.ReplaceOneQuizAsync(quiz.Owner, quiz.Name, jsonQuiz, "Quizzes");
                }
                else
                {
                    await MongoManager.SaveDocumentAsync(jsonQuiz);
                }

                await MongoManager.SaveQuizToSession(jsonQuiz, quiz.Owner);
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                RedirectToError();
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                RedirectToError();
            }
        }

        public async Task<IActionResult> StartQuiz(string _id)
        {
            try
            {
                QuizMasterVM quizMasterVM = await AgonManager.StartQuiz(_id);
                return View(quizMasterVM);
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                return RedirectToAction("Error", "Home");
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }


        public async Task DropPin(string id)
        {
            try
            {
                await MongoManager.RemovePinFromQuiz(id, "runningQuizzes");
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                RedirectToError();
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                RedirectToError();
            }
        }
        [AllowAnonymous]
        public async Task<IActionResult> PlayQuiz(string pin, string validusername)
        {
            QuizPlayerVM quizPlayerVM;
            bool pinExists;

            try
            {
                pinExists = await MongoManager.CheckIfPinExistsAsync(pin, "runningQuizzes");
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                return RedirectToAction("Error", "Home");
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                return RedirectToAction("Error", "Home");
            }

            if (pinExists)
            {
                try
                {
                    quizPlayerVM = await AgonManager.CreateQuizPlayerVM(pin, validusername);
                }
                catch (MongoException mex)
                {
                    HttpContext.Session.SetString("error", mex.Message);
                    return RedirectToAction("Error", "Home");
                }
                catch (Exception ex)
                {
                    HttpContext.Session.SetString("error", ex.Message);
                    return RedirectToAction("Error", "Home");
                }

                string cacheKey = pin;
                var players = _memoryCache.Get<List<string>>(cacheKey);

                if (players == null)
                {
                    players = new List<string>();
                }

                players.Add(validusername);
                _memoryCache.Set<List<string>>(cacheKey, players);

                return View(quizPlayerVM);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(string SubmitterName)
        {
            var answers = Request.Form["answer"];
            var id = Request.Form["runningQuizId"];
            try
            {
                await AgonManager.SaveAnswerAsync(answers, id, SubmitterName);

                string cacheKey = id;
                var submits = _memoryCache.Get<List<string>>(cacheKey);

                if (submits == null)
                {
                    submits = new List<string>();
                }

                submits.Add(SubmitterName);
                _memoryCache.Set<List<string>>(cacheKey, submits);


                return View("SubmitAnswer", SubmitterName);
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                return RedirectToAction("Error", "Home");
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<bool> CheckPin(string id)
        {
            bool exists = true;
            try
            {
                exists = await MongoManager.CheckIfPinExistsAsync(id, "runningQuizzes");
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                RedirectToError();
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                RedirectToError();
            }
            return exists;
        }



        [HttpGet]
        public async Task<IActionResult> Review(string quizID)
        {
            try
            {
                AnswerKeyVM viewModel = await AgonManager.GetAnswerKeyVMAsync(quizID);
                return View(viewModel);
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                return RedirectToAction("Error", "Home");
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }



        [HttpPost]
        public async Task<IActionResult> ActuallyReview(string runningQuizID)
        {
            AnswerKeyVM viewModel;
            try
            {
                viewModel = await AgonManager.GetAnswerKeyVMAsync(runningQuizID);
            }
            catch (MongoException mex)
            {
                HttpContext.Session.SetString("error", mex.Message);
                return RedirectToAction("Error", "Home");
            }
            var submittedAnswers = viewModel.Songs
                .SelectMany(o => o.Questions.SelectMany(q => q.SubmittedAnswers))
                .ToList();

            var correctAnswerIndices = new List<int>();
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("answer_"))
                {
                    var index = int.Parse(key.Split('_')[1]);
                    correctAnswerIndices.Add(index);
                }
            }

            var namesAnsAnswers = submittedAnswers.GroupBy(o => o.SubmitterName);
            var results = new Dictionary<string, int>();
            foreach (var group in namesAnsAnswers)
            {
                var name = group.Key;
                results.Add(name, 0);
                foreach (var item in group)
                {
                    var index = submittedAnswers.IndexOf(item);
                    if (correctAnswerIndices.Contains(index))
                        results[name] = results[name] + 1;
                }
            }
            //Remove stuff from DB - maybe store highscore typ
            return View(results.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value));
        }

        public IActionResult CheckConnectedPlayers(string id)
        {
            string cacheKey = id;
            var connectedPlayers = _memoryCache.Get<List<string>>(cacheKey)?.Count;

            return Json(new { connectedPlayers });
        }

        [AllowAnonymous]
        public bool IsUsernameAvailable(string quizPin, string username)
        {
            var players = _memoryCache.Get<List<string>>(quizPin);

            if (players != null)
                if (players.Contains(username))
                {
                    return false;
                }

            return true;
        }
        private ActionResult RedirectToError()
        {
            return RedirectToAction("Error", "Home");
        }

        public IActionResult GetUsernamesOfConnectedPlayers(string id)
        {
            string cacheKey = id;
            var playerNames = _memoryCache.Get<List<string>>(cacheKey).Aggregate((x, y) => $"{x}{Environment.NewLine}{y}");

            return Json(new { playerNames });
        }
        public IActionResult GetUsernamesOfSubmits(string id)
        {
            string cacheKey = id;
            var submits = _memoryCache.Get<List<string>>(cacheKey).Aggregate((x, y) => $"{x}<br/>{y}");

            return Json(new { submits });
        }
    }
}
