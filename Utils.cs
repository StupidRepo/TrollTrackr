using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using Humanizer;
using MongoDB.Driver;
using RevoltSharp;
using RevoltSharp.Commands;
using TrollTrackr.Extensions;
using TrollTrackr.Models;

namespace TrollTrackr
{
	public static class Utils
	{
		public static string FormatEmoji(Emojis emoji)
		{
			return $":{emoji.ToEnumMemberAttrValue()}:";
		}

		public static Option<string> MakeEmojiSentence(Emojis emoji, string message)
		{
			return new Option<string>($"{FormatEmoji(emoji)} {message}");
		}

		public static async Task<Option<string>> FormatTroll(Troll troll, bool isLast = false)
		{
			var sb = new StringBuilder();
			
			User? addedByUser = null;
			var didDo = Program.Client?.TryGetUser(troll.AddedByID, out addedByUser);
			if (!didDo.HasValue || !didDo.Value)
				addedByUser = await Program.Client?.Rest.GetUserAsync(troll.AddedByID);
			
			sb.Append($"### {troll.Name} - <t:{troll.Date.ToUnixTimestamp()}:R>\n");
			sb.Append($"**User ID:** {troll.UserId} • **Troll ID**: {troll.TrollId}\n");
			sb.Append($"**Reason:** {troll.Reason}\n");
			sb.Append($"**Added by:** {addedByUser?.CurrentName ?? "Unknown"}");

			if (!isLast) sb.Append('\n');
			
			return new Option<string>(sb.ToString());
		}

		public static bool IsOwner(CommandContext context)
		{
			return context.Message.AuthorId == context.Client.CurrentUser.Id;
		}
		
		public static bool IsOwnerOrHasPermission(CommandContext context, ServerPermission permission)
		{
			return IsOwner(context) || context.Member.Permissions.Has(permission);
		}

		public static string GenerateId(User user)
		{
			var sb = new StringBuilder();
			sb.Append(Program.DatabaseHandler?.TrollCollection?.CountDocuments(FilterDefinition<Troll>.Empty)+1 ?? 0);
			sb.Append(user.Id.TakeLast(4).ToArray());
			
			return sb.ToString();
		}
	}
}