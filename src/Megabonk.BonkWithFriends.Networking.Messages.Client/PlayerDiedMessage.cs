using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Client;

[NetworkMessage(MessageType.PlayerDied, MessageSendFlags.ReliableNoNagle)]
internal sealed class PlayerDiedMessage : MessageBase
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
