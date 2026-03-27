using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.BossDied, MessageSendFlags.ReliableNoNagle)]
internal sealed class BossDiedMessage : MessageBase
{
	internal bool IsLastStage { get; set; }

	internal float HostTime { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(IsLastStage);
		writer.Write(HostTime);
	}

	public override void Deserialize(NetworkReader reader)
	{
		IsLastStage = reader.ReadBoolean();
		HostTime = reader.ReadSingle();
	}
}
