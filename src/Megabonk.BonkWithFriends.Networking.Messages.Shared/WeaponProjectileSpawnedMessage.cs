using Megabonk.BonkWithFriends.IO;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.WeaponProjectileSpawned, MessageSendFlags.NoNagle)]
internal sealed class WeaponProjectileSpawnedMessage : MessageBase
{
	internal uint AttackId { get; set; }

	internal int ProjectileIndex { get; set; }

	internal Vector3 Position { get; set; }

	internal Quaternion Rotation { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(AttackId);
		writer.Write(ProjectileIndex);
		writer.WriteVector3(Position);
		writer.WriteQuaternion(Rotation);
	}

	public override void Deserialize(NetworkReader reader)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		AttackId = reader.ReadUInt32();
		ProjectileIndex = reader.ReadInt32();
		Position = reader.ReadVector3();
		Rotation = reader.ReadQuaternion();
	}
}
