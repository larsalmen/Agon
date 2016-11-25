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
        // add constants instead of strings in methods.
        const string databaseName = "agony";
        const string collection = "Quizzes";

        static public string MongoConnection { get; set; }
        static MongoClient mongoClient;

        public static void SetupMongoClient(string mongoConnection)
        {
            MongoConnection = mongoConnection;
            mongoClient = new MongoClient(MongoConnection);
        }


        public async static Task SaveQuizAsync(string quizJson)
        {
            var agony = mongoClient.GetDatabase(databaseName);

            var quizzes = agony.GetCollection<BsonDocument>(collection);

            var quiz = BsonDocument.Parse(quizJson);
            try
            {
                await quizzes.InsertOneAsync(quiz);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async static Task<string> GetOneQuizAsync(string owner, string quizName)
        {
            var agony = mongoClient.GetDatabase(databaseName);

            var quizzes = agony.GetCollection<BsonDocument>(collection);

            BsonDocument quiz;

            try
            {
                quiz = await quizzes.Find($"{{ Owner: '{owner}', Name: '{quizName}' }}").FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return quiz.ToJson();
        }

        public static async Task<string> GetAllQuizzesAsync(string owner)
        {
            var agony = mongoClient.GetDatabase(databaseName);

            var quizzes = agony.GetCollection<BsonDocument>(collection);

            List<BsonDocument> listOfQuizzes = new List<BsonDocument>();
            try
            {
                listOfQuizzes = await quizzes.Find($"{{ Owner: '{owner}'}}").ToListAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return listOfQuizzes.ToArray().ToJson();
        }

        public static async Task UpdateOneQuizAsync(string owner, string _id, string quizJson)
        {
            var agony = mongoClient.GetDatabase(databaseName);

            var quizzes = agony.GetCollection<BsonDocument>(collection);

            var quiz = BsonDocument.Parse(quizJson);
            try
            {
                await quizzes.FindOneAndReplaceAsync<BsonDocument>($"{{ Owner: '{owner}', _id: '{_id}'}}", quiz);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}