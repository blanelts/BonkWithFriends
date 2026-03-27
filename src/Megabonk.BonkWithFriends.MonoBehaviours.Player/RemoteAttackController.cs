using System;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Weapons;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Weapons.Attacks;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Weapons.Projectiles;
using Il2CppAssets.Scripts.Objects.Pooling;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.MonoBehaviours.Player;

[RegisterTypeInIl2Cpp]
public class RemoteAttackController : MonoBehaviour
{
	public class RemoteAttackData
	{
		public ulong PlayerId;

		public Vector3 Position;

		public Quaternion Rotation;

		public float Size;
	}

	public class AttackState
	{
		public uint AttackId;

		public EWeapon WeaponType;

		public int ProjectileCount;

		public float BurstInterval;

		public float ProjectileSize;

		public Vector3 Position;

		public Quaternion Rotation;

		public WeaponAttack Attack;

		public WeaponBase WeaponBase;

		public int Spawned;

		public float NextSpawn;
	}

	private static readonly Dictionary<int, RemoteAttackData> _remoteAttacks = new Dictionary<int, RemoteAttackData>();

	private static readonly HashSet<int> _remoteProjectiles = new HashSet<int>();

	private static readonly Dictionary<int, ulong> _projectileOwners = new Dictionary<int, ulong>();

	private static readonly object _lock = new object();

	private ulong _playerId;

	private readonly Dictionary<uint, AttackState> _attacks = new Dictionary<uint, AttackState>();

	public static bool TryGetRemoteAttackData(WeaponAttack attack, out RemoteAttackData data)
	{
		data = null;
		if ((Object)(object)attack == (Object)null)
		{
			return false;
		}
		lock (_lock)
		{
			return _remoteAttacks.TryGetValue(((Object)attack).GetInstanceID(), out data);
		}
	}

	public static void MarkProjectileAsRemote(ProjectileBase projectile, ulong playerId)
	{
		if ((Object)(object)projectile == (Object)null)
		{
			return;
		}
		lock (_lock)
		{
			int instanceID = ((Object)projectile).GetInstanceID();
			_remoteProjectiles.Add(instanceID);
			_projectileOwners[instanceID] = playerId;
		}
	}

	public static bool IsRemoteProjectile(ProjectileBase projectile)
	{
		if ((Object)(object)projectile == (Object)null)
		{
			return false;
		}
		lock (_lock)
		{
			return _remoteProjectiles.Contains(((Object)projectile).GetInstanceID());
		}
	}

	public static bool TryGetProjectileOwner(ProjectileBase projectile, out ulong playerId)
	{
		playerId = 0uL;
		if ((Object)(object)projectile == (Object)null)
		{
			return false;
		}
		lock (_lock)
		{
			return _projectileOwners.TryGetValue(((Object)projectile).GetInstanceID(), out playerId);
		}
	}

	public static void CleanupRemoteProjectile(ProjectileBase projectile)
	{
		if ((Object)(object)projectile == (Object)null)
		{
			return;
		}
		lock (_lock)
		{
			int instanceID = ((Object)projectile).GetInstanceID();
			_remoteProjectiles.Remove(instanceID);
			_projectileOwners.Remove(instanceID);
		}
	}

	private void Awake()
	{
		NetworkedPlayer componentInParent = ((Component)this).GetComponentInParent<NetworkedPlayer>();
		if ((Object)(object)componentInParent != (Object)null)
		{
			_playerId = componentInParent.SteamId;
			Melon<BonkWithFriendsMod>.Logger.Msg($"[RemoteAttackController] Initialized for player {_playerId}");
		}
		else
		{
			Melon<BonkWithFriendsMod>.Logger.Warning("[RemoteAttackController] No NetworkedPlayer parent found!");
		}
	}

	private void OnDestroy()
	{
		ClearState();
	}

	private void Update()
	{
		if (((Behaviour)this).enabled && !((Object)(object)PoolManager.Instance == (Object)null))
		{
			AlwaysManager instance = AlwaysManager.Instance;
			if (!((Object)(object)((instance != null) ? instance.dataManager : null) == (Object)null))
			{
				ProcessAttacks();
			}
		}
	}

