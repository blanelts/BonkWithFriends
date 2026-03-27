using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.TimelineEvent, MessageSendFlags.ReliableNoNagle)]
internal sealed class TimelineEventMessage : MessageBase
{
	internal int EventIndex { get; set; }

	internal float HostTime { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(EventIndex);
		writer.Write(HostTime);
	}

	public override void Deserialize(NetworkReader reader)
	{
		EventIndex = reader.ReadInt32();
		HostTime = reader.ReadSingle();
	}
}
