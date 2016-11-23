using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class Quiz
    {
        public string Name { get; set; }
        public string Owner { get; set; }

        public List<Song> Songs { get; set; }
        public Quiz()
        {
            Songs = new List<Song>();
        }
    }
}
