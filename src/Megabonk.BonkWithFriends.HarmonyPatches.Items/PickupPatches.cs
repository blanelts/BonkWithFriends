using HarmonyLib;
using Il2Cpp;
using Megabonk.BonkWithFriends.Managers.Items;
using Megabonk.BonkWithFriends.Networking.Messages.Client;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.HarmonyPatches.Items;

[HarmonyPatch]
public static class PickupPatches
{
	private static bool _blockingEnemyDeathSpawns;

	public static void SetBlockingEnemyDeathSpawns(bool value) => _blockingEnemyDeathSpawns = value;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(PickupManager), "SpawnPickup")]
	public static bool OnPickupSpawned_Prefix(ref Pickup __result)
	{
		if (SteamNetworkManager.Mode == SteamNetworkMode.None)
			return true;
		if (PickupSpawnManager.IsSpawningFromNetwork || SteamNetworkManager.IsServer)
			return true;
		// On client: block only enemy-death spawns (server handles those).
		// Allow jar/pot/destructible spawns through.
		if (_blockingEnemyDeathSpawns)
			return false;
		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PickupManager), "SpawnPickup")]
	public static void OnPickupSpawned_Postfix(Pickup __result, int ePickup, Vector3 pos, int value)
	{
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined && SteamNetworkManager.IsServer && !((Object)(object)__result == (Object)null) && !PickupSpawnManager.IsSpawningFromNetwork)
		{
			PickupSpawnManager.BroadcastPickupSpawned(__result, ePickup);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Pickup), "StartFollowingPlayer")]
	public static bool OnPickupStartFollow_Prefix(Pickup __instance)
	{
		if (SteamNetworkManager.Mode == SteamNetworkMode.None)
			return true;

		int pickupId = PickupSpawnManager.GetPickupId(__instance);
		if (pickupId == -1)
		{
			// Unregistered pickup (jar/pot drops) — allow native collection
			return true;
		}

		if (!SteamNetworkManager.IsMultiplayer)
			return true;

		Melon<BonkWithFriendsMod>.Logger.Msg($"[PickupPatches] Player collected pickup ID {pickupId}");

		if (SteamNetworkManager.IsServer)
		{
			// Host: collect natively + broadcast despawn to clients
			SteamNetworkServer.Instance?.BroadcastToRemoteClients(new PickupCollectedMessage
			{
				PickupId = pickupId
			});
			return true;
		}
		else
		{
			// Client: BLOCK native collection — XP comes from host's XpGainedMessage
			SteamNetworkClient.Instance?.SendMessage(new PickupCollectedMessage
			{
				PickupId = pickupId
			});
			// Despawn the local pickup copy
			PickupSpawnManager.ProcessPickupCollection(pickupId);
			return false;
		}
	}

	public static void ProcessPendingSpawns()
	{
		PickupSpawnManager.ProcessPendingSpawns();
	}

	public static void ProcessPendingDespawns()
	{
		PickupSpawnManager.ProcessPendingDespawns();
	}
}
