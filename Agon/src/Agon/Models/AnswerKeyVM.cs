using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class AnswerKeyVM
    {
        public AnswerKeyVM()
        {
            Songs = new List<AnswerKeySongVM>();
        }
        public List<AnswerKeySongVM> Songs { get; set; }
    }

    public class AnswerKeySongVM
    {
        public AnswerKeySongVM()
        {
            Questions = new List<Models.AnswerKeyQuestionVM>();
        }
        public string Artist { get; set; }
        public string Title { get; set; }
        public List<AnswerKeyQuestionVM> Questions { get; set; }
    }

    public class AnswerKeyQuestionVM
    {
        public AnswerKeyQuestionVM()
        {
            SubmittedAnswers = new List<Models.AnswerKeySubmittedAnswerVM>();
        }
        public string Text { get; set; }
        public string CorrectAnswer { get; set; }
        public List<AnswerKeySubmittedAnswerVM> SubmittedAnswers { get; set; }
    }

    public class AnswerKeySubmittedAnswerVM
    {
        public string SubmitterName { get; set; }
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }
    }
}
