using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class UserVM : ViewModelBase
    {

        public string Username { get; set; }
        public List<Quiz> Quizzes { get; set; }
        public bool LoggedIn { get; set; }
        public UserVM(string title, string username, List<Quiz>quizzes, bool loggedIn) : base(title)
        {
            Username = username;
            Quizzes = quizzes;
            LoggedIn = loggedIn;
        }
    }
}
