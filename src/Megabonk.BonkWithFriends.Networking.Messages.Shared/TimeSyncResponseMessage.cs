using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.TimeSyncResponse, MessageSendFlags.ReliableNoNagle)]
internal sealed class TimeSyncResponseMessage : MessageBase
{
	internal float ClientSendTime { get; set; }

	internal float ServerReceiveTime { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(ClientSendTime);
		writer.Write(ServerReceiveTime);
	}

	public override void Deserialize(NetworkReader reader)
	{
		ClientSendTime = reader.ReadSingle();
		ServerReceiveTime = reader.ReadSingle();
	}
}
