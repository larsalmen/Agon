﻿using Microsoft.AspNetCore.Http;
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

        public static SpotifyTokens GetSpotifyTokens(Controller controller)
        {
            return new SpotifyTokens(
                controller.HttpContext.Session.GetString("access_token"),
                controller.HttpContext.Session.GetString("refresh_token"),
                controller.HttpContext.Session.GetString("expires_at"),
                controller.User.Identity.Name
                );
        }


        public static void SetTokensInSession(ExternalLoginInfo info, ISession session)
        {
            var access_token = info.AuthenticationTokens.Where(x => x.Name == "access_token").Select(y => y.Value).FirstOrDefault();
            session.SetString("access_token", access_token);

            var refresh_token = info.AuthenticationTokens.Where(x => x.Name == "refresh_token").Select(y => y.Value).FirstOrDefault();
            session.SetString("refresh_token", refresh_token);

            var recieved_at = info.AuthenticationTokens.Where(x => x.Name == "expires_at").Select(y => y.Value).FirstOrDefault();
            session.SetString("recieved_at", recieved_at);
        }

        internal static Quiz UpdateQuestions(StringValues questionText, StringValues answerText, string jsonQuiz,string id)
        {
            var updatedQuiz = JsonConvert.DeserializeObject<Quiz>(jsonQuiz);

            var song = updatedQuiz.Songs.Where(s => s.SpotifyReferenceID == id).FirstOrDefault();

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
                    SpotifyReferenceID = item.Track.Href
                });

                counter++;
            }


            // Just for shits and giggles, save dis bad boy to the database.
            //var quizJson = JsonConvert.SerializeObject(quiz);

            //MongoManager.SaveQuiz(quizJson);

            return quiz;
        }
        public static EditSongVM CreateEditSongVM(string song)
        {
            var newSong = JsonConvert.DeserializeObject<Song>(song);
            var editSongVM = new EditSongVM(newSong.Artist,newSong.Title,newSong.SpotifyReferenceID);

            editSongVM.Questions.Add(new Question { Text = "What is the name of the song?", CorrectAnswer = newSong.Title });
            editSongVM.Questions.Add(new Question { Text = "What is the name of the artist?", CorrectAnswer = newSong.Artist });
            editSongVM.Questions.Add(new Question { Text = "What is the name of the album?", CorrectAnswer = newSong.AlbumTitle });
            editSongVM.Questions.Add(new Question { Text = "When was this song released?", CorrectAnswer = newSong.RealeaseDate });

            return editSongVM;
        }

        public static async Task<UserVM> GetUserVMAsync(string username, bool loggedIn)
        {
            List<Quiz> quizzes = new List<Quiz>();
            quizzes = JsonConvert.DeserializeObject<List<Quiz>>(await MongoManager.GetAllQuizzesAsync(username));
            var userVM = new UserVM(username, quizzes, loggedIn);

            return userVM;
        }
    }
}
