using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Agon.Models
{

    public class Quiz
    {
        [BsonId]
        public string _id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }

        public List<Song> Songs { get; set; }
        public Quiz()
        {
            Songs = new List<Song>();
        }
    }
}
