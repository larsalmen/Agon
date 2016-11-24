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

            return quiz;
        }
    }
}
