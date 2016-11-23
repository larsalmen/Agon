using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace MongoUtils
{
    public static class MongoManager
    {
        static public string MongoConnection { get; set; }

        public static void SetupEnvironmentVariables(string mongoConnection)
        {
            MongoConnection = mongoConnection;
        }




        public static void SaveQuiz(string quizJson)
        {
            MongoClient mongoClient = new MongoClient(MongoConnection);

            IMongoDatabase agony = mongoClient.GetDatabase("agony");

            var quizzes = agony.GetCollection<Quiz>("Quizzes");

            var document = JsonConvert.DeserializeObject<Quiz>(quizJson);

            quizzes.InsertOne(document);
        }
    }
}