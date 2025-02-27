using MongoDB.Bson.Serialization.Attributes;

namespace TrollTrackr.Models;

public class Troll
{
	[BsonId] public string TrollId { get; set; }
	[BsonElement("user_id")] public string UserId { get; set; }
	
	[BsonElement("name")] public string Name { get; set; }
	
	[BsonElement("date")] public DateTime Date { get; set; }
	[BsonElement("reason")] public string Reason { get; set; }
	
	[BsonElement("added_by")] public string AddedByID { get; set; }
}