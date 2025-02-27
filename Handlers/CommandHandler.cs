using RevoltSharp;
using RevoltSharp.Commands;

namespace TrollTrackr;

public class CommandHandler
{
	public readonly RevoltClient Client;
	public readonly CommandService Service = new();
	
	public CommandHandler(RevoltClient client)
	{
		Client = client;
		client.OnMessageRecieved += Client_OnMessageReceived;
	}
	
	private void Client_OnMessageReceived(Message? msg)
	{
		if (msg is not UserMessage Message || Message.Author!.IsBot)
			return;
		
		var commandContext = new CommandContext(Client, Message);
		if (!Utils.IsOwnerOrHasPermission(commandContext, ServerPermission.BanMembers))
		{
			return;
		}
		
		var argPos = 0;
		var prefix = Environment.GetEnvironmentVariable("PREFIX") ?? "../";
		if (!(Message.HasStringPrefix(prefix, ref argPos) || Message.HasMentionPrefix(Client.CurrentUser!, ref argPos)))
			return;
		
		var context = commandContext;
		Service.ExecuteAsync(context, argPos, null!);
	}
}