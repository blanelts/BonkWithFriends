using System.Collections.Generic;
using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Server;

[NetworkMessage(MessageType.HostWelcome, MessageSendFlags.ReliableNoNagle)]
internal sealed class HostWelcomeMessage : MessageBase
{
	internal sealed class PlayerInfo
	{
		internal ulong SteamUserId { get; set; }

		internal int Character { get; set; }
	}

	internal List<PlayerInfo> ExistingPlayers { get; set; } = new List<PlayerInfo>();

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write((ushort)ExistingPlayers.Count);
		foreach (PlayerInfo existingPlayer in ExistingPlayers)
		{
			writer.Write(existingPlayer.SteamUserId);
			writer.Write(existingPlayer.Character);
		}
	}

	public override void Deserialize(NetworkReader reader)
	{
		ExistingPlayers.Clear();
		ushort num = reader.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			ExistingPlayers.Add(new PlayerInfo
			{
				SteamUserId = reader.ReadUInt64(),
				Character = reader.ReadInt32()
			});
		}
	}
}
