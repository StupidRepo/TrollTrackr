using System.Text;
using Humanizer;
using MongoDB.Driver;
using RevoltSharp;
using RevoltSharp.Commands;
using TrollTrackr.Models;

namespace TrollTrackr.Commands;

public class TrollCommands : ModuleBase
{
	/*
	 * Lists all trolls in the DB.
	 */
	[Command("listTrolls")]
	public async Task ListTrollsAsync()
	{
		var message = await ReplyAsync(Utils.MakeEmojiSentence(Emojis.Processing, "Listing 3 most recent trolls from DB...").Value);
		
		// Get all trolls from DB
		var recentTrolls = await Program.DatabaseHandler?.TrollCollection?.Find(_ => true)
			.SortByDescending(troll => troll.Date)
			.Limit(3)
			.ToListAsync()!;

		if (recentTrolls.Count == 0)
		{
			await message.EditMessageAsync(Utils.MakeEmojiSentence(Emojis.Error, "No trolls found in DB.")!);
			return;
		}
		
		var sb = new StringBuilder();
		
		sb.Append("Last 3 most recent trolls:\n");
		for (var i = 0; i < recentTrolls.Count; i++)
		{
			var troll = recentTrolls[i];
			var isLast = i == recentTrolls.Count - 1;
			
			sb.Append((await Utils.FormatTroll(troll, isLast)).Value);
		}
		
		await message.EditMessageAsync(new Option<string?>(sb.ToString()));
	}
	
	/*
	 * Gets a troll from the DB.
	 */
	[Command("getTroll")]
	public async Task GetTrollAsync(string trollId)
	{
		var message = await ReplyAsync(Utils.MakeEmojiSentence(Emojis.Processing, "Getting troll from DB...").Value);
		
		// Get troll from DB
		var troll = await Program.DatabaseHandler?.TrollCollection?.Find(troll => troll.TrollId == trollId).FirstOrDefaultAsync();
		if (troll == null)
		{
			await message.EditMessageAsync(Utils.MakeEmojiSentence(Emojis.Error, "Troll not found in DB.")!);
			return;
		}

		await message.EditMessageAsync(
			Utils.MakeEmojiSentence(
				Emojis.Success,
				(await Utils.FormatTroll(troll, true)).Value
			)!
		);
	}
	
	/*
	 * Adds a troll to the DB.
	 */
	[Command("addTroll", true)]
	public async Task AddTrollAsync()
	{
		var message = await ReplyAsync(Utils.MakeEmojiSentence(Emojis.Processing, "Adding troll to DB...").Value);

		var mentions = Context.Message.Mentions;
		if(!mentions.Any())
		{
			await message.EditMessageAsync(Utils.MakeEmojiSentence(Emojis.Error, "No user mentioned.")!);
			return;
		}
		
		// Get user by mention
		var user = await Context?.Client.Rest.GetUserAsync(mentions[0]);
		if (user == null)
		{
			await message.EditMessageAsync(Utils.MakeEmojiSentence(Emojis.Error, "User not found.")!);
			return;
		}
		
		// Check if user already exists by ID
		var trollExists = await Program.DatabaseHandler?.TrollCollection?.Find(troll => troll.UserId == user.Id).AnyAsync();
		if (trollExists)
		{
			await message.EditMessageAsync(Utils.MakeEmojiSentence(Emojis.Error, $"{user.CurrentName} already exists in DB.")!);
			return;
		}
		
		// Add troll to DB
		try
		{
			var troll = new Troll
			{
				TrollId = Utils.GenerateId(user),
				UserId = user.Id,
				Name = user.CurrentName,

				Date = DateTime.Now,
				Reason = "[no reason provided yet]",
				
				AddedByID = Context.User.Id
			};
			await Program.DatabaseHandler?.TrollCollection?.InsertOneAsync(troll)!;
			await message.EditMessageAsync(Utils.MakeEmojiSentence(Emojis.Trolled, $"Added {user.CurrentName} to DB.\nTroll ID: {troll.TrollId}\nRun `{Context.Prefix}reason {troll.TrollId} [reason]` to attach a reason.")!);
		}
		catch (MongoWriteException)
		{
			await message.EditMessageAsync(Utils.MakeEmojiSentence(Emojis.Error, $"{user.CurrentName} already exists in DB.")!);
		}
	}
	
	/*
	 * Removes a troll from the DB.
	 */
	[Command("removeTroll")]
	public async Task RemoveTrollAsync(string trollId)
	{
		var message = await ReplyAsync(Utils.MakeEmojiSentence(Emojis.Processing, "Removing troll from DB...").Value);
		
		// Remove troll from DB
		var result = await Program.DatabaseHandler?.TrollCollection?.DeleteOneAsync(troll => troll.TrollId == trollId);
		if (result?.DeletedCount == 0)
		{
			await message.EditMessageAsync(Utils.MakeEmojiSentence(Emojis.Error, $"{trollId} not found in DB.")!);
			return;
		}
		
		await message.EditMessageAsync(Utils.MakeEmojiSentence(Emojis.Success, "Removed troll from DB.")!);
	}

	[Command("reason")]
	public async Task UpdateTrollReason(string trollId, [Remainder] string reason)
	{
		var message = await ReplyAsync(Utils.MakeEmojiSentence(Emojis.Processing, "Updating troll reason in DB...").Value);
		
		// Get troll by ID
		var troll = await Program.DatabaseHandler?.TrollCollection?.Find(troll => troll.TrollId == trollId).FirstOrDefaultAsync();
		if (troll == null)
		{
			await message.EditMessageAsync(Utils.MakeEmojiSentence(Emojis.Error, "Troll not found in DB.")!);
			return;
		}
		
		// Update troll reason in DB
		var update = Builders<Troll>.Update.Set(t => t.Reason, reason);
		await Program.DatabaseHandler?.TrollCollection?.UpdateOneAsync(t => t.TrollId == trollId, update);
		
		await message.EditMessageAsync(Utils.MakeEmojiSentence(Emojis.Success, "Updated troll reason in DB.")!);
	}
	
	/*
	 * Clears all trolls from the DB.
	 */
	[Command("clearTrolls")]
	public async Task ClearTrollsAsync()
	{
		if (!Utils.IsOwner(Context!)) return;
		
		var message = await ReplyAsync(Utils.MakeEmojiSentence(Emojis.Processing, "Clearing all trolls from DB...").Value);
		
		// Clear all trolls from DB
		var result = await Program.DatabaseHandler?.TrollCollection?.DeleteManyAsync(_ => true);
		if (result?.DeletedCount == 0)
		{
			await message.EditMessageAsync(Utils.MakeEmojiSentence(Emojis.Error, "No trolls found in DB.")!);
			return;
		}
		
		await message.EditMessageAsync(Utils.MakeEmojiSentence(Emojis.Success, $"Cleared {result.DeletedCount} troll(s) from DB.")!);
	}
}