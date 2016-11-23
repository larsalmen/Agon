using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoUtils
{
    public class Song
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public string RealeaseDate { get; set; }
        public string AlbumTitle { get; set; }
        public string SpotifyReferenceID { get; set; }

        public List<Question> Questions { get; set; }
        public Song()
        {
            Questions = new List<Question>();
        }
    }
}
