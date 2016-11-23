using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyUtils
{
    public class SpotifyTokens
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string RecievedAt { get; set; }

        public string Username { get; set; }


        public SpotifyTokens(string accessToken, string refreshToken, string recievedAt, string username)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            RecievedAt = recievedAt;
            Username = username;
        }
    }
}
