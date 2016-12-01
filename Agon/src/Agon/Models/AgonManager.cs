using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoUtils;
using Newtonsoft.Json;
using SpotifyUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Caching.Memory;

namespace Agon.Models
{
    public static class AgonManager
    {
        static Random randomizer = new Random();

        public static SpotifyTokens GetSpotifyTokens(Controller controller)
        {
            var token = new SpotifyTokens(
                controller.HttpContext.Session.GetString("access_token"),
                controller.HttpContext.Session.GetString("refresh_token"),
                controller.HttpContext.Session.GetString("timestamp"),
                controller.User.Identity.Name
                );

            return token;
        }
        public static void SetTokensInSession(ExternalLoginInfo info, ISession session)
        {
            var access_token = info.AuthenticationTokens.Where(x => x.Name == "access_token").Select(y => y.Value).FirstOrDefault();
            session.SetString("access_token", access_token);

            var refresh_token = info.AuthenticationTokens.Where(x => x.Name == "refresh_token").Select(y => y.Value).FirstOrDefault();
            session.SetString("refresh_token", refresh_token);

            var timestamp = DateTime.Parse(
                info.AuthenticationTokens.Where(x => x.Name == "expires_at").Select(y => y.Value).FirstOrDefault()
                ).ToUniversalTime().ToString();

            session.SetString("timestamp", timestamp);
        }

        public static async Task RemoveSongFromQuiz(SpotifyTokens token, int indexToRemove)
        {
            string currentQuizJson;
            Quiz currentQuiz;

            currentQuizJson = await MongoManager.GetQuizFromSession(token.Username);
            currentQuiz = JsonConvert.DeserializeObject<Quiz>(currentQuizJson);

            currentQuiz.Songs.RemoveAt(indexToRemove);

            currentQuizJson = JsonConvert.SerializeObject(currentQuiz);
            await MongoManager.SaveQuizToSession(currentQuizJson, token.Username);


        }

        internal static Quiz UpdateQuestions(StringValues questionText, StringValues answerText, string jsonQuiz, string id)
        {
            Quiz updatedQuiz;
            updatedQuiz = JsonConvert.DeserializeObject<Quiz>(jsonQuiz);

            var song = updatedQuiz.Songs.Where(s => s.SpotifyReferenceID == id).FirstOrDefault();

            song.Questions.Clear();
            for (int i = 0; i < questionText.Count; i++)
            {
                song.Questions.Add(new Question { Text = questionText[i], CorrectAnswer = answerText[i] });
            }


            return updatedQuiz;
        }

        public static async Task<List<PlaylistVM>> GetPlaylists(SpotifyTokens token)
        {
            ListOfPlaylists allReturnedPlaylists;

            allReturnedPlaylists = await SpotifyManager.GetAllUserPlaylists(token);

            var playlists = new List<PlaylistVM>();

            foreach (var item in allReturnedPlaylists.Items)
            {
                playlists.Add(new PlaylistVM(item.Name, item.Tracks.Href));
            }

            return playlists;
        }
        public static async Task<Quiz> GenerateQuiz(SpotifyTokens token, PlaylistVM viewModel)
        {
            ListOfSongs allReturnedSongs;

            allReturnedSongs = await SpotifyManager.GetAllSongsFromPlaylist(token, viewModel.SpotifyRef);


            var quiz = new Quiz();
            quiz._id = Guid.NewGuid().ToString();

            quiz.Name = viewModel.Name;
            quiz.Owner = token.Username;

            int counter = 0;

            foreach (var item in allReturnedSongs.Items)
            {

                StringBuilder builder = new StringBuilder();

                foreach (var artist in item.Track.Artists)
                {
                    builder.Append(artist.Name);
                    if (artist != item.Track.Artists[item.Track.Artists.Count - 1])
                        builder.Append(", ");
                }

                quiz.Songs.Add(new Song
                {
                    Title = item.Track.Name,
                    Artist = builder.ToString(),
                    AlbumTitle = item.Track.Album.Name,
                    RealeaseDate = allReturnedSongs.AlbumInfo.AlbumInfo[counter].ReleaseDate,
                    SpotifyReferenceID = item.Track.Href,
                    PreviewUrl = item.Track.PreviewUrl
                });

                counter++;
            }
            return quiz;
        }

        public static async Task<QuizMasterVM> StartQuiz(string _id)
        {
            await MongoManager.ClearOldAnswers(_id);
            int pin;
            // Hämta quiz från quizzes
            RunningQuiz runningQuiz;

            runningQuiz = JsonConvert.DeserializeObject<RunningQuiz>(await MongoManager.GetOneQuizAsync(_id, "Quizzes"));

            do
            {
                pin = randomizer.Next(9999);
            }
            while (await MongoManager.CheckIfPinExistsAsync(pin.ToString(), "runningQuizzes"));


            runningQuiz.Pin = pin.ToString();
            // Stoppa ner quizzet med PIN i runningQuizzes

            if (await MongoManager.CheckIfDocumentExistsAsync(runningQuiz._id, "runningQuizzes"))
            {
                await MongoManager.ReplaceOneQuizAsync(runningQuiz._id, JsonConvert.SerializeObject(runningQuiz), "runningQuizzes");
            }
            else
                await MongoManager.SaveDocumentAsync("runningQuizzes", JsonConvert.SerializeObject(runningQuiz));
            // Generera QuizMasterVM och returnera

            return new QuizMasterVM(runningQuiz);

        }

