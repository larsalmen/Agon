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

        internal static Quiz UpdateQuestions(StringValues questionText, StringValues answerText, string jsonQuiz, string id)
        {
            var updatedQuiz = JsonConvert.DeserializeObject<Quiz>(jsonQuiz);

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
            var allReturnedPlaylists = await SpotifyManager.GetAllUserPlaylists(token);

            var playlists = new List<PlaylistVM>();

            foreach (var item in allReturnedPlaylists.Items)
            {
                playlists.Add(new PlaylistVM(item.Name, item.Tracks.Href));
            }

            return playlists;
        }
        public static async Task<Quiz> GenerateQuiz(SpotifyTokens token, PlaylistVM viewModel)
        {
            var allReturnedSongs = await SpotifyManager.GetAllSongsFromPlaylist(token, viewModel.SpotifyRef);

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

            // This checks if a quiz exists, and if it does it does NOT try to save it.
            if (!await MongoManager.CheckIfDocumentExistsAsync(quiz.Owner, quiz.Name, "Quizzes"))
            {
                var quizJson = JsonConvert.SerializeObject(quiz);
                await MongoManager.SaveDocumentAsync(quizJson);
            }
            return quiz;
        }

        public static async Task<QuizMasterVM> StartQuiz(string _id)
        {
            int pin;
            // Hämta quiz från quizzes
            var runningQuiz = JsonConvert.DeserializeObject<RunningQuiz>(await MongoManager.GetOneQuizAsync(_id, "Quizzes"));

            // Fixa PIN som inte finns bland quizzar i runningQuizzes
            do
            {
                pin = randomizer.Next(9999);
            }
            while (await MongoManager.CheckIfPinExistsAsync(pin.ToString(), "runningQuizzes"));

            runningQuiz.Pin = pin.ToString();
            // Stoppa ner quizzet med PIN i runningQuizzes
            if (await MongoManager.CheckIfDocumentExistsAsync(runningQuiz._id, "runningQuizzes"))
            {
                await MongoManager.ReplaceOneQuizAsync(runningQuiz.Owner, runningQuiz._id, JsonConvert.SerializeObject(runningQuiz), "runningQuizzes");
            }
            else
                await MongoManager.SaveDocumentAsync("runningQuizzes", JsonConvert.SerializeObject(runningQuiz));
            // Generera QuizMasterVM och returnera

            return new QuizMasterVM(runningQuiz);
        }

        public static async Task<Song> AddSongToQuiz(SpotifyTokens token, string href)
        {
            var newTrack = await SpotifyManager.GetOneSong(token, href);

            var albumDate = await SpotifyManager.GetOneAlbum(token, newTrack.Album.Href);
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
                SpotifyReferenceID = newTrack.Href
            };
            return newSong;
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

        public static async Task<QuizPlayerVM>CreateQuizPlayerVM(string pin)
        {
            var quizPlayerVM = new QuizPlayerVM(JsonConvert.DeserializeObject<RunningQuiz>(await MongoManager.GetOneQuizByPinAsync(pin, "runningQuizzes")));
            return quizPlayerVM;
        }
    }
}
