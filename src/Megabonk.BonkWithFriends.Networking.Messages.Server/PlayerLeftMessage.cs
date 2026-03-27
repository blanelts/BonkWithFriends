using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.PlayerLeft, MessageSendFlags.ReliableNoNagle)]
internal sealed class PlayerLeftMessage : MessageBase
{
	internal ulong SteamUserId { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(SteamUserId);
	}

	public override void Deserialize(NetworkReader reader)
	{
		SteamUserId = reader.ReadUInt64();
	}
}
