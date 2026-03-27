using HarmonyLib;
using Il2CppAssets.Scripts.Actors.Player;
using Il2CppAssets.Scripts.Inventory__Items__Pickups;
using Il2CppInventory__Items__Pickups.Xp_and_Levels;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.Networking.Messages.Client;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;

namespace Megabonk.BonkWithFriends.HarmonyPatches.Player;

[HarmonyPatch]
public static class PlayerPatches
{
	private static bool _isAddingXpFromNetwork;

	internal static void SetAddingXpFromNetwork(bool value)
	{
		_isAddingXpFromNetwork = value;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerXp), "AddXp")]
	public static void PlayerXp_AddXp_Postfix(int amount)
	{
		if (!SteamNetworkManager.IsMultiplayer || _isAddingXpFromNetwork)
			return;

		// Only host broadcasts XP — client XP is server-authoritative
		// (client receives XP via XpGainedMessage from host, not from local kills)
		if (!SteamNetworkManager.IsServer)
			return;

		XpGainedMessage tMsg = new XpGainedMessage
		{
			XpAmount = amount
		};

		SteamNetworkServer.Instance?.BroadcastToRemoteClients(tMsg);
		Melon<BonkWithFriendsMod>.Logger.Msg($"[XpSync] Host gained {amount} XP, broadcasting to remote clients");
	}

	public static bool MyPlayer_OnPlayerDied_Prefix(MyPlayer __instance)
	{
		if (SteamNetworkManager.Mode == SteamNetworkMode.None)
		{
			return true;
		}
		LocalPlayerManager.UpdatePlayerDeath(true);
		Melon<BonkWithFriendsMod>.Logger.Msg("OnPlayerDied: Player died, broadcasting death state");
		return true;
	}

	public static bool PlayerHealth_CheckDead_Prefix(PlayerHealth __instance)
	{
		return true;
	}

	public static bool PlayerHealth_Tick_Prefix(PlayerHealth __instance)
	{
		return true;
	}
}
