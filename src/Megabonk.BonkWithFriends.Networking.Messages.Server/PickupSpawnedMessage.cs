using Megabonk.BonkWithFriends.IO;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.PickupSpawned, MessageSendFlags.ReliableNoNagle)]
internal sealed class PickupSpawnedMessage : MessageBase
{
	internal int PickupId { get; set; }

	internal int EPickup { get; set; }

	internal Vector3 Position { get; set; }

	internal int Value { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(PickupId);
		writer.Write(EPickup);
		writer.WriteVector3(Position);
		writer.Write(Value);
	}

	public override void Deserialize(NetworkReader reader)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		PickupId = reader.ReadInt32();
		EPickup = reader.ReadInt32();
		Position = reader.ReadVector3();
		Value = reader.ReadInt32();
	}
}
