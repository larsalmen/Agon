using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class QuizMasterVM : RunningQuiz
    {
        public QuizMasterVM(RunningQuiz runningQuiz) : base()
        {
            Name = runningQuiz.Name;
            Owner = runningQuiz.Owner;
            Pin = runningQuiz.Pin;
            Songs = runningQuiz.Songs;
            _id = runningQuiz._id;
        }
    }
}
