using System.Collections.Generic;
using Megabonk.BonkWithFriends.IO;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.BossSpawnSync, MessageSendFlags.ReliableNoNagle)]
internal sealed class BossSpawnSyncMessage : MessageBase
{
	internal sealed class BossInfo
	{
		internal uint BossPartId { get; set; }

		internal Vector3 Position { get; set; }

		internal float MaxHp { get; set; }
	}

	internal List<BossInfo> Spawns { get; set; } = new List<BossInfo>();

	public override void Serialize(NetworkWriter writer)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(Spawns.Count);
		foreach (BossInfo spawn in Spawns)
		{
			writer.Write(spawn.BossPartId);
			writer.WriteVector3(spawn.Position);
			writer.Write(spawn.MaxHp);
		}
	}

	public override void Deserialize(NetworkReader reader)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Spawns.Clear();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			Spawns.Add(new BossInfo
			{
				BossPartId = reader.ReadUInt32(),
				Position = reader.ReadVector3(),
				MaxHp = reader.ReadSingle()
			});
		}
	}
}
