using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.SeedSync, MessageSendFlags.ReliableNoNagle)]
internal sealed class SeedSyncMessage : MessageBase
{
	internal int Seed { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(Seed);
	}

	public override void Deserialize(NetworkReader reader)
	{
		Seed = reader.ReadInt32();
	}
}
