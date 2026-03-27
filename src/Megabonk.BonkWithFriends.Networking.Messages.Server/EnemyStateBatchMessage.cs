using System.Collections.Generic;
using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.EnemyStateBatch, MessageSendFlags.Unreliable)]
internal sealed class EnemyStateBatchMessage : MessageBase
{
	internal sealed class EnemyState
	{
		internal uint EnemyId { get; set; }

		internal short PosX { get; set; }

		internal short PosY { get; set; }

		internal short PosZ { get; set; }

		internal byte YawQuantized { get; set; }

		internal sbyte VelX { get; set; }

		internal sbyte VelZ { get; set; }

		internal sbyte AngVelQuantized { get; set; }

		internal ushort Hp { get; set; }

		internal ushort MaxHp { get; set; }

		internal int Flags { get; set; }

		internal float ServerTime { get; set; }

		internal uint Seq { get; set; }
	}

	internal List<EnemyState> States { get; set; } = new List<EnemyState>();

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(States.Count);
		foreach (EnemyState state in States)
		{
			writer.Write(state.EnemyId);
			writer.Write(state.PosX);
			writer.Write(state.PosY);
			writer.Write(state.PosZ);
			writer.Write(state.YawQuantized);
			writer.Write(state.VelX);
			writer.Write(state.VelZ);
			writer.Write(state.AngVelQuantized);
			writer.Write(state.Hp);
			writer.Write(state.MaxHp);
			writer.Write(state.Flags);
			writer.Write(state.ServerTime);
			writer.Write(state.Seq);
		}
	}

	public override void Deserialize(NetworkReader reader)
	{
		States.Clear();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			States.Add(new EnemyState
			{
				EnemyId = reader.ReadUInt32(),
				PosX = reader.ReadInt16(),
				PosY = reader.ReadInt16(),
				PosZ = reader.ReadInt16(),
				YawQuantized = reader.ReadByte(),
				VelX = reader.ReadSByte(),
				VelZ = reader.ReadSByte(),
				AngVelQuantized = reader.ReadSByte(),
				Hp = reader.ReadUInt16(),
				MaxHp = reader.ReadUInt16(),
				Flags = reader.ReadInt32(),
				ServerTime = reader.ReadSingle(),
				Seq = reader.ReadUInt32()
			});
		}
	}
}
