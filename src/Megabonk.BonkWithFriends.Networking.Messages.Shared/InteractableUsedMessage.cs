using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.InteractableUsed, MessageSendFlags.ReliableNoNagle)]
internal sealed class InteractableUsedMessage : MessageBase
{
	internal ulong PlayerSteamId { get; set; }

	internal int InteractableId { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(PlayerSteamId);
		writer.Write(InteractableId);
	}

	public override void Deserialize(NetworkReader reader)
	{
		PlayerSteamId = reader.ReadUInt64();
		InteractableId = reader.ReadInt32();
	}
}
