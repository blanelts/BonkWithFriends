using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Client;

[NetworkMessage(MessageType.RequestLoadStage, MessageSendFlags.ReliableNoNagle)]
internal sealed class RequestLoadStageMessage : MessageBase
{
	public override void Serialize(NetworkWriter writer) { }

	public override void Deserialize(NetworkReader reader) { }
}
