using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.PickupDespawned, MessageSendFlags.ReliableNoNagle)]
internal sealed class PickupDespawnedMessage : MessageBase
{
	internal int PickupId { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(PickupId);
	}

	public override void Deserialize(NetworkReader reader)
	{
		PickupId = reader.ReadInt32();
	}
}
