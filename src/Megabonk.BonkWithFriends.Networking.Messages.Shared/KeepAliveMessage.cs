using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.KeepAlive, MessageSendFlags.ReliableNoNagle)]
internal sealed class KeepAliveMessage : MessageBase
{
	internal KeepAliveMessage()
	{
	}

	public override void Serialize(NetworkWriter networkWriter)
	{
	}

	public override void Deserialize(NetworkReader networkReader)
	{
	}
}
