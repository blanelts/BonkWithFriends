using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.AnimationState, MessageSendFlags.NoNagle)]
internal sealed class AnimationStateMessage : MessageBase
{
	internal byte StateFlags { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(StateFlags);
	}

	public override void Deserialize(NetworkReader reader)
	{
		StateFlags = reader.ReadByte();
	}
}
