using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class AnswerKeyVM
    {
        public string _id { get; set; }
        public List<Song> Songs { get; set; }
        public List<AnswerForm> SubmittedAnswer { get; set; }
    }
}
