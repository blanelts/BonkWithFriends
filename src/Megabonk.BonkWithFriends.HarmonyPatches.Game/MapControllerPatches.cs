using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Managers;
using Il2CppUtility;
using Megabonk.BonkWithFriends.Managers.Enemies;
using Megabonk.BonkWithFriends.Managers.Server;
using Megabonk.BonkWithFriends.Networking.Messages.Client;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using Random = UnityEngine.Random;

namespace Megabonk.BonkWithFriends.HarmonyPatches.Game;

[HarmonyPatch]
public static class MapControllerPatches
{
	private static bool _isNetworkLoading;

	internal static bool IsNetworkLoading => _isNetworkLoading;

	internal static void SetNetworkLoading(bool value)
	{
		_isNetworkLoading = value;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MapController), "LoadNextStage")]
	private static bool LoadNextStage_Prefix()
	{
		if (SteamNetworkManager.IsServer)
		{
			// Re-seed RNG deterministically before stage generation so host & client match
			int stageSeed = SteamNetworkManager.NetworkSeed + MapController.index + 1;
			Random.InitState(stageSeed);
			MapGenerator.seed = stageSeed;
			MyRandom.random = (Il2CppSystem.Random)new ConsistentRandom(stageSeed);
			Melon<BonkWithFriendsMod>.Logger.Msg($"[Host] Re-seeded RNG for stage {MapController.index + 1} with seed {stageSeed}");
			return true;
		}

		if (SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer)
		{
			if (_isNetworkLoading)
				return true; // Host told us to load

			// Client entered portal natively — request host to trigger stage load
			SteamNetworkClient.Instance?.SendMessage(new RequestLoadStageMessage());
			Melon<BonkWithFriendsMod>.Logger.Msg("[Client] Sent RequestLoadStage to host");
			return false;
		}
		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(MapController), "LoadNextStage")]
	private static void LoadNextStage_Postfix()
	{
		GameStatePatches.PrintGeneratedMapList();
		if (SteamNetworkManager.IsServer)
		{
			int index = MapController.index;
			int stageSeed = SteamNetworkManager.NetworkSeed + index;
			HostEnemyManager.ClearState();
			LoadStageMessage tMsg = new LoadStageMessage
			{
				StageIndex = index,
				Seed = stageSeed
			};
			SteamNetworkServer.Instance.BroadcastToRemoteClients(tMsg);
			Melon<BonkWithFriendsMod>.Logger.Msg($"[Host] Broadcast LoadStage: index={index}, seed={stageSeed}");
		}
		else if (SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer && _isNetworkLoading)
		{
			NetworkTimeSync.Reset();
			_isNetworkLoading = false;
		}
	}
}
