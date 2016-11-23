using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoUtils
{
    public class Answer
    {
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public string Submitter { get; set; }
    }
}
