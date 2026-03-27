using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.EnemyDied, MessageSendFlags.ReliableNoNagle)]
internal sealed class EnemyDiedMessage : MessageBase
{
	internal uint EnemyId { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(EnemyId);
	}

	public override void Deserialize(NetworkReader reader)
	{
		EnemyId = reader.ReadUInt32();
	}
}
