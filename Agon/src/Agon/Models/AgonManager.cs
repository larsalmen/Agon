using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpotifyUtils;
using System;
using System.Collections.Generic;
using System.Linq;
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

            

            //var songs = new List<Songs>();

            //var quiz = new Quiz() { Name = viewModel.Name + " Quiz", Songs = songs };

            return new Quiz();
        }
    }
}
