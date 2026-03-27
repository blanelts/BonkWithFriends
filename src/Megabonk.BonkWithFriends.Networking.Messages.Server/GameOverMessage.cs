using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.GameOver, MessageSendFlags.ReliableNoNagle)]
internal sealed class GameOverMessage : MessageBase
{
	public override void Serialize(NetworkWriter writer)
	{
	}

	public override void Deserialize(NetworkReader reader)
	{
	}
}
