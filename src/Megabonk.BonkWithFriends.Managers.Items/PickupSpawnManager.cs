using System;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Pickups;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Managers.Items;

public static class PickupSpawnManager
{
	private struct PendingPickupSpawn
	{
		public int PickupId;

		public int EPickup;

		public Vector3 Position;

		public int Value;
	}

	[ThreadStatic]
	public static bool IsSpawningFromNetwork;

	private static Dictionary<int, Pickup> _pickupRegistry = new Dictionary<int, Pickup>();

	private static int _nextPickupId = 0;

	private static readonly Queue<PendingPickupSpawn> _pendingSpawns = new Queue<PendingPickupSpawn>();

	private static readonly Queue<int> _pendingDespawns = new Queue<int>();

	public static int RegisterPickup(Pickup pickup)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)((Object)(object)pickup)))
		{
			return -1;
		}
		int num = _nextPickupId++;
		_pickupRegistry[num] = pickup;
		MelonLogger.Msg($"[PickupSpawnManager] Registered pickup ID: {num}, Type: {pickup.ePickup}");
		return num;
	}

	public static void RegisterPickupWithId(int pickupId, Pickup pickup)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (((Object)((Object)(object)pickup)))
		{
			_pickupRegistry[pickupId] = pickup;
			if (pickupId >= _nextPickupId)
			{
				_nextPickupId = pickupId + 1;
			}
			MelonLogger.Msg($"[PickupSpawnManager] Registered pickup with ID: {pickupId}, Type: {pickup.ePickup}");
		}
	}

	public static Pickup GetPickup(int pickupId)
	{
		_pickupRegistry.TryGetValue(pickupId, out var value);
		return value;
	}

	public static int GetPickupId(Pickup pickup)
	{
		foreach (KeyValuePair<int, Pickup> item in _pickupRegistry)
		{
			if ((Object)(object)item.Value == (Object)(object)pickup)
			{
				return item.Key;
			}
		}
		return -1;
	}

	private static void UnregisterPickup(int id)
	{
		if (_pickupRegistry.TryGetValue(id, out var _))
		{
			_pickupRegistry.Remove(id);
		}
	}

	public static void BroadcastPickupSpawned(Pickup pickup, int ePickup)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (!SteamNetworkManager.IsServer || !((Object)((Object)(object)pickup)))
		{
			return;
		}
		try
		{
			EPickup value = (EPickup)ePickup;
			Vector3 position = ((Component)pickup).transform.position;
			int value2 = pickup.GetValue();
			int num = RegisterPickup(pickup);
			Melon<BonkWithFriendsMod>.Logger.Msg($"[PickupSpawnManager] Host spawned pickup: ID={num}, Type={value}, Value={value2}");
			SteamNetworkServer.Instance?.BroadcastToRemoteClients(new PickupSpawnedMessage
			{
				PickupId = num,
				EPickup = ePickup,
				Position = position,
				Value = value2
			});
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[PickupSpawnManager] Failed to broadcast pickup spawn: " + ex.Message);
		}
	}

	public static void ProcessPickupCollection(int pickupId)
	{
		if (_pickupRegistry.TryGetValue(pickupId, out var value))
		{
			if (!((Object)((Object)(object)value)))
			{
				UnregisterPickup(pickupId);
				return;
			}
			value.pickedUp = true;
			if (((Object)((Object)(object)PickupManager.Instance)))
			{
				PickupManager.Instance.DespawnPickup(value);
			}
			else
			{
				Object.Destroy((Object)(object)((Component)value).gameObject);
			}
			UnregisterPickup(pickupId);
		}
		else
		{
			MelonLogger.Warning($"[MP] Could not find pickup ID {pickupId} in registry for despawn!");
		}
	}

	internal static void HandlePickupSpawn(PickupSpawnedMessage msg)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			PendingPickupSpawn item = new PendingPickupSpawn
			{
				PickupId = msg.PickupId,
				EPickup = msg.EPickup,
				Position = msg.Position,
				Value = msg.Value
			};
			_pendingSpawns.Enqueue(item);
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[PickupSpawnManager] Failed to queue pickup spawn: " + ex.Message);
		}
	}

	public static void ProcessPendingSpawns()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkManager.IsServer || _pendingSpawns.Count == 0 || !((Object)((Object)(object)PickupManager.Instance)))
		{
			return;
		}
		while (_pendingSpawns.Count > 0)
		{
			PendingPickupSpawn pendingPickupSpawn = _pendingSpawns.Dequeue();
			try
			{
				IsSpawningFromNetwork = true;
				Pickup val = PickupManager.Instance.SpawnPickup((EPickup)pendingPickupSpawn.EPickup, pendingPickupSpawn.Position, pendingPickupSpawn.Value, false, 0f);
				if (((Object)((Object)(object)val)))
				{
					RegisterPickupWithId(pendingPickupSpawn.PickupId, val);
				}
			}
			catch (Exception ex)
			{
				Melon<BonkWithFriendsMod>.Logger.Error("[PickupSpawnManager] Error processing pending pickup spawn: " + ex.Message);
			}
			finally
			{
				IsSpawningFromNetwork = false;
			}
		}
	}

	public static void ProcessPendingDespawns()
	{
		if ((Object)(object)PickupManager.Instance == (Object)null)
		{
			return;
		}
		while (_pendingDespawns.Count > 0)
		{
			int num = _pendingDespawns.Dequeue();
			Pickup pickup = GetPickup(num);
			if ((Object)(object)pickup != (Object)null)
			{
				UnregisterPickup(num);
				pickup.pickedUp = true;
				PickupManager.Instance.DespawnPickup(pickup);
			}
		}
	}

	public static void QueuePickupDespawn(int pickupId)
	{
		_pendingDespawns.Enqueue(pickupId);
	}

	public static void QueueRemotePickupDespawn(int pickupId)
	{
		QueuePickupDespawn(pickupId);
	}

	public static void ClearState()
	{
		_pickupRegistry.Clear();
		_pendingSpawns.Clear();
		_pendingDespawns.Clear();
		_nextPickupId = 0;
		Melon<BonkWithFriendsMod>.Logger.Msg("[PickupSpawnManager] State cleared.");
	}
}
