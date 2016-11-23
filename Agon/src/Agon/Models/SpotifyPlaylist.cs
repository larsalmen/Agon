using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class SpotifyPlaylist
    {
        public string href { get; set; }

        [JsonConstructor]
        public SpotifyPlaylist(string href)
        {
            this.href = href;
        }
    }
}
