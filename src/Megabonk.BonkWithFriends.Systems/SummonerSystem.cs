using System;
using Megabonk.BonkWithFriends.Debug;
using Megabonk.BonkWithFriends.HarmonyPatches.Game;
using Megabonk.BonkWithFriends.HarmonyPatches.Spawning;
using Megabonk.BonkWithFriends.Managers.Enemies;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.Networking.Messages;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Systems;

public static class SummonerSystem
{
	[NetworkMessageHandler(MessageType.TimelineEvent)]
	private static void HandleTimelineEvent(SteamNetworkMessage message)
	{
		if (!SteamNetworkManager.IsClient || SteamNetworkManager.IsServer)
			return;
		TimelineEventMessage timelineEventMessage = message.Deserialize<TimelineEventMessage>();
		Melon<BonkWithFriendsMod>.Logger.Msg($"[Client] Timeline event {timelineEventMessage.EventIndex} received");

		// Trigger native timeline event on client for UI/audio cues
		try
		{
			var sc = SummonerPatches.CachedController;
			if ((Object)(object)sc != (Object)null)
			{
				sc.StartEvent(timelineEventMessage.EventIndex);
				Melon<BonkWithFriendsMod>.Logger.Msg($"[Client] Called StartEvent({timelineEventMessage.EventIndex})");
			}
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[Client] StartEvent failed: {ex.Message}");
		}
	}

	[NetworkMessageHandler(MessageType.WaveCue)]
	private static void HandleWaveCue(SteamNetworkMessage message)
	{
		if (!SteamNetworkManager.IsClient || SteamNetworkManager.IsServer)
			return;
		WaveCueMessage waveCueMessage = message.Deserialize<WaveCueMessage>();
		Melon<BonkWithFriendsMod>.Logger.Msg($"[Client] Wave cue: type={waveCueMessage.WaveType}, duration={waveCueMessage.Duration}");
	}

	[NetworkMessageHandler(MessageType.WaveFinalCue)]
	private static void HandleWaveFinalCue(SteamNetworkMessage message)
	{
		if (!SteamNetworkManager.IsClient || SteamNetworkManager.IsServer)
			return;
		Melon<BonkWithFriendsMod>.Logger.Msg("[Client] Final wave cue received");

		// Trigger native final swarm on client for audio/visual feedback
		try
		{
			var sc = SummonerPatches.CachedController;
			if ((Object)(object)sc != (Object)null)
			{
				sc.StartFinalSwarm();
				Melon<BonkWithFriendsMod>.Logger.Msg("[Client] Called StartFinalSwarm");
			}
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[Client] StartFinalSwarm failed: {ex.Message}");
		}
	}

	[NetworkMessageHandler(MessageType.WavesStopped)]
	private static void HandleWavesStopped(SteamNetworkMessage message)
	{
		if (!SteamNetworkManager.IsClient || SteamNetworkManager.IsServer)
			return;
		Melon<BonkWithFriendsMod>.Logger.Msg("[Client] Waves stopped");

		// Stop spawning on client for visual consistency
		try
		{
			var sc = SummonerPatches.CachedController;
			if ((Object)(object)sc != (Object)null)
			{
				sc.TryStopSummoners();
				Melon<BonkWithFriendsMod>.Logger.Msg("[Client] Called TryStopSummoners");
			}
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[Client] TryStopSummoners failed: {ex.Message}");
		}
	}

	[NetworkMessageHandler(MessageType.BossSpawnSync)]
	private static void HandleBossSpawnSync(SteamNetworkMessage message)
	{
		if (!SteamNetworkManager.IsClient || SteamNetworkManager.IsServer)
		{
			return;
		}
		BossSpawnSyncMessage bossSpawnSyncMessage = message.Deserialize<BossSpawnSyncMessage>();
		foreach (BossSpawnSyncMessage.BossInfo spawn in bossSpawnSyncMessage.Spawns)
		{
			if (RemoteEnemyManager.HasEnemy(spawn.BossPartId))
			{
				Melon<BonkWithFriendsMod>.Logger.Msg($"[Client] Boss part {spawn.BossPartId} confirmed in RemoteEnemyManager");
			}
			else
			{
				Melon<BonkWithFriendsMod>.Logger.Warning($"[Client] Boss part {spawn.BossPartId} not yet in RemoteEnemyManager");
			}
		}
	}

	[NetworkMessageHandler(MessageType.BossDied)]
	private static void HandleBossDied(SteamNetworkMessage message)
	{
		if (!SteamNetworkManager.IsClient || SteamNetworkManager.IsServer)
			return;

		BossDiedMessage bossDiedMessage = message.Deserialize<BossDiedMessage>();
		Melon<BonkWithFriendsMod>.Logger.Msg($"[Client] Boss died (lastStage={bossDiedMessage.IsLastStage})");

		// Call native OnBossDied to stop waves and activate portal on client
		try
		{
			var sc = SummonerPatches.CachedController;
			if ((Object)(object)sc != (Object)null)
			{
				sc.OnBossDied(bossDiedMessage.IsLastStage);
				Melon<BonkWithFriendsMod>.Logger.Msg("[Client] Called SummonerController.OnBossDied successfully");
			}
			else
			{
				Melon<BonkWithFriendsMod>.Logger.Warning("[Client] SummonerController not cached for OnBossDied");
			}
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Error($"[Client] Error calling OnBossDied: {ex.Message}");
		}
	}

	[NetworkMessageHandler(MessageType.GameOver)]
	private static void HandleGameOver(SteamNetworkMessage message)
	{
		if (!SteamNetworkManager.IsClient || SteamNetworkManager.IsServer)
		{
			return;
		}
		Melon<BonkWithFriendsMod>.Logger.Msg("[Client] Game over received from host");

		// Exit spectator mode if active
		try
		{
			SpectatorManager.ExitSpectatorMode();
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[Client] Error exiting spectator: {ex.Message}");
		}

		// Trigger native game over flow with re-entry guard
		GameStatePatches._isHandlingNetworkGameOver = true;
		try
		{
			var instance = Il2Cpp.GameManager.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				instance.OnDied();
				Melon<BonkWithFriendsMod>.Logger.Msg("[Client] Native OnDied triggered for game over");
			}
			else
			{
				Melon<BonkWithFriendsMod>.Logger.Warning("[Client] GameManager.Instance is null, cannot trigger OnDied");
			}
		}
		finally
		{
			GameStatePatches._isHandlingNetworkGameOver = false;
		}

		LocalPlayerManager.OnGameEnded();
	}
}
