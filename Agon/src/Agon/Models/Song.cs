using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class Song
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public string SpotifyReferenceID { get; set; }

        public List<Question> Questions { get; set; }
    }
}
