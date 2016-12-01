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
        const string answersCollection = "answers";


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

            try
            {
                var quiz = BsonDocument.Parse(input);
                await col.InsertOneAsync(quiz);
            }
            catch (Exception ex)
            {
                throw new MongoException("MongoManager:SaveDocumentAsync. Insert failed.", ex.InnerException);
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

            try
            {
                var quiz = BsonDocument.Parse(input);
                await col.InsertOneAsync(quiz);
            }
            catch (Exception ex)
            {
                throw new MongoException("MongoManager:SaveDocumentAsync. Insert failed.", ex.InnerException);
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
                return quiz.ToJson();
            }
            catch (Exception ex)
            {
                throw new MongoException("MongoManager:GetOneAsync. Single document retrieval failed.", ex.InnerException);
            }

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
                return quiz.ToJson();
            }
            catch (Exception ex)
            {
                throw new MongoException("MongoManager:GetOneAsync. Single document retrieval failed.", ex.InnerException);
            }

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
                return listOfQuizzes.ToArray().ToJson();
            }
            catch (Exception ex)
            {
                throw new MongoException("MongoManager:GetOneAsync. Multiple document retrieval failed.", ex.InnerException);
            }

        }

        /// <summary>
        /// Finds one quiz from the quiz collection, based on filter quiz "Owner", quiz "_id", and replaces atomically it 
        /// with the deserialized BSONDocument result of "quizJson".
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        /// <param name="quizJson"></param>
        /// <returns></returns>
        public static async Task ReplaceOneQuizAsync(string owner, string name, string quizJson, string collection)
        {
            var agony = mongoClient.GetDatabase(databaseName);

            var col = agony.GetCollection<BsonDocument>(collection);

            try
            {
                var quiz = BsonDocument.Parse(quizJson);
                if (await col.Find($"{{ Owner: '{owner}', Name: '{name}'}}").CountAsync() > 0)
                    await col.DeleteOneAsync($"{{ Owner: '{owner}', Name: '{name}'}}");

                await Task.Delay(1000);

                await col.InsertOneAsync(quiz);
            }
            catch (Exception ex)
            {
                throw new MongoException("MongoManager:ReplaceOneQuizAsync. Document replacement failed.", ex.InnerException);
            }
        }

        /// <summary>
        /// Finds one quiz from the quiz collection, based on filter "_id", and replaces atomically it 
        /// with the deserialized BSONDocument result of "quizJson".
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="quizJson"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static async Task ReplaceOneQuizAsync(string _id, string quizJson, string collection)
        {
            var agony = mongoClient.GetDatabase(databaseName);

            var col = agony.GetCollection<BsonDocument>(collection);

            try
            {
                var quiz = BsonDocument.Parse(quizJson);
                if (await col.Find($"{{ _id: '{_id}'}}").CountAsync() > 0)
                    await col.DeleteOneAsync($"{{ _id: '{_id}'}}");


                await col.InsertOneAsync(quiz);
            }
            catch (Exception ex)
            {
                throw new MongoException("MongoManager:ReplaceOneQuizAsync. Document replacement failed.", ex.InnerException);
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
                throw new MongoException("MongoManager:CheckIfDocumentExistsAsync. Check for document failed.", ex.InnerException);
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
                throw new MongoException("MongoManager:CheckIfDocumentExistsAsync. Check for document failed.", ex.InnerException);
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
                throw new MongoException("MongoManager:CheckIfPinExistsAsync. Check for document failed.", ex.InnerException);
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
                throw new MongoException("MongoManager:RemovePinFromQuiz. Pin removal failed.", ex.InnerException);
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


            try
            {
                var quiz = await col.Find($"{{Pin: '{pin}' }}").FirstOrDefaultAsync();
                return quiz.ToJson();
            }
            catch (Exception ex)
            {
                throw new MongoException("MongoManager:GetOneQuizByPinAsync. Single document retrieval failed.", ex.InnerException);
            }

        }

        public static async Task SaveQuizToSession(string input, string owner)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(sessionCollection);

            try
            {
                var sessionQuiz = BsonDocument.Parse(input);

                if (await col.Find($"{{Owner: '{owner}'}}").CountAsync() > 0)
                    await col.DeleteOneAsync($"{{ Owner: '{owner}'}}");

                await Task.Delay(1000);
                await col.InsertOneAsync(sessionQuiz);
            }
            catch (Exception ex)
            {
                throw new MongoException("MongoManager:SaveQuizToSession. Saving to session storage failed.", ex.InnerException);
            }
        }

        public static async Task<string> GetQuizFromSession(string owner)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(sessionCollection);
            try
            {
                var quiz = await col.Find($"{{Owner: '{owner}' }}").FirstOrDefaultAsync();
                return quiz.ToJson();
            }
            catch (Exception ex)
            {
                throw new MongoException("MongoManager:GetQuizFromSession. Retrieving from session storage failed.", ex.InnerException);
            }
        }
        public static async Task<string> GetAllAnswerFormsAsync(string runningQuizId, string collection)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(collection);

            List<BsonDocument> ListOfAnswerForms = new List<BsonDocument>();
            try
            {
                ListOfAnswerForms = await col.Find($"{{ RunningQuizId: '{runningQuizId}'}}").ToListAsync();
                return ListOfAnswerForms.ToArray().ToJson();
            }
            catch (Exception ex)
            {
                throw new MongoException("MongoManager:GetAllAnswerFormsAsync. Retrieving answers failed.", ex.InnerException);
            }

        }

        /// <summary>
        /// Deletes the document that matches the specified filter "_id" from the specified collection.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static async Task DeleteDocument(string _id, string collection)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(collection);

            try
            {
                await col.FindOneAndDeleteAsync($"{{_id: '{_id}' }}");
            }
            catch (Exception ex)
            {
                throw new MongoException("MongoManager:DeleteDocument. Document removal failed.", ex.InnerException);
            }
        }

        /// <summary>
        /// Deletes all documents that matches the specified filter "runningQuizzId" from the "answers" collection.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static async Task ClearOldAnswers(string runningQuizId)
        {
            var agony = mongoClient.GetDatabase(databaseName);
            var col = agony.GetCollection<BsonDocument>(answersCollection);

            try
            {
                await col.DeleteManyAsync($"{{RunningQuizId: '{runningQuizId}' }}");
            }
            catch (Exception ex)
            {
                throw new MongoException("MongoManager:ClearOldAnswers. Document removal failed.", ex.InnerException);
            }
        }
    }
}