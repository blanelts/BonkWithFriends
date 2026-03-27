using System.Collections.Generic;
using Megabonk.BonkWithFriends.HarmonyPatches.Items;
using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.InteractableSpawnBatch, MessageSendFlags.ReliableNoNagle)]
internal sealed class InteractableSpawnBatchMessage : MessageBase
{
	internal List<InteractableSpawnData> Spawns { get; set; } = new List<InteractableSpawnData>();

	public override void Serialize(NetworkWriter writer)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(Spawns.Count);
		foreach (InteractableSpawnData spawn in Spawns)
		{
			writer.Write(spawn.Id);
			writer.Write((byte)spawn.Type);
			writer.WriteVector3(spawn.Position);
			writer.WriteQuaternion(spawn.Rotation);
			writer.Write(spawn.SubType);
		}
	}

	public override void Deserialize(NetworkReader reader)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Spawns.Clear();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			Spawns.Add(new InteractableSpawnData
			{
				Id = reader.ReadInt32(),
				Type = (InteractableType)reader.ReadByte(),
				Position = reader.ReadVector3(),
				Rotation = reader.ReadQuaternion(),
				SubType = reader.ReadInt32()
			});
		}
	}
}
