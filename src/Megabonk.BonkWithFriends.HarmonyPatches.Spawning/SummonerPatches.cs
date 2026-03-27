using HarmonyLib;
using Il2CppAssets.Scripts.Actors.Enemies;
using Il2CppAssets.Scripts.Game.Spawning.New;
using Il2CppAssets.Scripts.Game.Spawning.New.Timelines;
using Il2CppSystem.Collections.Generic;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.HarmonyPatches.Spawning;

[HarmonyPatch]
internal static class SummonerPatches
{
	internal static SummonerController CachedController;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SummonerController), "Tick")]
	private static bool BlockClientTick(SummonerController __instance)
	{
		CachedController = __instance;
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined)
		{
			return SteamNetworkManager.IsServer;
		}
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SummonerController), "TickTimeline")]
	private static bool BlockClientTimeline()
	{
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined)
		{
			return SteamNetworkManager.IsServer;
		}
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SummonerController), "TryAddNewEnemyCard")]
	private static bool BlockClientAddCard()
	{
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined)
		{
			return SteamNetworkManager.IsServer;
		}
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SummonerController), "CanAddNewEnemyCard")]
	private static bool BlockClientCanAddCard(ref bool __result)
	{
		if (SteamNetworkLobbyManager.State != SteamNetworkLobbyState.Joined || SteamNetworkManager.IsServer)
		{
			return true;
		}
		__result = false;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SummonerController), "StartEvent")]
	private static void OnTimelineEvent(int eventIndex)
	{
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined && SteamNetworkManager.IsServer)
		{
			SteamNetworkServer.Instance?.BroadcastToRemoteClients(new TimelineEventMessage
			{
				EventIndex = eventIndex,
				HostTime = Time.time
			});
			Melon<BonkWithFriendsMod>.Logger.Msg($"[MP Host] Timeline event {eventIndex} started");
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SummonerController), "EventSwarm")]
	private static void OnSwarmEvent(TimelineEvent timelineEvent)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected I4, but got Unknown
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined && SteamNetworkManager.IsServer)
		{
			SteamNetworkServer.Instance?.BroadcastToRemoteClients(new WaveCueMessage
			{
				WaveType = (int)timelineEvent.eTimelineEvent,
				Duration = timelineEvent.duration
			});
			Melon<BonkWithFriendsMod>.Logger.Msg("[MP Host] Swarm event started");
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SummonerController), "StartFinalSwarm")]
	private static void OnFinalSwarm()
	{
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined && SteamNetworkManager.IsServer)
		{
			SteamNetworkServer.Instance?.BroadcastToRemoteClients(new WaveFinalCueMessage());
			Melon<BonkWithFriendsMod>.Logger.Msg("[MP Host] Final swarm started");
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SummonerController), "SpawnStageBoss")]
	private static bool BlockClientBossSpawn()
	{
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined)
		{
			return SteamNetworkManager.IsServer;
		}
		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(SummonerController), "SpawnStageBoss")]
	private static void OnBossSpawned(List<Enemy> __result, Vector3 pos)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined && SteamNetworkManager.IsServer && __result != null && __result.Count != 0)
		{
			BossSpawnSyncMessage bossSpawnSyncMessage = new BossSpawnSyncMessage();
			Il2CppSystem.Collections.Generic.List<Enemy>.Enumerator enumerator = __result.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Enemy current = enumerator.Current;
				bossSpawnSyncMessage.Spawns.Add(new BossSpawnSyncMessage.BossInfo
				{
					BossPartId = current.id,
					Position = ((Component)current).transform.position,
					MaxHp = current.maxHp
				});
			}
			SteamNetworkServer.Instance?.BroadcastToRemoteClients(bossSpawnSyncMessage);
			Melon<BonkWithFriendsMod>.Logger.Msg($"[MP Host] Boss spawned with {__result.Count} parts");
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SummonerController), "OnBossDied")]
	private static void OnBossDied(bool isLastStage)
	{
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined && SteamNetworkManager.IsServer)
		{
			SteamNetworkServer.Instance?.BroadcastToRemoteClients(new BossDiedMessage
			{
				IsLastStage = isLastStage,
				HostTime = Time.time
			});
			Melon<BonkWithFriendsMod>.Logger.Msg($"[MP Host] Boss died (last stage: {isLastStage})");
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SummonerController), "TryStopSummoners")]
	private static void OnSpawningStop()
	{
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined && SteamNetworkManager.IsServer)
		{
			SteamNetworkServer.Instance?.BroadcastToRemoteClients(new WavesStoppedMessage());
			Melon<BonkWithFriendsMod>.Logger.Msg("[MP Host] Spawning stopped");
		}
	}
}
