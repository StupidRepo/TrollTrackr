using System.Reflection;
using dotenv.net;
using MongoDB.Driver;
using RevoltSharp;

namespace TrollTrackr;

internal class Program
{
	public static RevoltClient? Client;
	public static DatabaseHandler? DatabaseHandler;
	
	private static void Main(string[] args)
	{
		DotEnv.Load();

		DatabaseHandler = new DatabaseHandler();
		Start().GetAwaiter().GetResult();
	}
	
	public static async Task Start()
	{
		Client = new RevoltClient(ClientMode.WebSocket, new ClientConfig
		{
			// ApiUrl = "https://api.revolt.chat",
			Owners = ["01JMV4KTP06D5EPAKHTTCQ2XRH"]
		});
		
		await Client.LoginAsync(Environment.GetEnvironmentVariable("TOKEN") ?? "", AccountType.User);
		await Client.StartAsync();
		
		var Commands = new CommandHandler(Client);
		await Commands.Service.AddModulesAsync(Assembly.GetEntryAssembly()!, null!);
		
		await Task.Delay(-1);
	}
}