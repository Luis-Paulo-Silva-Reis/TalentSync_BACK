using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;


namespace minimalwebapi.models
{
    public class JobModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }

        public string Profissao { get; set; }
        public string Tipo { get; set; }
        public string Level { get; set; }
        public string LocalDeTrabalho { get; set; }
        public string PCD { get; set; }
    }
}
