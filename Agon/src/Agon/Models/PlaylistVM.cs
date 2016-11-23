using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class PlaylistVM
    {
        public string Name { get; set; }
        public string SpotifyRef { get; set; }

        public PlaylistVM()
        {

        }

        public PlaylistVM(string name, string spotifyRef)
        {
            Name = name;
            SpotifyRef = spotifyRef;
        }
    }
}
