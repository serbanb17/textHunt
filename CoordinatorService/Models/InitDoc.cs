using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CoordinatorService.Models
{
    public class InitDoc
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("text")]
        public string Text { get; set; }

        [BsonElement("item")]
        public string Item { get; set; }
    }
}