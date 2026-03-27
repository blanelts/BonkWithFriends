using Megabonk.BonkWithFriends.IO;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.WeaponProjectileHit, MessageSendFlags.NoNagle)]
internal sealed class WeaponProjectileHitMessage : MessageBase
{
	internal uint AttackId { get; set; }

	internal int ProjectileIndex { get; set; }

	internal Vector3 HitPosition { get; set; }

	internal Vector3 HitNormal { get; set; }

	internal uint TargetId { get; set; }

	internal float Damage { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(AttackId);
		writer.Write(ProjectileIndex);
		writer.WriteVector3(HitPosition);
		writer.WriteVector3(HitNormal);
		writer.Write(TargetId);
		writer.Write(Damage);
	}

	public override void Deserialize(NetworkReader reader)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		AttackId = reader.ReadUInt32();
		ProjectileIndex = reader.ReadInt32();
		HitPosition = reader.ReadVector3();
		HitNormal = reader.ReadVector3();
		TargetId = reader.ReadUInt32();
		Damage = reader.ReadSingle();
	}
}