	public void OnAttackReceived(uint attackId, int weaponType, int projectileCount, float burstInterval, float projectileSize, Vector3 position, Quaternion rotation)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (!_attacks.ContainsKey(attackId))
		{
			_attacks[attackId] = new AttackState
			{
				AttackId = attackId,
				WeaponType = (EWeapon)weaponType,
				ProjectileCount = projectileCount,
				BurstInterval = burstInterval,
				ProjectileSize = projectileSize,
				Position = position,
				Rotation = rotation,
				Spawned = 0,
				NextSpawn = Time.time
			};
			Melon<BonkWithFriendsMod>.Logger.Msg($"[RemoteAttack] Received attack {attackId}: {weaponType} x{projectileCount} at {position}");
		}
	}

	public void UpdateAttackPosition(uint attackId, Vector3 position, Quaternion rotation)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (!_attacks.TryGetValue(attackId, out var value))
		{
			return;
		}
		value.Position = position;
		value.Rotation = rotation;
		if (!((Object)(object)value.Attack != (Object)null))
		{
			return;
		}
		lock (_lock)
		{
			if (_remoteAttacks.TryGetValue(((Object)value.Attack).GetInstanceID(), out var value2))
			{
				value2.Position = position;
				value2.Rotation = rotation;
			}
		}
	}

	public void ClearState()
	{
		foreach (KeyValuePair<uint, AttackState> attack in _attacks)
		{
			UnregisterAttack(attack.Value);
		}
		_attacks.Clear();
	}

	public void ClearAllAttacks()
	{
		lock (_lock)
		{
			foreach (KeyValuePair<uint, AttackState> attack in _attacks)
			{
				if ((Object)(object)attack.Value.Attack != (Object)null)
				{
					attack.Value.Attack.attackDone = true;
					_remoteAttacks.Remove(((Object)attack.Value.Attack).GetInstanceID());
				}
			}
			_attacks.Clear();
			var toRemove = new List<int>();
			foreach (var kvp in _projectileOwners)
			{
				if (kvp.Value == _playerId)
					toRemove.Add(kvp.Key);
			}
			foreach (var key in toRemove)
			{
				_projectileOwners.Remove(key);
				_remoteProjectiles.Remove(key);
			}
		}
		Melon<BonkWithFriendsMod>.Logger.Msg($"[RemoteAttack] Cleared all attacks for player {_playerId}");
	}

	private void ProcessAttacks()
	{
		List<uint> list = new List<uint>();
		float time = Time.time;
		foreach (KeyValuePair<uint, AttackState> attack in _attacks)
		{
			AttackState value = attack.Value;
			try
			{
				if ((Object)(object)value.Attack == (Object)null && !InitializeAttack(value))
				{
					list.Add(attack.Key);
				}
				else if (value.Spawned < value.ProjectileCount)
				{
					if (time >= value.NextSpawn)
					{
						SpawnProjectile(value);
						value.NextSpawn = time + value.BurstInterval;
					}
				}
				else
				{
					value.Attack.attackDone = true;
					list.Add(attack.Key);
				}
			}
			catch (Exception ex)
			{
				Melon<BonkWithFriendsMod>.Logger.Error("[RemoteAttack] Error: " + ex.Message);
				list.Add(attack.Key);
			}
		}
		foreach (uint item in list)
		{
			if (_attacks.TryGetValue(item, out var value2))
			{
				UnregisterAttack(value2);
				_attacks.Remove(item);
			}
		}
	}

	private bool InitializeAttack(AttackState state)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		WeaponData weapon = AlwaysManager.Instance.dataManager.GetWeapon(state.WeaponType);
		if ((Object)(object)weapon == (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Error($"[RemoteAttack] Unknown weapon: {state.WeaponType}");
			return false;
		}
		state.WeaponBase = new WeaponBase(weapon);
		state.Attack = PoolManager.Instance.GetAttack(state.WeaponBase);
		if ((Object)(object)state.Attack == (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning("[RemoteAttack] GetAttack returned null");
			return false;
		}
		RegisterAttack(state);
		state.Attack.weaponBase = state.WeaponBase;
		state.Attack.attackDone = false;
		((Component)state.Attack).transform.SetPositionAndRotation(state.Position, state.Rotation);
		if ((Object)(object)state.Attack.muzzle != (Object)null)
		{
			state.Attack.muzzle.Set(state.ProjectileCount, state.BurstInterval);
		}
		Melon<BonkWithFriendsMod>.Logger.Msg($"[RemoteAttack] Initialized: ID={state.AttackId}, InstanceID={((Object)state.Attack).GetInstanceID()}");
		return true;
	}

	private void SpawnProjectile(AttackState state)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		lock (_lock)
		{
			if (_remoteAttacks.TryGetValue(((Object)state.Attack).GetInstanceID(), out var value))
			{
				value.Position = state.Position;
				value.Rotation = state.Rotation;
				value.Size = state.ProjectileSize;
			}
		}
		state.Attack.SpawnProjectile(state.Spawned);
		state.Spawned++;
		Melon<BonkWithFriendsMod>.Logger.Msg($"[RemoteAttack] Spawned {state.Spawned}/{state.ProjectileCount}");
	}

	private void RegisterAttack(AttackState state)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		int instanceID = ((Object)state.Attack).GetInstanceID();
		RemoteAttackData value = new RemoteAttackData
		{
			PlayerId = _playerId,
			Position = state.Position,
			Rotation = state.Rotation,
			Size = state.ProjectileSize
		};
		lock (_lock)
		{
			_remoteAttacks[instanceID] = value;
		}
	}

	private void UnregisterAttack(AttackState state)
	{
		if ((Object)(object)state.Attack == (Object)null)
		{
			return;
		}
		lock (_lock)
		{
			_remoteAttacks.Remove(((Object)state.Attack).GetInstanceID());
		}
	}
}
