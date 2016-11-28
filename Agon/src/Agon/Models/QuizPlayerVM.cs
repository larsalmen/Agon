using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class QuizPlayerVM : Quiz
    {
        public QuizPlayerVM(RunningQuiz runningQuiz) : base()
        {
            _id = runningQuiz._id;
            Name = runningQuiz.Name;
            Owner = runningQuiz.Owner;
            Songs = runningQuiz.Songs;
        }
        [Required]
        public string PlayerName { get; set; }

    }
}
