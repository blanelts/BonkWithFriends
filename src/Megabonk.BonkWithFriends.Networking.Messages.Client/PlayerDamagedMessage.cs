using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Client;

[NetworkMessage(MessageType.PlayerDamaged, MessageSendFlags.ReliableNoNagle)]
internal sealed class PlayerDamagedMessage : MessageBase
{
	internal float Damage { get; set; }

	internal int Hp { get; set; }

	internal int MaxHp { get; set; }

	internal float Shield { get; set; }

	internal float MaxShield { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(Damage);
		writer.Write(Hp);
		writer.Write(MaxHp);
		writer.Write(Shield);
		writer.Write(MaxShield);
	}

	public override void Deserialize(NetworkReader reader)
	{
		Damage = reader.ReadSingle();
		Hp = reader.ReadInt32();
		MaxHp = reader.ReadInt32();
		Shield = reader.ReadSingle();
		MaxShield = reader.ReadSingle();
	}
}
