using System;
using System.Collections;
using System.Reflection;
using Il2Cpp;
using Il2CppAssets.Scripts.Actors.Player;
using Il2CppAssets.Scripts.Camera;
using Il2CppAssets.Scripts.Game.Other;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Weapons;
using Il2CppAssets.Scripts.Managers;
using Il2CppAssets.Scripts.Menu.Shop;
using Il2CppAssets.Scripts._Data.MapsAndStages;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using Megabonk.BonkWithFriends.HarmonyPatches.Game;
using Megabonk.BonkWithFriends.MonoBehaviours.Player;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Networking.Steam;
using Megabonk.BonkWithFriends.Steam;
using Megabonk.BonkWithFriends.UI;
using SteamManager = Megabonk.BonkWithFriends.Steam.SteamManager;
using MelonLoader;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Megabonk.BonkWithFriends.Managers.Player;

public static class LocalPlayerManager
{
	public static NetworkedPlayer LocalPlayer;

	public static PlayerState LocalPlayerState;

	public static MyPlayer _myPlayer;

	private static PlayerInventory _playerInventory;

	private static ulong SteamUserId;

	private static uint _nextAttackId;

	private const float STATE_BROADCAST_RATE = 0.1f;

	private static float _lastStateBroadcastTime;

	private static bool _stateDirty;

	public static bool IsGameActive { get; private set; }

	static LocalPlayerManager()
	{
		_playerInventory = null;
		_nextAttackId = 1u;
		_lastStateBroadcastTime = 0f;
		_stateDirty = false;
		IsGameActive = false;
		SteamMatchmakingImpl.OnLobbyEnterReceivedSuccess = (SteamMatchmakingImpl.LobbyEnterSuccessCallbackDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyEnterReceivedSuccess, new SteamMatchmakingImpl.LobbyEnterSuccessCallbackDelegate(OnLobbyJoined));
	}

	public static void OnLobbyJoined(CSteamID steamLobbyId, bool lobbyLocked)
	{
		MelonCoroutines.Start(WaitForSeedAndInitialize());
	}

	private static IEnumerator WaitForSeedAndInitialize()
	{
		while ((SteamNetworkLobby.Instance?.Seed ?? SteamNetworkManager.NetworkSeed) == 0 && (!SteamNetworkManager.IsClient || SteamNetworkManager.IsServer))
		{
			yield return null;
		}
		yield return null;
		MultiplayerUI.CreateLobbyMenus();
		MelonCoroutines.Start(MultiplayerUI.WaitForLobbyAndOpen());
	}

