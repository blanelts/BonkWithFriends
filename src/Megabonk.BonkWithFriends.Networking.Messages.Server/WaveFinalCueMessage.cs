using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.WaveFinalCue, MessageSendFlags.ReliableNoNagle)]
internal sealed class WaveFinalCueMessage : MessageBase
{
	public override void Serialize(NetworkWriter writer)
	{
	}

	public override void Deserialize(NetworkReader reader)
	{
	}
}
