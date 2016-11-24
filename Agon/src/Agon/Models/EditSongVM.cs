using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class EditSongVM
    {
        public string Artist;
        public string Title;
        public string SpotifyReferenceID { get; set; }

        public List<Question> Questions { get; set; }
        public EditSongVM(string artist, string title, string spotifyReferenceID)
        {
            Artist = artist;
            Title = title;
            SpotifyReferenceID = spotifyReferenceID;
            Questions = new List<Question>();
        }
        public EditSongVM()
        {

        }
    }
}
