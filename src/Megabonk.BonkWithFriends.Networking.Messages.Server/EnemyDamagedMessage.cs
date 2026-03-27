using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.EnemyDamaged, MessageSendFlags.ReliableNoNagle)]
internal sealed class EnemyDamagedMessage : MessageBase
{
	internal uint EnemyId { get; set; }

	internal float HpNow { get; set; }

	internal float DamageForFx { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(EnemyId);
		writer.Write(HpNow);
		writer.Write(DamageForFx);
	}

	public override void Deserialize(NetworkReader reader)
	{
		EnemyId = reader.ReadUInt32();
		HpNow = reader.ReadSingle();
		DamageForFx = reader.ReadSingle();
	}
}
