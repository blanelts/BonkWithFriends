using Megabonk.BonkWithFriends.IO;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.EnemySpawned, MessageSendFlags.ReliableNoNagle)]
internal sealed class EnemySpawnedMessage : MessageBase
{
	internal uint EnemyId { get; set; }

	internal int EnemyType { get; set; }

	internal Vector3 Position { get; set; }

	internal Vector3 EulerAngles { get; set; }

	internal Vector2 VelXZ { get; set; }

	internal float MaxHp { get; set; }

	internal int Flags { get; set; }

	internal float extraSizeMultiplier { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(EnemyId);
		writer.Write(EnemyType);
		writer.WriteVector3(Position);
		writer.WriteVector3(EulerAngles);
		writer.WriteVector2(VelXZ);
		writer.Write(MaxHp);
		writer.Write(Flags);
		writer.Write(extraSizeMultiplier);
	}

	public override void Deserialize(NetworkReader reader)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		EnemyId = reader.ReadUInt32();
		EnemyType = reader.ReadInt32();
		Position = reader.ReadVector3();
		EulerAngles = reader.ReadVector3();
		VelXZ = reader.ReadVector2();
		MaxHp = reader.ReadSingle();
		Flags = reader.ReadInt32();
		extraSizeMultiplier = reader.ReadSingle();
	}
}
