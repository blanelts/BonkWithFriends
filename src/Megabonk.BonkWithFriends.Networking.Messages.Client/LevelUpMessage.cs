using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Client;

[NetworkMessage(MessageType.LevelUp, MessageSendFlags.ReliableNoNagle)]
internal sealed class LevelUpMessage : MessageBase
{
	internal int NewLevel { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(NewLevel);
	}

	public override void Deserialize(NetworkReader reader)
	{
		NewLevel = reader.ReadInt32();
	}
}
