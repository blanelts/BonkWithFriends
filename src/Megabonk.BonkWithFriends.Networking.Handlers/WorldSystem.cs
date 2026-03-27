using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.Networking.Messages;
using MelonLoader;

namespace Megabonk.BonkWithFriends.Networking.Handlers;

public static class WorldSystem
{
	[NetworkMessageHandler(MessageType.GameStarted)]
	private static void HandleGameStarted(SteamNetworkMessage steamNetworkMessage)
	{
		LocalPlayerManager.OnGameStarted();
		Melon<BonkWithFriendsMod>.Logger.Msg("[Client] GameStarted.");
	}
}
