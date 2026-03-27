using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.TimeSyncRequest, MessageSendFlags.ReliableNoNagle)]
internal sealed class TimeSyncRequestMessage : MessageBase
{
	internal float ClientSendTime { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(ClientSendTime);
	}

	public override void Deserialize(NetworkReader reader)
	{
		ClientSendTime = reader.ReadSingle();
	}
}
