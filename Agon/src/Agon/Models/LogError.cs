using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class LogError
    {
        public string User { get; set; }
        public DateTime Time { get; set; }
        public string ErrorMessage { get; set; }

        public double Version { get; set; }

        public LogError(string user, DateTime time, string errorMessage, double version = 1.0)
        {
            User = user;
            Time = time;
            ErrorMessage = errorMessage;
            Version = version;
        }
    }
}
