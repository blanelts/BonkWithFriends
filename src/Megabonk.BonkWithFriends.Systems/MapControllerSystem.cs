using Il2Cpp;
using Il2CppAssets.Scripts.Managers;
using Il2CppUtility;
using Megabonk.BonkWithFriends.HarmonyPatches.Game;
using Megabonk.BonkWithFriends.Networking.Messages;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Systems;

public static class MapControllerSystem
{
	[NetworkMessageHandler(MessageType.LoadStage)]
	private static void HandleLoadStage(SteamNetworkMessage message)
	{
		if (SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer)
		{
			LoadStageMessage loadStageMessage = message.Deserialize<LoadStageMessage>();
			Melon<BonkWithFriendsMod>.Logger.Msg($"[Client] Host ordered load to Stage Index: {loadStageMessage.StageIndex}, Seed: {loadStageMessage.Seed}");

			// Re-initialize RNG with host's seed to ensure identical map generation
			if (loadStageMessage.Seed != 0)
			{
				SteamNetworkManager.NetworkSeed = loadStageMessage.Seed;
				Random.InitState(loadStageMessage.Seed);
				MapGenerator.seed = loadStageMessage.Seed;
				MyRandom.random = (Il2CppSystem.Random)new ConsistentRandom(loadStageMessage.Seed);
			}

			MapController.index = loadStageMessage.StageIndex - 1;
			MapControllerPatches.SetNetworkLoading(value: true);
			MapController.LoadNextStage();
		}
	}

	[NetworkMessageHandler(MessageType.RequestLoadStage)]
	private static void HandleRequestLoadStage(SteamNetworkMessage message)
	{
		if (!SteamNetworkManager.IsServer)
			return;

		Melon<BonkWithFriendsMod>.Logger.Msg($"[Server] Client {message.SteamUserId} requested stage load");
		MapController.LoadNextStage();
	}
}
