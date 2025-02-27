using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TrollTrackr
{
	public enum Emojis
	{
		[EnumMember(Value = "01JMY1HA3NT93TTZMEY78121YW")]
		Trolled,
		
		[EnumMember(Value = "01JN39QY7PZ9BH3EYH5ED4GB8Y")]
		Processing,
		[EnumMember(Value = "01JN22HKMBTWS51NSPDT3PVV1K")]
		Success,
		[EnumMember(Value = "01JN22HH3R6XPGKS4TABG8X087")]
		Error
	}
}