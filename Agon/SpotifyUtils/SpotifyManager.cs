using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpotifyUtils
{
    public static class SpotifyManager
    {
        public static string SpotifyClientId { get; set; }
        public static string SpotifyClientSecret { get; set; }


        public static void SetVariables(string spotifyClientId, string spotifyClientSecret)
        {
            SpotifyClientId = spotifyClientId;
            SpotifyClientSecret = spotifyClientSecret;
        }

        public static async Task<ListOfPlaylists> GetAllUserPlaylists(SpotifyTokens token)
        {
            // Checks if the accestoken has expired.
            CheckToken(token);

            string endpoint = @"https://api.spotify.com/v1/users/" + token.Username + "/playlists";

            WebHeaderCollection headerCollection = new WebHeaderCollection();

            headerCollection.Add("Accept-Encoding:gzip,deflate,compress");
            headerCollection.Add("Authorization:Bearer " + token.AccessToken);

            var request = WebRequest.CreateHttp(endpoint);
            request.Host = "api.spotify.com";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Headers = headerCollection;

            var response = await request.GetResponseAsync();
            // Get some ifs in here to check whether gzip or other encoding is used. Also, try-catch.
            string text = "";
            using (var stream = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress)))
            {
                text = stream.ReadToEnd();
            }

            var listOfPlaylists = JsonConvert.DeserializeObject<ListOfPlaylists>(text);
            listOfPlaylists.Items = listOfPlaylists.Items.Where(i => i.Tracks.Total < 20).ToList();
            return listOfPlaylists;
        }

        public static async Task<ListOfSongs> GetAllSongsFromPlaylist(SpotifyTokens token, string spotifyRef)
        {
            // Checks if the accestoken has expired.
            CheckToken(token);

            string endpoint = @"https://api.spotify.com/v1/users/" + token.Username + "/playlists/" + spotifyRef + "/tracks?fields=items(track(artists,name,href,album(name,href)))";
            //string endpoint = @"https://api.spotify.com/v1/users/" + token.Username + "/playlists/" + spotifyRef + "/";

            WebHeaderCollection headerCollection = new WebHeaderCollection();

            headerCollection.Add("Accept-Encoding:gzip,deflate,compress");
            headerCollection.Add("Authorization:Bearer " + token.AccessToken);

            var request = WebRequest.CreateHttp(endpoint);
            request.Host = "api.spotify.com";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Headers = headerCollection;

            var response = await request.GetResponseAsync();
            // Get some ifs in here to check whether gzip or other encoding is used. Also, try-catch.
            string text = "";
            using (var stream = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress)))
            {
                text = stream.ReadToEnd();
            }

            var listOfSongs = JsonConvert.DeserializeObject<ListOfSongs>(text);

            var albumInfo = await GetAlbumInfo(token, listOfSongs);
            listOfSongs.AlbumInfo = albumInfo;
            return listOfSongs;
        }

        private static async Task<ListOfAlbumInfo> GetAlbumInfo(SpotifyTokens token, ListOfSongs listOfSongs)
        {
            // Checks if the accestoken has expired.
            CheckToken(token);

            StringBuilder endpoint = new StringBuilder();
            endpoint.Append(@"https://api.spotify.com/v1/albums?ids=");

            foreach (var item in listOfSongs.Items)
            {
                endpoint.Append(item.Track.Album.Href + ",");
            }
            endpoint.Remove(endpoint.Length - 1, 1);

            WebHeaderCollection headerCollection = new WebHeaderCollection();

            headerCollection.Add("Accept-Encoding:gzip,deflate,compress");
            headerCollection.Add("Authorization:Bearer " + token.AccessToken);

            var request = WebRequest.CreateHttp(endpoint.ToString());
            request.Host = "api.spotify.com";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Headers = headerCollection;

            var response = await request.GetResponseAsync();
            // Get some ifs in here to check whether gzip or other encoding is used. Also, try-catch.
            string text = "";
            using (var stream = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress)))
            {
                text = stream.ReadToEnd();
            }

            var albumInfo = JsonConvert.DeserializeObject<ListOfAlbumInfo>(text);
            return albumInfo;
        }
        private static void CheckToken(SpotifyTokens token)
        {
            // Parses the timestamp to Datetime, adds expiration time and checks if that time has passed.
            if (DateTime.Parse(token.Timestamp).AddSeconds(3540) < DateTime.Now)
            {
                // Builds a correct refresh request to post to spotify.
                var endpoint = @"https://accounts.spotify.com/api/token";
                string authHeader = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(SpotifyClientId + ":" + SpotifyClientSecret));
                var postData = "grant_type=refresh_token";
                postData += "&refresh_token=" + token.RefreshToken;
                var data = Encoding.ASCII.GetBytes(postData);

                var request = WebRequest.CreateHttp(endpoint);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers.Set("Authorization", authHeader);
                request.ContentLength = data.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                }

                // Gets the response from Spotify.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                   // Get some cool error handling in here.
                }

                // Reads the streams content into the responseBody variable.
                var responseBody = "";

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    responseBody = reader.ReadToEnd();
                }

                // Sets the definition for an anon.object to use during deserialization.
                var def = new { access_token = "" };

                var newAccessToken = JsonConvert.DeserializeAnonymousType(responseBody, def);

                // Sets the new token and timestamp. Discards the rest.
                token.AccessToken = newAccessToken.access_token;
                token.Timestamp = DateTime.Now.ToString();
            }
        }
    }
}
