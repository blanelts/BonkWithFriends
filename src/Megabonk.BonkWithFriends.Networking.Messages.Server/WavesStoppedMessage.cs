using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.WavesStopped, MessageSendFlags.ReliableNoNagle)]
internal sealed class WavesStoppedMessage : MessageBase
{
	public override void Serialize(NetworkWriter writer)
	{
	}

	public override void Deserialize(NetworkReader reader)
	{
	}
}
