using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Chests;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Interactables;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace Megabonk.BonkWithFriends.HarmonyPatches.Items;

[HarmonyPatch]
public static class SpawnInteractablesPatches
{
	private static readonly List<InteractableSpawnData> _pendingSpawns = new List<InteractableSpawnData>(64);

	private static readonly Dictionary<int, GameObject> _idToObj = new Dictionary<int, GameObject>();

	private static readonly Dictionary<GameObject, int> _objToId = new Dictionary<GameObject, int>();

	private static int _nextInteractableId = 0;

	private static bool _isHostSpawning = false;

	private static SpawnInteractables _cachedSpawnerInstance;

	public static void Reset()
	{
		_pendingSpawns.Clear();
		_idToObj.Clear();
		_objToId.Clear();
		_isHostSpawning = false;
		_nextInteractableId = 0;
		_cachedSpawnerInstance = null;
	}

	private static SpawnInteractables GetSpawnerInstance()
	{
		if ((Object)(object)_cachedSpawnerInstance == (Object)null)
		{
			_cachedSpawnerInstance = Object.FindObjectOfType<SpawnInteractables>();
		}
		return _cachedSpawnerInstance;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(UnityEngine.Object), "Instantiate", new System.Type[]
	{
		typeof(UnityEngine.Object),
		typeof(Vector3),
		typeof(Quaternion)
	})]
	public static void OnGameObjectInstantiated(UnityEngine.Object __result, Vector3 position, Quaternion rotation)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (_isHostSpawning)
		{
			GameObject val = (GameObject)(object)((__result is GameObject) ? __result : null);
			if (val != null && TryDetectInteractable(val, out var type, out var subType))
			{
				int num = _nextInteractableId++;
				_idToObj[num] = val;
				_objToId[val] = num;
				_pendingSpawns.Add(new InteractableSpawnData(num, type, position, rotation, subType));
			}
		}
	}

	private static bool TryDetectInteractable(GameObject go, out InteractableType type, out int subType)
	{
		type = InteractableType.Other;
		subType = 0;
		InteractableChest component = go.GetComponent<InteractableChest>();
		if ((Object)(object)component != (Object)null)
		{
			bool flag = component.GetPrice() == 0;
			type = (flag ? InteractableType.ChestFree : InteractableType.Chest);
			return true;
		}
		if (((Object)((Object)(object)go.GetComponent<InteractableShrineBalance>())))
		{
			type = InteractableType.Shrine;
			subType = 0;
			return true;
		}
		if (((Object)((Object)(object)go.GetComponent<InteractableShrineChallenge>())))
		{
			type = InteractableType.Shrine;
			subType = 1;
			return true;
		}
		if (((Object)((Object)(object)go.GetComponent<InteractableShrineCursed>())))
		{
			type = InteractableType.Shrine;
			subType = 2;
			return true;
		}
		if (((Object)((Object)(object)go.GetComponent<InteractableShrineGreed>())))
		{
			type = InteractableType.Shrine;
			subType = 3;
			return true;
		}
		if (((Object)((Object)(object)go.GetComponent<InteractableShrineMagnet>())))
		{
			type = InteractableType.Shrine;
			subType = 4;
			return true;
		}
		if (((Object)((Object)(object)go.GetComponent<InteractableShrineMoai>())))
		{
			type = InteractableType.Shrine;
			subType = 5;
			return true;
		}
		if ((Object)(object)go.GetComponent<InteractablePot>() != (Object)null)
		{
			type = InteractableType.Pot;
			return true;
		}
		if ((Object)(object)go.GetComponent<InteractablePortal>() != (Object)null)
		{
			type = InteractableType.Portal;
			return true;
		}
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SpawnInteractables), "SpawnChests")]
	public static bool OnSpawnChests()
	{
		return HandleSpawnPrefix("Chests");
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(SpawnInteractables), "SpawnChests")]
	public static void OnSpawnChestsPost()
	{
		HandleSpawnPostfix();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SpawnInteractables), "SpawnShrines")]
	public static bool OnSpawnShrines()
	{
		return HandleSpawnPrefix("Shrines");
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(SpawnInteractables), "SpawnShrines")]
	public static void OnSpawnShrinesPost()
	{
		HandleSpawnPostfix();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SpawnInteractables), "SpawnShit")]
	public static bool OnSpawnPots()
	{
		return HandleSpawnPrefix("Pots");
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(SpawnInteractables), "SpawnShit")]
	public static void OnSpawnPotsPost()
	{
		HandleSpawnPostfix();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SpawnInteractables), "SpawnOther")]
	public static bool OnSpawnOther()
	{
		return HandleSpawnPrefix("Other");
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(SpawnInteractables), "SpawnOther")]
	public static void OnSpawnOtherPost()
	{
		HandleSpawnPostfix();
	}

	private static bool HandleSpawnPrefix(string spawnType)
	{
		if (SteamNetworkLobbyManager.State != SteamNetworkLobbyState.Joined)
		{
			return true;
		}
		if (SteamNetworkManager.IsServer)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("[MP][Host] Spawning " + spawnType + " (Capturing...)");
			_isHostSpawning = true;
			return true;
		}
		// Client: allow native spawn since RNG seeds are synchronized
		// This ensures shrines, pots, portals are visible and interactable on client
		Melon<BonkWithFriendsMod>.Logger.Msg("[MP][Client] Spawning " + spawnType + " (native, seed-synced)");
		return true;
	}

	private static void HandleSpawnPostfix()
	{
		if (_isHostSpawning)
		{
			_isHostSpawning = false;
			BroadcastSpawnData();
		}
	}

	private static void BroadcastSpawnData()
	{
		if (_pendingSpawns.Count != 0)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg($"[MP][Host] Broadcasting {_pendingSpawns.Count} items");
			InteractableSpawnBatchMessage tMsg = new InteractableSpawnBatchMessage
			{
				Spawns = new List<InteractableSpawnData>(_pendingSpawns)
			};
			SteamNetworkServer.Instance?.BroadcastToRemoteClients(tMsg);
			_pendingSpawns.Clear();
		}
	}

	internal static void HandleInteractableSpawnBatch(InteractableSpawnBatchMessage msg)
	{
		if (msg.Spawns == null)
		{
			return;
		}
		SpawnInteractables spawnerInstance = GetSpawnerInstance();
		if ((Object)(object)spawnerInstance == (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[MP][Client] Cannot spawn items: SpawnInteractables instance not found!");
			return;
		}
		foreach (InteractableSpawnData spawn in msg.Spawns)
		{
			SpawnClientInteractable(spawn, spawnerInstance);
		}
	}

	private static void SpawnClientInteractable(InteractableSpawnData data, SpawnInteractables spawner)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		GameObject prefabForType = GetPrefabForType(data.Type, data.SubType, spawner);
		if (!((Object)((Object)(object)prefabForType)))
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[MP][Client] No prefab for {data.Type}:{data.SubType}");
		}
		else
		{
			GameObject val = Object.Instantiate<GameObject>(prefabForType, data.Position, data.Rotation);
			if (((Object)((Object)(object)val)))
			{
				_idToObj[data.Id] = val;
				_objToId[val] = data.Id;
			}
		}
	}

	private static GameObject GetPrefabForType(InteractableType type, int subType, SpawnInteractables spawner)
	{
		return (GameObject)(type switch
		{
			InteractableType.Chest => spawner.chest, 
			InteractableType.ChestFree => spawner.chestFree, 
			InteractableType.Shrine => null, 
			InteractableType.Pot => null, 
			InteractableType.Portal => null, 
			_ => null, 
		});
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(InteractableChest), "Interact")]
	public static void OnChestInteract(InteractableChest __instance)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined && _objToId.TryGetValue(((Component)__instance).gameObject, out var value))
		{
			InteractableUsedMessage tMsg = new InteractableUsedMessage
			{
				PlayerSteamId = SteamUser.GetSteamID().m_SteamID,
				InteractableId = value
			};
			if (SteamNetworkManager.IsMultiplayer)
			{
				SteamNetworkClient.Instance?.SendMessage(tMsg);
			}
		}
	}

	internal static void HandleInteractableUsed(InteractableUsedMessage msg)
	{
		if (_idToObj.TryGetValue(msg.InteractableId, out var value))
		{
			_idToObj.Remove(msg.InteractableId);
			if ((Object)(object)value != (Object)null)
			{
				_objToId.Remove(value);
				value.SetActive(false);
			}
		}
	}
}
