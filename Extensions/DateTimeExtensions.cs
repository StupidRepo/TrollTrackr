using System.Runtime.Serialization;

namespace TrollTrackr.Extensions;

public static class DateTimeExtension
{
	public static string ToUnixTimestamp(this DateTime @dateTime)
	{
		return ((DateTimeOffset) @dateTime).ToUnixTimeSeconds().ToString();
	}
}