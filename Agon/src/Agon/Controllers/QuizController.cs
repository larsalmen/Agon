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
            catch (Exception ex)
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
            Quiz updatedQuiz;
            try
            {
                updatedQuiz = AgonManager.UpdateQuestions(questionText, answerText, jsonQuiz, id);
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("error", ex.Message);
                return RedirectToAction("Error", "Home");
            }

            await MongoManager.ReplaceOneQuizAsync(updatedQuiz.Owner, updatedQuiz.Name, JsonConvert.SerializeObject(updatedQuiz), "Quizzes");

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

                await SaveQuiz();

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
                await MongoManager.ReplaceOneQuizAsync(quiz.Owner, quiz.Name, jsonQuiz, "Quizzes");
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
                string cacheKey = pin;
                int clientsConnected = _memoryCache.Get<Int32>(cacheKey);

                if (clientsConnected == 0)
                    _memoryCache.Set<Int32>(cacheKey, 1);

                else
                    _memoryCache.Set(cacheKey, clientsConnected + 1);

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

            await AgonManager.SaveAnswerAsync(answers, id, SubmitterName);
            return View("SubmitAnswer", SubmitterName);
        }

        public async Task<bool> CheckPin(string pin)
        {
            return await MongoManager.CheckIfPinExistsAsync(pin, "runningQuizzes");
        }



        [HttpGet]
        public async Task<IActionResult> Review(string quizID)
        {
            AnswerKeyVM viewModel = await AgonManager.GetAnswerKeyVMAsync(quizID);
            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> ActuallyReview(string runningQuizID)
        {
            AnswerKeyVM viewModel = await AgonManager.GetAnswerKeyVMAsync(runningQuizID);
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


        //AnswerKeyVM GetAnswerKey()
        //{
        //    return new AnswerKeyVM
        //    {
        //        Songs =
        //        {
        //            new AnswerKeySongVM { Artist = "Aqua", Title = "Barbie Girl", Questions = {
        //                    new AnswerKeyQuestionVM { Text = "Fråga 1 om Aqua - Barie Girl", CorrectAnswer = "Rätt svar 1 om Aqua - Barbie Girl", SubmittedAnswers = {
        //                            new AnswerKeySubmittedAnswerVM { Answer = "Ett svar på en fråga", SubmitterName = "Pontus", IsCorrect = true }
        //                        } },
        //                    new AnswerKeyQuestionVM { Text = "Fråga 2 om Aqua - Barie Girl", CorrectAnswer = "Rätt svar 2 om Aqua - Barbie Girl", SubmittedAnswers = {
        //                            new AnswerKeySubmittedAnswerVM { Answer = "Två svar på en fråga", SubmitterName = "Pontus", IsCorrect = true },
        //                            new AnswerKeySubmittedAnswerVM { Answer = "Två svar på en fråga B", SubmitterName = "Anders", IsCorrect = true }
        //                        } },
        //                    new AnswerKeyQuestionVM { Text = "Fråga 3 om Aqua - Barie Girl", CorrectAnswer = "Rätt svar 3 om Aqua - Barbie Girl", SubmittedAnswers = {
        //                            new AnswerKeySubmittedAnswerVM { Answer = "Tre svar på en fråga", SubmitterName = "Pontus", IsCorrect = false }
        //                        } }
        //                }
        //            },
        //            new AnswerKeySongVM { Artist = "Helloween", Title = "Helloweenlåten med stort H", Questions = {
        //                    new AnswerKeyQuestionVM { Text = "Fråga 1 om Helloween - Helloweenlåten med stort H", CorrectAnswer = "Helloween - Helloweenlåten med stort H", SubmittedAnswers = {
        //                            new AnswerKeySubmittedAnswerVM { Answer = "Ett svar på en fråga", SubmitterName = "Pontus", IsCorrect = true }
        //                        } },
        //                    new AnswerKeyQuestionVM { Text = "Fråga 2 om Helloween - Helloweenlåten med stort H", CorrectAnswer = "Helloween - Helloweenlåten med stort H", SubmittedAnswers = {
        //                            new AnswerKeySubmittedAnswerVM { Answer = "Ett svar på en fråga", SubmitterName = "Pontus", IsCorrect = false }
        //                        } },
        //                    new AnswerKeyQuestionVM { Text = "Fråga 3 om Helloween - Helloweenlåten med stort H", CorrectAnswer = "Helloween - Helloweenlåten med stort H", SubmittedAnswers = {
        //                            new AnswerKeySubmittedAnswerVM { Answer = "Ett svar på en fråga", SubmitterName = "Anders", IsCorrect = true }
        //                        } }
        //                }
        //            }
        //        }
        //    };
        //}

        public IActionResult CheckConnectedPlayers(string id)
        {
            string cacheKey = id;
            var connectedPlayers = _memoryCache.Get(cacheKey);

            return Json(new { connectedPlayers });
        }
    }
}
