using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyUtils
{
    public class SpotifyException : Exception
    {
            public SpotifyException()
            {
            }

            public SpotifyException(string message)
                : base(message)
            {
            }

            public SpotifyException(string message, Exception inner)
                : base(message, inner)
            {
            }
        
    }
}
