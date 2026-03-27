using System;
using System.Collections;
using System.Collections.Generic;
using Megabonk.BonkWithFriends.HarmonyPatches.Enemies;
using Megabonk.BonkWithFriends.Managers.Enemies;
using Megabonk.BonkWithFriends.Managers.Items;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.Debug;
using Megabonk.BonkWithFriends.Managers.Server;
using Megabonk.BonkWithFriends.Networking.Steam;
using Megabonk.BonkWithFriends.Steam;
using MelonLoader;
using Steamworks;

namespace Megabonk.BonkWithFriends.Managers;

public static class PlayerSceneManager
{
	public static string _pendingSceneToLoad;

	static PlayerSceneManager()
	{
		SteamMatchmakingImpl.OnLobbyLeave = (SteamMatchmakingImpl.LobbyLeaveDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyLeave, new SteamMatchmakingImpl.LobbyLeaveDelegate(OnLobbyClosedCleanup));
	}

	private static void OnLobbyClosedCleanup(CSteamID steamLobbyId)
	{
		Melon<BonkWithFriendsMod>.Logger.Msg("[Cleanup] Lobby closed, clearing state...");
		TestModeManager.DeactivateTestMode();
		BotManager.ClearAll();
		RemotePlayerManager.ClearState();
		RemoteEnemyManager.ClearState();
		HostEnemyManager.ClearState();
		PickupSpawnManager.ClearState();
		LocalPlayerManager.ClearState();
		EnemyPatches.EnemyMovement_GetTargetPosition_Patch.ClearCache();
	}

	public static bool HasPendingSceneLoad(out string sceneName)
	{
		sceneName = _pendingSceneToLoad;
		return !string.IsNullOrEmpty(sceneName);
	}

	public static void ClearPendingSceneLoad()
	{
		_pendingSceneToLoad = null;
	}

	public static void OnSceneLoaded(string scene)
	{
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		if (!SteamNetworkManager.IsMultiplayer)
		{
			return;
		}
		Melon<BonkWithFriendsMod>.Logger.Msg("[MP] OnSceneLoaded: '" + scene + "'");
		Melon<BonkWithFriendsMod>.Logger.Msg($"[MP] OnSceneLoaded: SteamNetworkLobby Seed = {SteamNetworkLobby.Instance?.Seed}");
		Melon<BonkWithFriendsMod>.Logger.Msg($"[MP] OnSceneLoaded: SteamNetworkManager.Mode = {SteamNetworkManager.Mode}");
		if (SteamNetworkManager.IsMultiplayer)
		{
			MelonCoroutines.Start(LocalPlayerManager.AddNetworkedPlayerComponent());
			if (SteamNetworkLobby.Instance == null)
			{
				Melon<BonkWithFriendsMod>.Logger.Error("[MP] SteamNetworkLobby.Instance is null in OnSceneLoaded!");
				return;
			}
			IReadOnlyList<SteamNetworkLobbyMember> members = SteamNetworkLobby.Instance.Members;
			Melon<BonkWithFriendsMod>.Logger.Msg($"[MP] Lobby has {members.Count} members:");
			foreach (SteamNetworkLobbyMember item in members)
			{
				Melon<BonkWithFriendsMod>.Logger.Msg($"[MP] Member - SteamID: {item.UserId.m_SteamID}, Character: {item.Character}, Skin: {item.SkinType}");
				if (item.UserId == SteamUser.GetSteamID())
				{
					Melon<BonkWithFriendsMod>.Logger.Msg("[MP] Skipping self");
					continue;
				}
				if (item.UserId.m_SteamID == 0L)
				{
					Melon<BonkWithFriendsMod>.Logger.Error("[MP] Invalid Steam ID (0) detected! Skipping.");
					continue;
				}
				Melon<BonkWithFriendsMod>.Logger.Msg($"[MP] Creating remote player for: {item.UserId.m_SteamID}");
				RemotePlayerManager.OnGameStarted(item.UserId, item.Character, item.SkinType);
			}
		}
		BotManager.SpawnBots();
		MelonCoroutines.Start(DelayedAddMinimapIcons());
		if (SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer)
		{
			NetworkTimeSync.Initialize();
		}
	}

	private static IEnumerator DelayedAddMinimapIcons()
	{
		// Wait a few frames for MinimapCamera and local player icon to initialize
		yield return null;
		yield return null;
		yield return null;
		RemotePlayerManager.AddMiniMapIcon();
	}

	public static void OnSceneUnloaded(string scene)
	{
		if (SteamNetworkManager.IsMultiplayer)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("[MP] OnSceneUnloaded: '" + scene + "')");
			BotManager.ClearActiveBots();
			NetworkTimeSync.Reset();
			LocalPlayerManager.Clear();
			RemotePlayerManager.ClearState();
			RemoteEnemyManager.ClearState();
			HostEnemyManager.ClearState();
			PickupSpawnManager.ClearState();
		}
	}
}
