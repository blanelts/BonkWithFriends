using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.Acknowledge, MessageSendFlags.ReliableNoNagle)]
internal sealed class AcknowledgeMessage : MessageBase
{
	internal AcknowledgeMessage()
	{
	}

	public override void Serialize(NetworkWriter networkWriter)
	{
	}

	public override void Deserialize(NetworkReader networkReader)
	{
	}
}