	public static void OnGameStarted()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkLobby.Instance == null)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("SteamNetworkLobby.Instance is null in OnGameStarted!");
			return;
		}
		IsGameActive = true;
		Melon<BonkWithFriendsMod>.Logger.Msg("[LocalPlayerManager] Game started - IsGameActive = true");
		if (SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer)
		{
			EMap map = SteamNetworkLobby.Instance.Map;
			int tier = SteamNetworkLobby.Instance.Tier;
			MapData map2 = DataManager.Instance.GetMap(map);
			StageData stageData = ((Il2CppArrayBase<StageData>)(object)map2.stages)[tier];
			MapController.StartNewMap(new RunConfig
			{
				mapData = map2,
				stageData = stageData,
				mapTierIndex = SteamNetworkLobby.Instance.Tier,
				challenge = SteamNetworkLobby.Instance.Challenge
			});
		}
		if (SteamNetworkManager.IsServer)
		{
			Scene activeScene = SceneManager.GetActiveScene();
			string name = activeScene.name;
			Melon<BonkWithFriendsMod>.Logger.Msg("Host: starting co-op on scene '" + name + "'");
		}
		GameStatePatches.PrintGeneratedMapList();
	}

	public static void OnGameEnded()
	{
		IsGameActive = false;
		Melon<BonkWithFriendsMod>.Logger.Msg("[LocalPlayerManager] Game ended - IsGameActive = false");
	}

	public static IEnumerator AddNetworkedPlayerComponent()
	{
		if (((Object)((Object)(object)LocalPlayer)))
		{
			yield break;
		}
		if (((Object)((Object)(object)GameManager.Instance)))
		{
			_myPlayer = GameManager.Instance.GetPlayer();
			Melon<BonkWithFriendsMod>.Logger.Msg("_myPlayer: " + ((Object)((Component)_myPlayer).gameObject).name);
		}
		try
		{
			NetworkedPlayer networkedPlayer = ((Component)_myPlayer).gameObject.AddComponent<NetworkedPlayer>();
			Melon<BonkWithFriendsMod>.Logger.Msg("Added NetworkedPlayer component to local player");
			if (((Object)((Object)(object)networkedPlayer)))
			{
				SteamUserId = SteamManager.Instance.CurrentUserId.m_SteamID;
				networkedPlayer.Initialize(SteamUserId, isLocal: true, SteamNetworkManager.IsServer);
				LocalPlayer = networkedPlayer;
				Melon<BonkWithFriendsMod>.Logger.Msg("LocalPlayerManager initialized successfully");
			}
		}
		catch (Exception value)
		{
			Melon<BonkWithFriendsMod>.Logger.Error($"Error in AddNetworkedPlayerComponent: {value}");
		}
	}

	public static void UpdatePlayerHp(int newHp)
	{
		if (LocalPlayerState.CurrentHp != newHp)
		{
			LocalPlayerState.CurrentHp = newHp;
			_stateDirty = true;
		}
	}

	public static void UpdatePlayerMaxHp(int newMaxHp)
	{
		if (LocalPlayerState.MaxHp != newMaxHp)
		{
			LocalPlayerState.MaxHp = newMaxHp;
			_stateDirty = true;
		}
	}

	public static void UpdatePlayerShield(float newShield)
	{
		if (!Mathf.Approximately(LocalPlayerState.Shield, newShield))
		{
			LocalPlayerState.Shield = newShield;
			_stateDirty = true;
		}
	}

	public static void UpdatePlayerMaxShield(float newMaxShield)
	{
		if (!Mathf.Approximately(LocalPlayerState.MaxShield, newMaxShield))
		{
			LocalPlayerState.MaxShield = newMaxShield;
			_stateDirty = true;
		}
	}

	public static void UpdatePlayerLevel(int newLevel)
	{
		if (LocalPlayerState.Level != newLevel)
		{
			LocalPlayerState.Level = newLevel;
			_stateDirty = true;
		}
	}

	public static void UpdatePlayerXp(int newXp)
	{
		if (LocalPlayerState.Xp != newXp)
		{
			LocalPlayerState.Xp = newXp;
			_stateDirty = true;
		}
	}

	public static void UpdatePlayerDeath(bool isDead)
	{
		if (LocalPlayerState.IsDead != isDead)
		{
			LocalPlayerState.IsDead = isDead;
			BroadcastPlayerStateChange(forceImmediate: true);
		}
	}

	public static PlayerInventory GetPlayerInventory()
	{
		if (!((Object)((Object)(object)_myPlayer)))
		{
			return null;
		}
		return _myPlayer.inventory;
	}

	public static void BroadcastPlayerStateChange(bool forceImmediate = false)
	{
		if (!((Object)((Object)(object)LocalPlayer)))
		{
			return;
		}
		float unscaledTime = Time.unscaledTime;
		bool flag = unscaledTime - _lastStateBroadcastTime >= 0.1f;
		if (!forceImmediate && (!flag || !_stateDirty))
		{
			return;
		}
		PlayerInventory playerInventory = GetPlayerInventory();
		if (playerInventory != null)
		{
			if (SteamNetworkManager.IsMultiplayer)
			{
				PlayerStateRelayMessage relayMsg = new PlayerStateRelayMessage
				{
					SteamUserId = SteamUserId,
					Hp = playerInventory.playerHealth.hp,
					MaxHp = playerInventory.playerHealth.maxHp,
					Shield = playerInventory.playerHealth.shield,
					MaxShield = playerInventory.playerHealth.maxShield,
					Level = playerInventory.playerXp.level,
					Xp = playerInventory.playerXp.xp,
					IsDead = (playerInventory.playerHealth.hp <= 0)
				};
				SteamNetworkClient.Instance?.SendMessage(relayMsg);
			}
			_lastStateBroadcastTime = unscaledTime;
			_stateDirty = false;
		}
	}

	public static void Clear()
	{
		OnGameEnded();
		LocalPlayer = null;
		LocalPlayerState = default(PlayerState);
	}

	public static void SendAttackStarted(WeaponBase weapon, Vector3 spawnPosition, Quaternion spawnRotation)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected I4, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected I4, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkManager.IsMultiplayer)
		{
			uint attackId = _nextAttackId++;
			int attackQuantity = WeaponUtility.GetAttackQuantity(weapon);
			float burstInterval = WeaponUtility.GetBurstInterval(weapon);
			float projectileSize = GetProjectileSize(weapon);
			WeaponAttackStartedRelayMessage relayMsg = new WeaponAttackStartedRelayMessage
			{
				SteamUserId = SteamUserId,
				WeaponType = (int)weapon.weaponData.eWeapon,
				ProjectileCount = attackQuantity,
				BurstInterval = burstInterval,
				ProjectileSize = projectileSize,
				SpawnPosition = spawnPosition,
				SpawnRotation = spawnRotation,
				AttackId = attackId
			};
			SteamNetworkClient.Instance?.SendMessage(relayMsg);
		}
	}

	public static void SendProjectileSpawned(uint attackId, int projectileIndex, Vector3 position, Quaternion rotation)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkManager.IsMultiplayer)
		{
			WeaponProjectileSpawnedMessage tMsg = new WeaponProjectileSpawnedMessage
			{
				AttackId = attackId,
				ProjectileIndex = projectileIndex,
				Position = position,
				Rotation = rotation
			};
			Melon<BonkWithFriendsMod>.Logger.Msg($"[PlayerAttackBroadcaster] Projectile spawned: Attack={attackId}, Index={projectileIndex}");
			SteamNetworkClient.Instance?.SendMessage(tMsg);
		}
	}

	public static void SendProjectileHit(uint attackId, int projectileIndex, Vector3 hitPosition, Vector3 hitNormal, uint targetId, float damage)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkManager.IsMultiplayer)
		{
			WeaponProjectileHitMessage tMsg = new WeaponProjectileHitMessage
			{
				AttackId = attackId,
				ProjectileIndex = projectileIndex,
				HitPosition = hitPosition,
				HitNormal = hitNormal,
				TargetId = targetId,
				Damage = damage
			};
			Melon<BonkWithFriendsMod>.Logger.Msg($"[PlayerAttackBroadcaster] Projectile hit: Attack={attackId}, Target={targetId}, Damage={damage:F1}");
			SteamNetworkClient.Instance?.SendMessage(tMsg);
		}
	}

	private static float GetProjectileSize(WeaponBase weapon)
	{
		float num = 1f;
		if (((weapon != null) ? weapon.weaponStats : null) != null && weapon.weaponStats.ContainsKey((EStat)9))
		{
			num = weapon.weaponStats[(EStat)9];
		}
		return num * 1f - 1f + 1f;
	}

	public static void ClearState()
	{
		_nextAttackId = 1u;
	}

	public static void AnalyzeMinimapSetup()
	{
		MinimapCamera val = Object.FindObjectOfType<MinimapCamera>();
		if ((Object)(object)val == (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("MinimapCamera not found!");
			return;
		}
		Camera component = ((Component)val).GetComponent<Camera>();
		if ((Object)(object)component != (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg($"[Minimap] Camera culling mask: {component.cullingMask}");
			Melon<BonkWithFriendsMod>.Logger.Msg("[Minimap] Camera culling mask (binary): " + Convert.ToString(component.cullingMask, 2));
			for (int i = 0; i < 32; i++)
			{
				if ((component.cullingMask & (1 << i)) != 0)
				{
					Melon<BonkWithFriendsMod>.Logger.Msg($"[Minimap] Layer {i} ({LayerMask.LayerToName(i)}) is visible");
				}
			}
		}
		MinimapPlayerIcon val2 = Object.FindObjectOfType<MinimapPlayerIcon>();
		if ((Object)(object)val2 != (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg($"[Minimap] Player icon layer: {((Component)val2).gameObject.layer} ({LayerMask.LayerToName(((Component)val2).gameObject.layer)})");
		}
		foreach (InteractableShrineChallenge item in Object.FindObjectsOfType<InteractableShrineChallenge>())
		{
			if ((Object)(object)item.minimapIcon != (Object)null)
			{
				Melon<BonkWithFriendsMod>.Logger.Msg($"[Minimap] Shrine icon layer: {item.minimapIcon.layer} ({LayerMask.LayerToName(item.minimapIcon.layer)})");
				Transform parent = item.minimapIcon.transform.parent;
				Melon<BonkWithFriendsMod>.Logger.Msg("[Minimap] Shrine icon parent: " + (((Object)(object)parent != (Object)null) ? ((Object)parent).name : "null"));
			}
		}
		object obj = ((object)val).GetType().GetField("arrowPrefab", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(val);
		if (obj != null)
		{
			Transform val3 = (Transform)((obj is Transform) ? obj : null);
			if (val3 != null)
			{
				Melon<BonkWithFriendsMod>.Logger.Msg($"[Minimap] Arrow prefab layer: {((Component)val3).gameObject.layer} ({LayerMask.LayerToName(((Component)val3).gameObject.layer)})");
			}
		}
	}
}
