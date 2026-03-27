using Megabonk.BonkWithFriends.IO;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.PlayerMovementRelay, MessageSendFlags.NoNagle)]
internal sealed class PlayerMovementRelayMessage : MessageBase
{
	internal ulong SteamUserId { get; set; }

	internal Vector3 Position { get; set; }

	internal Quaternion Rotation { get; set; }

	internal Vector3 Velocity { get; set; }

	internal float ServerTime { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(SteamUserId);
		writer.WriteVector3(Position);
		writer.WriteQuaternion(Rotation);
		writer.WriteVector3(Velocity);
		writer.Write(ServerTime);
	}

	public override void Deserialize(NetworkReader reader)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		SteamUserId = reader.ReadUInt64();
		Position = reader.ReadVector3();
		Rotation = reader.ReadQuaternion();
		Velocity = reader.ReadVector3();
		ServerTime = reader.ReadSingle();
	}
}
