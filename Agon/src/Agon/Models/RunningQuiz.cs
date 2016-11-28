using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class RunningQuiz : Quiz
    {
        public RunningQuiz() : base()
        {

        }
        public int Pin { get; set; }

    }
}
