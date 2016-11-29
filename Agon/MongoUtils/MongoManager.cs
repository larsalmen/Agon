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
        // All collections have indexes and constraints, see readme.
        const string databaseName = "agony";
        const string quizCollection = "Quizzes";
        const string sessionCollection = "session";

        static public string MongoConnection { get; set; }
        static MongoClient mongoClient;

        public static void SetupMongoClient(string mongoConnection)
        {
            MongoConnection = mongoConnection;
            mongoClient = new MongoClient(MongoConnection);
        }

        /// <summary>
        /// Parses the incoming JSON string to BSONDocument and saves it to the default Quiz collection.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async static Task SaveDocumentAsync(string input)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(quizCollection);

            var quiz = BsonDocument.Parse(input);
            try
            {
                await col.InsertOneAsync(quiz);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Parses the incoming JSON string to BSONDocument and saves it to the specified collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async static Task SaveDocumentAsync(string collection, string input)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(collection);

            var quiz = BsonDocument.Parse(input);
            try
            {
                await col.InsertOneAsync(quiz);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Finds one quiz from the specified (or default) collection, based on the filter "Owner" and "Name". Serializes the response to a JSON string.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="quizName"></param>
        /// <returns></returns>
        public async static Task<string> GetOneQuizAsync(string owner, string quizName, string collection = quizCollection)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(collection);

            BsonDocument quiz;

            try
            {
                quiz = await col.Find($"{{ Owner: '{owner}', Name: '{quizName}' }}").FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return quiz.ToJson();
        }
        /// <summary>
        /// Finds one quiz from the quiz collection, based on the filter "_id". Serializes the response to a JSON string.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public async static Task<string> GetOneQuizAsync(string _id, string collection)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(collection);

            BsonDocument quiz;

            try
            {
                quiz = await col.Find($"{{_id: '{_id}' }}").FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return quiz.ToJson();
        }

        /// <summary>
        /// Returns all quizzes based on the filter "Owner". Serializes the response to a JSON string.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static async Task<string> GetAllQuizzesAsync(string owner)
        {
            var agony = mongoClient.GetDatabase(databaseName);

            var quizzes = agony.GetCollection<BsonDocument>(quizCollection);

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

        /// <summary>
        /// Finds one quiz from the quiz collection, based on filter quiz "Owner", quiz "_id", and replaces atomically it 
        /// with the deserialized BSONDocument result of "quizJson".
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="_id"></param>
        /// <param name="quizJson"></param>
        /// <returns></returns>
        public static async Task ReplaceOneQuizAsync(string owner, string _id, string quizJson, string collection)
        {
            var agony = mongoClient.GetDatabase(databaseName);

            var col = agony.GetCollection<BsonDocument>(collection);

            var quiz = BsonDocument.Parse(quizJson);
            try
            {
                await col.FindOneAndReplaceAsync($"{{ Owner: '{owner}', _id: '{_id}'}}", quiz);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Checks and returns true if any document in the default quiz collection exists that matches the filter quiz "Owner" and quiz "Name".
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="quizName"></param>
        /// <returns></returns>
        public static async Task<bool> CheckIfDocumentExistsAsync(string owner, string quizName, string collection = quizCollection)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var quizzes = agony.GetCollection<BsonDocument>(collection);
            long count;
            bool exists = false;
            try
            {
                count = await quizzes.Find($"{{ Owner: '{owner}', Name: '{quizName}'}}").CountAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            if (count > 0)
                exists = true;

            return exists;
        }
        

        /// <summary>
        /// Checks and returns true if any document in the specified collection exists that matches the filter "_id".
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static async Task<bool> CheckIfDocumentExistsAsync(string _id, string collection)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(collection);
            long count;
            bool exists = false;
            try
            {
                count = await col.Find($"{{_id: '{_id}'}}").CountAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            if (count > 0)
                exists = true;

            return exists;
        }
        /// <summary>
        /// Checks and returns true if any document in the specified collection exists that matches the filter "pin".
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static async Task<bool> CheckIfPinExistsAsync(string pin, string collection)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(collection);
            long count;
            bool exists = false;
            try
            {
                count = await col.Find($"{{ Pin: '{pin}'}}").CountAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            if (count > 0)
                exists = true;

            return exists;
        }

        /// <summary>
        /// Finds one quiz matching the "_id" filter in the specified collection and sets the pin to null.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static async Task RemovePinFromQuiz(string pin, string collection)
        {
            var agony = mongoClient.GetDatabase(databaseName);

            var col = agony.GetCollection<BsonDocument>(collection);

            var filter = Builders<BsonDocument>.Filter.Eq("Pin", pin);
            var update = Builders<BsonDocument>.Update.Set("Pin", "running");


            try
            {
                await col.FindOneAndUpdateAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Finds and returns a quiz as JSON string in the specified collection by "Pin" filter.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static async Task<string> GetOneQuizByPinAsync(string pin, string collection)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(collection);

            BsonDocument quiz;

            try
            {
                quiz = await col.Find($"{{Pin: '{pin}' }}").FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return quiz.ToJson();
        }

        public static async Task SaveQuizToSession(string input, string owner)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(sessionCollection);
            var sessionQuiz = BsonDocument.Parse(input);

            if (await col.Find($"{{Owner: '{owner}'}}").CountAsync() > 0)
                await col.FindOneAndDeleteAsync($"{{ Owner: '{owner}'}}");
            
                await col.InsertOneAsync(sessionQuiz);
        }

        public static async Task<string>GetQuizFromSession(string owner)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(sessionCollection);

            BsonDocument quiz;
            
            quiz = await col.Find($"{{Owner: '{owner}' }}").FirstOrDefaultAsync();

            return quiz.ToJson();
        }
    }
}