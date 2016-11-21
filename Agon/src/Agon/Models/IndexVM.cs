using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class IndexVM : ViewModelBase
    {
        public IndexVM(string title) : base(title)
        {
        }

        public string Username { get; set; }
        public List<Quiz> Quizzes { get; set; }
        public bool LoggedIn { get; set; }
    }
}
