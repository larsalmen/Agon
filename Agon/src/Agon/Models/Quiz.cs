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

        public List<Song> songs { get; set; }
    }
}
