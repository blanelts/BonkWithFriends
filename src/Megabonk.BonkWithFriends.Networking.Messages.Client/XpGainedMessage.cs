using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Client;

[NetworkMessage(MessageType.XpGained, MessageSendFlags.ReliableNoNagle)]
internal sealed class XpGainedMessage : MessageBase
{
	internal int XpAmount { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(XpAmount);
	}

	public override void Deserialize(NetworkReader reader)
	{
		XpAmount = reader.ReadInt32();
	}
}
