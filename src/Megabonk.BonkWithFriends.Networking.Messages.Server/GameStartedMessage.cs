using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.GameStarted, MessageSendFlags.ReliableNoNagle)]
internal sealed class GameStartedMessage : MessageBase
{
	public override void Serialize(NetworkWriter writer)
	{
	}

	public override void Deserialize(NetworkReader reader)
	{
	}
}
