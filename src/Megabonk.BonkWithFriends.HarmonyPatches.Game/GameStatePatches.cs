using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Actors.Player;
using Il2CppAssets.Scripts.Camera;
using Il2CppAssets.Scripts.Game.Other;
using Il2CppAssets.Scripts.Managers;
using Il2CppAssets.Scripts.UI.InGame.Levelup;
using Il2CppAssets.Scripts.UI.InGame.Rewards;
using Il2CppAssets.Scripts.Utility;
using Il2CppAssets.Scripts._Data;
using Il2CppAssets.Scripts._Data.MapsAndStages;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using Il2CppTMPro;
using Il2CppUtility;
using Megabonk.BonkWithFriends.Debug;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Megabonk.BonkWithFriends.HarmonyPatches.Game;

[HarmonyPatch]
public static class GameStatePatches
{
	[HarmonyPatch(typeof(Il2Cpp.SteamManager), "Load", new System.Type[] { })]
	private static class SteamManagerLoadPatch
	{
		private static bool Prefix(SteamManager __instance)
		{
			return false;
		}
	}

	private static PauseUi _pauseUi;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameManager), "StartPlaying")]
	public static void OnGameStarted()
	{
		if (SteamNetworkLobbyManager.State != SteamNetworkLobbyState.None)
		{
			_ = SteamNetworkManager.Mode;
			_ = 1;
		}
	}

	private static bool ConfirmCharacterPatch_Prefix()
	{
		if (!SteamNetworkManager.IsMultiplayer)
		{
			return true;
		}
		if (SteamNetworkManager.IsServer)
		{
			return true;
		}
		CharacterInfoUI val = Object.FindObjectOfType<CharacterInfoUI>();
		if (((Object)((Object)(object)val)))
		{
			Button val2 = ((IEnumerable<Button>)((Component)val).GetComponentsInChildren<Button>(true)).FirstOrDefault((Func<Button, bool>)((Button b) => ((Object)b).name == "B_Confirm"));
			if (((Object)((Object)(object)val2)))
			{
				((Component)val2).GetComponentInChildren<TMP_Text>().text = "Waiting for host..";
				((Component)val2).GetComponent<ResizeOnLocalization>().DelayedRefresh();
			}
		}
		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(MyPlayer), "Start")]
	private static void PlayerSpawned()
	{
		_ = SteamNetworkManager.Mode;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MyTime), "Pause")]
	private static bool Pause_Prefix()
	{
		if (!SteamNetworkManager.IsMultiplayer)
		{
			return true;
		}
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(EncounterWindows), "PopReward")]
	private static bool PopReward_Prefix(EncounterWindows __instance)
	{
		if (!SteamNetworkManager.IsMultiplayer)
		{
			return true;
		}
		PopRewardWithoutPause(__instance);
		return false;
	}

	private static void PopRewardWithoutPause(EncounterWindows instance)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected I4, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		instance.encounterInProgress = true;
		if (instance.rewardQueue != null && instance.rewardQueue.Count != 0)
		{
			EEncounter val = instance.rewardQueue.Dequeue();
			BaseEncounterWindow val2;
			switch ((int)val)
			{
			case 0:
			case 6:
			case 7:
				val2 = instance.levelupScreen;
				break;
			case 3:
			case 4:
			case 5:
			case 10:
			case 11:
				val2 = instance.chestWindow;
				break;
			case 9:
				val2 = instance.microwaveWindow;
				break;
			default:
				val2 = instance.genericEncounterWindow;
				break;
			}
			instance.activeEncounterWindow = val2;
			if (((Object)((Object)(object)val2)))
			{
				val2.Open(val);
			}
			PauseUi pauseUi = GetPauseUi();
			if (((Behaviour)pauseUi).isActiveAndEnabled)
			{
				pauseUi.Resume();
			}
			Il2CppSystem.Action a_WindowOpened = EncounterWindows.A_WindowOpened;
			if (a_WindowOpened != null)
			{
				a_WindowOpened.Invoke();
			}
		}
	}

	private static PauseUi GetPauseUi()
	{
		if (((Object)((Object)(object)_pauseUi)))
		{
			return _pauseUi;
		}
		try
		{
			UiManager instance = UiManager.Instance;
			if ((Object)(object)instance != (Object)null && (Object)(object)instance.pause != (Object)null)
			{
				_pauseUi = instance.pause;
				return _pauseUi;
			}
			PauseHandler val = Object.FindObjectOfType<PauseHandler>();
			if (val != null && val.pauseUi != null)
			{
				_pauseUi = val.pauseUi;
				return _pauseUi;
			}
			_pauseUi = Object.FindObjectOfType<PauseUi>();
			if (_pauseUi != null)
			{
				return _pauseUi;
			}
		}
		catch (Exception value)
		{
			Melon<BonkWithFriendsMod>.Logger.Error($"Error locating PauseUi: {value}");
		}
		return _pauseUi;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MyTime), "Update")]
	private static bool MyTime_Update_Prefix()
	{
		if (!SteamNetworkManager.IsMultiplayer)
		{
			return true;
		}
		MyTime.paused = false;
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MyTime), "FixedUpdate")]
	private static bool MyTime_FixedUpdate_Prefix()
	{
		if (!SteamNetworkManager.IsMultiplayer)
		{
			return true;
		}
		MyTime.paused = false;
		return true;
	}

	private static bool Unpause_Prefix()
	{
		if (!SteamNetworkManager.IsMultiplayer)
		{
			return true;
		}
		return false;
	}

	private static bool StartMap_Prefix()
	{
		_ = SteamNetworkManager.Mode;
		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(CharacterInfoUI), "OnCharacterSelected")]
	private static void CharacterSelectionPatch_Postfix(MyButtonCharacter btn)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkManager.IsMultiplayer && ((Object)((Object)(object)btn)) && SteamNetworkLobby.Instance != null)
		{
			SteamNetworkLobby.Instance.MemberSetCharacter(btn.characterData.eCharacter);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(MapStatsInfoUi), "SetConfig")]
	private static void MapStatsInfoUISetConfig_Postfix(MapStatsInfoUi __instance, RunConfig runConfig)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkManager.IsMultiplayer && runConfig != null && ((Object)((Object)(object)runConfig.mapData)) && SteamNetworkManager.IsServer && SteamNetworkLobby.Instance != null)
		{
			SteamNetworkLobby.Instance.SetMap(runConfig.mapData.eMap);
			SteamNetworkLobby.Instance.SetTier(runConfig.mapTierIndex);
			SteamNetworkLobby.Instance.SetChallenge(runConfig.challenge);
			Melon<BonkWithFriendsMod>.Logger.Msg("Map Tier Index: " + runConfig.mapTierIndex);
			Melon<BonkWithFriendsMod>.Logger.Msg("Map Data: " + ((Object)runConfig.mapData).ToString());
			if (((Object)((Object)(object)runConfig.challenge)))
			{
				Melon<BonkWithFriendsMod>.Logger.Msg("challenge Data: " + ((Object)runConfig.challenge).ToString());
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(SkinSelection), "SetCurrentlySelected")]
	private static void SkinSelectionCurrenlySelected_Postfix(SkinSelection __instance)
	{
		if (SteamNetworkManager.IsMultiplayer && SteamNetworkLobby.Instance != null)
		{
			SteamNetworkLobby.Instance.MemberSetSkinType((ESkinType)__instance.currentlySelected);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MapController), "StartNewMap")]
	private static void MapControllerStartNewMap_Prefix(RunConfig newRunConfig)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkManager.IsMultiplayer)
		{
			int seed = SteamNetworkLobby.Instance?.Seed ?? SteamNetworkManager.NetworkSeed;
			Melon<BonkWithFriendsMod>.Logger.Msg($"[MapController] StartNewMap - Reinitializing RNGs with seed: {seed}");
			Random.InitState(seed);
			MapGenerator.seed = seed;
			MyRandom.random = (Il2CppSystem.Random)new ConsistentRandom(seed);
			var logger = Melon<BonkWithFriendsMod>.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 4);
			defaultInterpolatedStringHandler.AppendLiteral("[MapController] Map: ");
			EMap? value;
			if (newRunConfig == null)
			{
				value = null;
			}
			else
			{
				MapData mapData = newRunConfig.mapData;
				value = ((mapData != null) ? new EMap?(mapData.eMap) : ((EMap?)null));
			}
			defaultInterpolatedStringHandler.AppendFormatted(value);
			defaultInterpolatedStringHandler.AppendLiteral(", Tier: ");
			defaultInterpolatedStringHandler.AppendFormatted((newRunConfig != null) ? new int?(newRunConfig.mapTierIndex) : ((int?)null));
			defaultInterpolatedStringHandler.AppendLiteral(", Challenge: ");
			defaultInterpolatedStringHandler.AppendFormatted<ChallengeData>((newRunConfig != null) ? newRunConfig.challenge : null);
			defaultInterpolatedStringHandler.AppendLiteral(", Music: ");
			defaultInterpolatedStringHandler.AppendFormatted((newRunConfig != null) ? new int?(newRunConfig.musicTrackIndex) : ((int?)null));
			logger.Msg(defaultInterpolatedStringHandler.ToStringAndClear());
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RunConfig), "GetEnemyHp")]
	private static void RunConfig_GetEnemyHp_Postfix(ref float __result)
	{
		if (SteamNetworkManager.IsMultiplayer)
		{
			__result *= Preferences.EnemyHpModifer.Value;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RunConfig), "GetEnemyDamage")]
	private static void RunConfig_GetEnemyDamage_Postfix(ref float __result)
	{
		if (SteamNetworkManager.IsMultiplayer)
		{
			__result *= Preferences.EnemyDmgModifer.Value;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RunConfig), "GetEnemySpeed")]
	private static void RunConfig_GetEnemySpeed_Postfix(ref float __result)
	{
		if (SteamNetworkManager.IsMultiplayer)
		{
			__result *= Preferences.EnemySpeedModifer.Value;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(MinimapCamera), "Start")]
	private static void MinimapCamera_GetEnemySpeed_Postfix()
	{
		// Minimap icons are added via PlayerSceneManager.DelayedAddMinimapIcons() coroutine
		// to avoid double-creation race condition
	}

	internal static bool _isHandlingNetworkGameOver;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameManager), "get_isGameOver")]
	private static void GameManager_IsGameOver_Postfix(ref bool __result)
	{
		if (!SteamNetworkManager.IsMultiplayer || _isHandlingNetworkGameOver)
			return;
		// In multiplayer, force isGameOver=false while any teammate is alive
		// This keeps SummonerController ticking and boss timeline progressing
		if (SteamNetworkManager.IsServer && !RemotePlayerManager.AreAllRemotePlayersDead())
		{
			__result = false;
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GameManager), "OnDied")]
	private static bool GameManager_OnDied_Prefix(GameManager __instance)
	{
		if (!SteamNetworkManager.IsMultiplayer)
			return true;

		// Allow re-entry from network game over handler
		if (_isHandlingNetworkGameOver)
			return true;

		LocalPlayerManager.UpdatePlayerDeath(true);

		if (SteamNetworkManager.IsServer)
		{
			// Server: check if all players are dead
			if (!RemotePlayerManager.AreAllRemotePlayersDead())
			{
				__instance._isGameOver_k__BackingField = false;
				Melon<BonkWithFriendsMod>.Logger.Msg("Player died but others alive - entering spectator mode");
				EnterSpectatorMode();
				return false;
			}
			// All dead — broadcast game over
			__instance._isGameOver_k__BackingField = true;
			Melon<BonkWithFriendsMod>.Logger.Msg("All players dead - game over");
			SteamNetworkServer.Instance?.BroadcastToRemoteClients(new GameOverMessage());
			return true;
		}
		else
		{
			// Client: ALWAYS enter spectator on death. Server will decide game over.
			__instance._isGameOver_k__BackingField = false;
			Melon<BonkWithFriendsMod>.Logger.Msg("[Client] Player died - entering spectator (server decides game over)");
			EnterSpectatorMode();
			return false;
		}
	}

	private static void EnterSpectatorMode()
	{
		try
		{
			SpectatorManager.EnterSpectatorMode();
			Melon<BonkWithFriendsMod>.Logger.Msg("Entered spectator mode");
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("Error entering spectator mode: " + ex.Message);
		}
	}

	public static void PrintGeneratedMapList()
	{
		MapData currentMap = MapController.currentMap;
		if (!((Object)((Object)(object)currentMap)))
		{
			Melon<BonkWithFriendsMod>.Logger.Error("MapData is NULL!");
			return;
		}
		Melon<BonkWithFriendsMod>.Logger.Msg("========================================");
		Melon<BonkWithFriendsMod>.Logger.Msg("MAP GENERATION REPORT");
		Melon<BonkWithFriendsMod>.Logger.Msg($"Seed: {MapGenerator.seed}");
		Il2CppReferenceArray<StageData> stages = currentMap.stages;
		if (stages == null)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("Stage List is NULL!");
		}
		else
		{
			Melon<BonkWithFriendsMod>.Logger.Msg($"Generated Count: {((Il2CppArrayBase<StageData>)(object)stages).Count}");
			for (int i = 0; i < ((Il2CppArrayBase<StageData>)(object)stages).Count; i++)
			{
				StageData val = ((Il2CppArrayBase<StageData>)(object)stages)[i];
				if (((Object)((Object)(object)val)))
				{
					string name = val.GetName();
					Melon<BonkWithFriendsMod>.Logger.Msg($"[{i}] {name}");
				}
			}
		}
		Melon<BonkWithFriendsMod>.Logger.Msg("========================================");
	}
}
