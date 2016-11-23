using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class PlaylistVM : ViewModelBase
    {
        public string Name { get; set; }
        public string SpotifyRef { get; set; }

        public PlaylistVM(string name, string spotifyRef, string title): base(title)
        {
            Name = name;
            SpotifyRef = spotifyRef;
        }

    }
}
