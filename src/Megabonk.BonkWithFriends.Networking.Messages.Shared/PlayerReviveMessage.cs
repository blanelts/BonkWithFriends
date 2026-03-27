using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.PlayerRevive, MessageSendFlags.ReliableNoNagle)]
internal sealed class PlayerReviveMessage : MessageBase
{
	internal ulong TargetSteamId { get; set; }
	internal ulong ReviverSteamId { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(TargetSteamId);
		writer.Write(ReviverSteamId);
	}

	public override void Deserialize(NetworkReader reader)
	{
		TargetSteamId = reader.ReadUInt64();
		ReviverSteamId = reader.ReadUInt64();
	}
}
