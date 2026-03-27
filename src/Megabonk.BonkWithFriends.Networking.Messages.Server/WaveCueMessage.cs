using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.WaveCue, MessageSendFlags.ReliableNoNagle)]
internal sealed class WaveCueMessage : MessageBase
{
	internal int WaveType { get; set; }

	internal float Duration { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(WaveType);
		writer.Write(Duration);
	}

	public override void Deserialize(NetworkReader reader)
	{
		WaveType = reader.ReadInt32();
		Duration = reader.ReadSingle();
	}
}
