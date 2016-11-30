using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyUtils
{
    public class HttpException : Exception
    {

        public HttpException()
        {
        }

        public HttpException(string message)
                : base(message)
            {
        }

        public HttpException(string message, Exception inner)
                : base(message, inner)
            {
        }
    }
}
