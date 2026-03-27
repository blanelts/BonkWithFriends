using Il2Cpp;
using Il2CppAssets.Scripts._Data;
using Steamworks;

namespace Megabonk.BonkWithFriends.Networking.Steam;

internal sealed class SteamNetworkLobbyMember
{
	internal CSteamID LobbyId { get; private set; }

	internal CSteamID UserId { get; private set; }

	internal bool IsReady { get; set; }

	internal ECharacter Character { get; set; }

	internal ESkinType SkinType { get; set; }

	internal SteamNetworkLobbyMember(CSteamID steamLobbyId, CSteamID steamUserId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		LobbyId = steamLobbyId;
		UserId = steamUserId;
	}
}
