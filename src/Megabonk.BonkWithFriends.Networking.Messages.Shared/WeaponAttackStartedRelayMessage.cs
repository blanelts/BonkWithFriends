using Megabonk.BonkWithFriends.IO;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.WeaponAttackStarted, MessageSendFlags.ReliableNoNagle)]
internal sealed class WeaponAttackStartedRelayMessage : MessageBase
{
	internal ulong SteamUserId { get; set; }

	internal int WeaponType { get; set; }

	internal int ProjectileCount { get; set; }

	internal float BurstInterval { get; set; }

	internal float ProjectileSize { get; set; }

	internal Vector3 SpawnPosition { get; set; }

	internal Quaternion SpawnRotation { get; set; }

	internal uint AttackId { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(SteamUserId);
		writer.Write(WeaponType);
		writer.Write(ProjectileCount);
		writer.Write(BurstInterval);
		writer.Write(ProjectileSize);
		writer.WriteVector3(SpawnPosition);
		writer.WriteQuaternion(SpawnRotation);
		writer.Write(AttackId);
	}

	public override void Deserialize(NetworkReader reader)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		SteamUserId = reader.ReadUInt64();
		WeaponType = reader.ReadInt32();
		ProjectileCount = reader.ReadInt32();
		BurstInterval = reader.ReadSingle();
		ProjectileSize = reader.ReadSingle();
		SpawnPosition = reader.ReadVector3();
		SpawnRotation = reader.ReadQuaternion();
		AttackId = reader.ReadUInt32();
	}
}
