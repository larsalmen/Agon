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
            try
            {
                CheckToken(token);
            }
            catch (Exception ex)
            {
                throw new SpotifyException("SpotifyManager:Checktoken failed. This is a known problem, logout and log back in to solve the problem.", ex.InnerException);
            }
            string endpoint = @"https://api.spotify.com/v1/users/" + token.Username + "/playlists";
            ListOfPlaylists listOfPlaylists;
            try
            {
                string text = await HttpRequest(token, endpoint);
                listOfPlaylists = JsonConvert.DeserializeObject<ListOfPlaylists>(text);
                listOfPlaylists.Items = listOfPlaylists.Items.Where(i => i.Tracks.Total < 20).ToList();
                return listOfPlaylists;
            }
            catch (HttpException ex)
            {
                throw new SpotifyException("SpotifyManager:GetAllUserPlaylists, HttpRequest failed.", ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new SpotifyException("SpotifyManager:GetAllUserPlaylists, failed to deserializeObject<ListOfPlaylists>", ex.InnerException);
            }
        }
        public static async Task<ListOfSongs> GetAllSongsFromPlaylist(SpotifyTokens token, string spotifyRef)
        {
            // Checks if the accestoken has expired.
            try
            {
                CheckToken(token);
            }
            catch (Exception ex)
            {
                throw new SpotifyException("SpotifyManager:Checktoken failed. This is a known problem, logout and log back in to solve the problem.", ex.InnerException);
            }

            string endpoint = @"https://api.spotify.com/v1/users/" + token.Username + "/playlists/" + spotifyRef + "/tracks?fields=items(track(artists,name,href,preview_url,album(name,href)))";


            ListOfSongs listOfSongs;

            try
            {
                string text = await HttpRequest(token, endpoint);
                listOfSongs = JsonConvert.DeserializeObject<ListOfSongs>(text);
                var albumInfo = await GetAlbumInfo(token, listOfSongs);
                listOfSongs.AlbumInfo = albumInfo;
                return listOfSongs;
            }
            catch (HttpException ex)
            {
                throw new SpotifyException("SpotifyManager: GetAllSongsFromPlaylist, HttpRequest failed.", ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new SpotifyException("SpotifyManager:GetAllSongsFromPlaylist, failed to DeserializeObject<ListOfSongs>", ex.InnerException);
            }

        }
        private static async Task<ListOfAlbumInfo> GetAlbumInfo(SpotifyTokens token, ListOfSongs listOfSongs)
        {
            // Checks if the accestoken has expired.
            try
            {
                CheckToken(token);
            }
            catch (Exception ex)
            {
                throw new SpotifyException("SpotifyManager:Checktoken failed. This is a known problem, logout and log back in to solve the problem.", ex.InnerException);
            }

            StringBuilder endpoint = new StringBuilder();
            endpoint.Append(@"https://api.spotify.com/v1/albums?ids=");

            foreach (var item in listOfSongs.Items)
            {
                endpoint.Append(item.Track.Album.Href + ",");
            }
            endpoint.Remove(endpoint.Length - 1, 1);

            try
            {
                string text = await HttpRequest(token, endpoint.ToString());
                var albumInfo = JsonConvert.DeserializeObject<ListOfAlbumInfo>(text);
                return albumInfo;
            }
            catch (HttpException ex)
            {
                throw new SpotifyException("SpotifyManager: GetAlbumInfo, HttpRequest failed.", ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new SpotifyException("SpotifyManager:GetAlbumInfo - Failed to DeserializeObject<ListOfAlbumInfo>.", ex.InnerException);
            }
        }
        private static async Task<string> HttpRequest(SpotifyTokens token, string endpoint)
        {
            string text = "";
            try
            {
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
                using (var stream = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress)))
                {
                    text = stream.ReadToEnd();
                }
                return text;
            }
            catch (Exception ex)
            {
                throw new HttpException("Communication with Spotify failed.", ex.InnerException);
            }
        }
        private static void CheckToken(SpotifyTokens token)
        {
            try
            {
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
            catch (Exception ex)
            {
                throw new HttpException("SpotifyManager:CheckToken. TokenRefresh failed.", ex.InnerException);
            }
        }
        public static async Task<string> GetOneAlbum(SpotifyTokens token, string albumHref)
        {
            var endpoint = ("https://api.spotify.com/v1/albums/" + albumHref);
            string text;
            try
            {
                CheckToken(token);
            }
            catch (Exception ex)
            {
                throw new SpotifyException("SpotifyManager:Checktoken failed. This is a known problem, logout and log back in to solve the problem.", ex.InnerException);
            }
            try
            {
                text = await HttpRequest(token, endpoint);
            }
            catch (Exception ex)
            {
                throw new HttpException("Communication with Spotify failed.", ex.InnerException);
            }

            try
            {
                var def = new { release_date = "" };

                var releasedate = JsonConvert.DeserializeAnonymousType(text, def);

                return releasedate.release_date;
            }
            catch (Exception ex)
            {
                throw new SpotifyException("SpotifyManager:GetOneAlbum, failed to deserializeAnonymousType", ex.InnerException);
            }
        }
        public static async Task<Track> GetOneSong(SpotifyTokens token, string href)
        {
            try
            {
                CheckToken(token);
            }
            catch (Exception ex)
            {
                throw new SpotifyException("SpotifyManager:Checktoken failed. This is a known problem, logout and log back in to solve the problem.", ex.InnerException);
            }
            string text;
            try
            {
                text = await HttpRequest(token, href);
            }
            catch (Exception ex)
            {
                throw new HttpException("Communication with Spotify failed.", ex.InnerException);
            }
            try
            {
                Track newTrack = JsonConvert.DeserializeObject<Track>(text);
                return newTrack;
            }
            catch (Exception ex)
            {
                throw new SpotifyException("SpotifyManager:GetOneSong, failed to deserializeObject<Track>", ex.InnerException);
            }

        }
    }
}