        public static async Task<Song> AddSongToQuiz(SpotifyTokens token, string href)
        {
            Track newTrack;
            string albumDate;

            newTrack = await SpotifyManager.GetOneSong(token, href);
            albumDate = await SpotifyManager.GetOneAlbum(token, newTrack.Album.Href);

            StringBuilder builder = new StringBuilder();

            foreach (var artist in newTrack.Artists)
            {
                builder.Append(artist.Name);
                if (artist != newTrack.Artists[newTrack.Artists.Count - 1])
                    builder.Append(", ");
            }

            var newSong = new Song
            {
                Artist = builder.ToString(),
                Title = newTrack.Name,
                RealeaseDate = albumDate,
                AlbumTitle = newTrack.Album.Name,
                SpotifyReferenceID = newTrack.Href,
                PreviewUrl = newTrack.PreviewUrl
            };
            return newSong;
        }

        public static async Task SaveAnswerAsync(StringValues answers, string _id, string submitterName)
        {
            var answerForm = new AnswerForm();
            foreach (var answer in answers)
            {
                answerForm.Answers.Add(answer);
            }
            answerForm.RunningQuizId = _id;
            answerForm.SubmitterName = submitterName;
            answerForm.Timestamp = DateTime.Now.ToString();

            // Add cache-counter.

            var jsonAnswer = JsonConvert.SerializeObject(answerForm);
            await MongoManager.SaveDocumentAsync("answers", jsonAnswer);

        }

        public static async Task<AnswerKeyVM> GetAnswerKeyVMAsync(string quizID)
        {
            AnswerKeyVM daBoss = new AnswerKeyVM();
            daBoss.RunningQuizID = quizID;

            var runningQuizJson = await MongoManager.GetOneQuizAsync(quizID, "runningQuizzes");
            var runningQuiz = JsonConvert.DeserializeObject<RunningQuiz>(runningQuizJson);

            var submittedAnswersJson = await MongoManager.GetAllAnswerFormsAsync(quizID, "answers");
            var submittedAnswers = JsonConvert.DeserializeObject<List<AnswerForm>>(submittedAnswersJson);

            // Stoppa in allt på rätt sätt i rätt AKVM

            var answerkeySongs = new List<AnswerKeySongVM>();

            int counter = 0;
            for (int i = 0; i < runningQuiz.Songs.Count; i++)
            {
                var questions = new List<AnswerKeyQuestionVM>();
                for (int j = 0; j < runningQuiz.Songs[i].Questions.Count; j++)
                {
                    var answers = new List<AnswerKeySubmittedAnswerVM>();
                    string answer = "";

                    for (int k = 0; k < submittedAnswers.Count; k++)
                    {
                        answer = submittedAnswers[k].Answers[counter];
                        answers.Add(new AnswerKeySubmittedAnswerVM { Answer = answer, SubmitterName = submittedAnswers[k].SubmitterName, IsCorrect = (answer.ToLower() == (runningQuiz.Songs[i].Questions[j].CorrectAnswer).ToLower()) });
                    }
                    var correctAnswer = runningQuiz.Songs[i].Questions[j].CorrectAnswer;
                    var text = runningQuiz.Songs[i].Questions[j].Text;
                    questions.Add(new AnswerKeyQuestionVM { CorrectAnswer = correctAnswer, Text = text, SubmittedAnswers = new List<AnswerKeySubmittedAnswerVM>() });
                    questions[j].SubmittedAnswers.AddRange(answers);
                    counter++;
                }
                var artist = runningQuiz.Songs[i].Artist;
                var title = runningQuiz.Songs[i].Title;
                answerkeySongs.Add(new AnswerKeySongVM { Artist = artist, Title = title, Questions = new List<AnswerKeyQuestionVM>() });
                answerkeySongs[i].Questions.AddRange(questions);
            }

            daBoss.Songs.AddRange(answerkeySongs);
            return daBoss;
        }

        public static EditSongVM CreateEditSongVM(string song)
        {
            var newSong = JsonConvert.DeserializeObject<Song>(song);
            var editSongVM = new EditSongVM(newSong.Artist, newSong.Title, newSong.SpotifyReferenceID);

            if (newSong.Questions.Count == 0)
            {
                editSongVM.Questions.Add(new Question { Text = "What is the name of the song?", CorrectAnswer = newSong.Title });
                editSongVM.Questions.Add(new Question { Text = "What is the name of the artist?", CorrectAnswer = newSong.Artist });
                editSongVM.Questions.Add(new Question { Text = "What is the name of the album?", CorrectAnswer = newSong.AlbumTitle });
                editSongVM.Questions.Add(new Question { Text = "When was this song released?", CorrectAnswer = newSong.RealeaseDate });
            }
            else
            {
                foreach (var question in newSong.Questions)
                {
                    editSongVM.Questions.Add(new Question { Text = question.Text, CorrectAnswer = question.CorrectAnswer });

                }

            }


            return editSongVM;
        }

        public static async Task<UserVM> GetUserVMAsync(string username, bool loggedIn)
        {
            List<Quiz> quizzes = new List<Quiz>();

            quizzes = JsonConvert.DeserializeObject<List<Quiz>>(await MongoManager.GetAllQuizzesAsync(username));

            var userVM = new UserVM(username, quizzes, loggedIn);

            return userVM;
        }

        public static async Task<QuizPlayerVM> CreateQuizPlayerVM(string pin)
        {

            var quizPlayerVM = new QuizPlayerVM(JsonConvert.DeserializeObject<RunningQuiz>(await MongoManager.GetOneQuizByPinAsync(pin, "runningQuizzes")));
            return quizPlayerVM;

        }
    }
}
