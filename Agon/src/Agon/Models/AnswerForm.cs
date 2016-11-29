using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public class AnswerForm
    {
        [BsonId]
        public string _id { get; set; }
        [Required]
        public string SubmitterName { get; set; }
        public List<string> Answers { get; set; }

        public string RunningQuizId { get; set; }

        public string Timestamp { get; set; }


    }
}
