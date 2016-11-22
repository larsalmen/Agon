using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoUtils
{
    public static class MongoManager
    {
        static public string MongoConnection { get; set; }

        public static void SetupEnvironmentVariables(string mongoConnection)
        {
            MongoConnection = mongoConnection;
        }
    }
}