using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoUtils
{
    public class MongoException : Exception
    {
        public MongoException()
        {
        }

        public MongoException(string message)
                : base(message)
            {
        }

        public MongoException(string message, Exception inner)
                : base(message, inner)
            {
        }
    }
}
