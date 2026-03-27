using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Client;

[NetworkMessage(MessageType.PlayerHealed, MessageSendFlags.ReliableNoNagle)]
internal sealed class PlayerHealedMessage : MessageBase
{
	internal int HealAmount { get; set; }

	internal int Hp { get; set; }

	internal int MaxHp { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(HealAmount);
		writer.Write(Hp);
		writer.Write(MaxHp);
	}

	public override void Deserialize(NetworkReader reader)
	{
		HealAmount = reader.ReadInt32();
		Hp = reader.ReadInt32();
		MaxHp = reader.ReadInt32();
	}
}
