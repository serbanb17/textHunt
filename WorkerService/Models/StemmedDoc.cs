using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkerService.Models
{
    public class StemmedDoc
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("initDocId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string InitDocId { get; set; }

        [BsonElement("stemmedWords")]
        public List<string> StemmedWords { get; set; }
    }
}