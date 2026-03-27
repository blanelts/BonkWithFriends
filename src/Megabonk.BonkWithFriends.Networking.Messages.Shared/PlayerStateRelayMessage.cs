using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.PlayerState, MessageSendFlags.ReliableNoNagle)]
internal sealed class PlayerStateRelayMessage : MessageBase
{
	internal ulong SteamUserId { get; set; }

	internal int Hp { get; set; }

	internal int MaxHp { get; set; }

	internal float Shield { get; set; }

	internal float MaxShield { get; set; }

	internal int Level { get; set; }

	internal int Xp { get; set; }

	internal bool IsDead { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(SteamUserId);
		writer.Write(Hp);
		writer.Write(MaxHp);
		writer.Write(Shield);
		writer.Write(MaxShield);
		writer.Write(Level);
		writer.Write(Xp);
		writer.Write(IsDead);
	}

	public override void Deserialize(NetworkReader reader)
	{
		SteamUserId = reader.ReadUInt64();
		Hp = reader.ReadInt32();
		MaxHp = reader.ReadInt32();
		Shield = reader.ReadSingle();
		MaxShield = reader.ReadSingle();
		Level = reader.ReadInt32();
		Xp = reader.ReadInt32();
		IsDead = reader.ReadBoolean();
	}
}
