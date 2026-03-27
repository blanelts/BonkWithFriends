using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.PlayerJoined, MessageSendFlags.ReliableNoNagle)]
internal sealed class PlayerJoinedMessage : MessageBase
{
	internal int Character { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(Character);
	}

	public override void Deserialize(NetworkReader reader)
	{
		Character = reader.ReadInt32();
	}
}
