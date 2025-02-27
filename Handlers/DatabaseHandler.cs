using MongoDB.Driver;
using TrollTrackr.Models;

namespace TrollTrackr;

public class DatabaseHandler
{
	public readonly MongoClient? Client;
	public readonly IMongoDatabase? Database;
	
	public IMongoCollection<Troll>? TrollCollection => Database?.GetCollection<Troll>("Trolls");
	
	public DatabaseHandler()
	{
		Client = new MongoClient(Environment.GetEnvironmentVariable("MONGO_URI") ?? "");
		Database = Client.GetDatabase("TrollTrackr");
	}
}