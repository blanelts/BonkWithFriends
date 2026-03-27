using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.AnimationStateRelay, MessageSendFlags.NoNagle)]
internal sealed class AnimationStateRelayMessage : MessageBase
{
	internal ulong SteamUserId { get; set; }

	internal byte StateFlags { get; set; }

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(SteamUserId);
		writer.Write(StateFlags);
	}

	public override void Deserialize(NetworkReader reader)
	{
		SteamUserId = reader.ReadUInt64();
		StateFlags = reader.ReadByte();
	}
}
