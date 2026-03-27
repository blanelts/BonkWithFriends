using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.LoadStage, MessageSendFlags.Reliable)]
internal sealed class LoadStageMessage : MessageBase
{
	public int StageIndex { get; set; }
	public int Seed { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(StageIndex);
		writer.Write(Seed);
	}

	public override void Deserialize(NetworkReader reader)
	{
		StageIndex = reader.ReadInt32();
		Seed = reader.ReadInt32();
	}
}
