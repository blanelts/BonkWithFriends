using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Actors.Player;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Weapons;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Weapons.Attacks;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Weapons.Projectiles;
using Il2CppAssets.Scripts.Objects.Pooling;

using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.MonoBehaviours.Player;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.HarmonyPatches.Combat;

[HarmonyPatch]
public static class WeaponSyncPatches
{
	internal static Dictionary<WeaponAttack, uint> _attackIds = new Dictionary<WeaponAttack, uint>();

	private static uint _nextAttackId = 1u;

	private static Dictionary<WeaponAttack, int> _lastProjectileIndex = new Dictionary<WeaponAttack, int>();

	public static void OnSetAttack(WeaponAttack __instance, WeaponBase weaponBase, MyPlayer player)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.None)
		{
			return;
		}
		var logger = Melon<BonkWithFriendsMod>.Logger;
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 2);
		defaultInterpolatedStringHandler.AppendLiteral("[WeaponSync] SetAttack called! InLobby=");
		defaultInterpolatedStringHandler.AppendFormatted(SteamNetworkLobbyManager.State);
		defaultInterpolatedStringHandler.AppendLiteral(", Weapon=");
		EWeapon? value;
		if (weaponBase == null)
		{
			value = null;
		}
		else
		{
			WeaponData weaponData = weaponBase.weaponData;
			value = ((weaponData != null) ? new EWeapon?(weaponData.eWeapon) : ((EWeapon?)null));
		}
		defaultInterpolatedStringHandler.AppendFormatted(value);
		logger.Msg(defaultInterpolatedStringHandler.ToStringAndClear());
		if (SteamNetworkLobbyManager.State != SteamNetworkLobbyState.Joined)
		{
			return;
		}
		if (LocalPlayerManager.LocalPlayerState.IsDead)
		{
			return;
		}
		if (weaponBase == null || !((Object)((Object)(object)player)))
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[WeaponSync] Null params: weapon={weaponBase != null}, player={(Object)(object)player != (Object)null}");
			return;
		}
		try
		{
			uint value2 = _nextAttackId++;
			_attackIds[__instance] = value2;
			Vector3 projectilePosition = __instance.GetProjectilePosition();
			Quaternion projectileRotation = __instance.GetProjectileRotation();
			if (((Object)((Object)(object)LocalPlayerManager.LocalPlayer)))
			{
				LocalPlayerManager.SendAttackStarted(weaponBase, projectilePosition, projectileRotation);
			}
			Melon<BonkWithFriendsMod>.Logger.Msg($"[WeaponSync] SetAttack hooked: AttackID={value2}, Weapon={weaponBase.weaponData.eWeapon}");
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[WeaponSync] Error in OnSetAttack: " + ex.Message);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(WeaponAttack), "SpawnProjectile")]
	public static void OnSpawnProjectile(WeaponAttack __instance, int projectileIndex)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkLobbyManager.State != SteamNetworkLobbyState.Joined || RemoteAttackController.TryGetRemoteAttackData(__instance, out var _))
		{
			return;
		}
		if ((Object)(object)__instance.player == (Object)null || (Object)(object)__instance.player != (Object)(object)MyPlayer.Instance)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[WeaponSync] OnSpawnProjectile was called for a local player attack, but the player field is null! This should not happen.");
			return;
		}
		if (LocalPlayerManager.LocalPlayerState.IsDead)
		{
			return;
		}
		uint value;
		if (projectileIndex == 0)
		{
			value = _nextAttackId++;
			_attackIds[__instance] = value;
			try
			{
				Vector3 position = ((Component)__instance).transform.position;
				Quaternion rotation = ((Component)__instance).transform.rotation;
				if (((Object)((Object)(object)LocalPlayerManager.LocalPlayer)))
				{
					LocalPlayerManager.SendAttackStarted(__instance.weaponBase, position, rotation);
				}
				var logger = Melon<BonkWithFriendsMod>.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[WeaponSync] New attack from SpawnProjectile: AttackID=");
				defaultInterpolatedStringHandler.AppendFormatted(value);
				defaultInterpolatedStringHandler.AppendLiteral(", Weapon=");
				WeaponBase weaponBase = __instance.weaponBase;
				EWeapon? value2;
				if (weaponBase == null)
				{
					value2 = null;
				}
				else
				{
					WeaponData weaponData = weaponBase.weaponData;
					value2 = ((weaponData != null) ? new EWeapon?(weaponData.eWeapon) : ((EWeapon?)null));
				}
				defaultInterpolatedStringHandler.AppendFormatted(value2);
				logger.Msg(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			catch (Exception ex)
			{
				Melon<BonkWithFriendsMod>.Logger.Error("[WeaponSync] Error creating attack: " + ex.Message);
				return;
			}
		}
		else if (!_attackIds.TryGetValue(__instance, out value))
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[WeaponSync] Projectile index {projectileIndex} but no attack ID found, skipping");
			return;
		}
		try
		{
			Vector3 position2 = ((Component)__instance).transform.position;
			Quaternion rotation2 = ((Component)__instance).transform.rotation;
			if (((Object)((Object)(object)LocalPlayerManager.LocalPlayer)))
			{
				LocalPlayerManager.SendProjectileSpawned(value, projectileIndex, position2, rotation2);
			}
			Melon<BonkWithFriendsMod>.Logger.Msg($"[WeaponSync] SpawnProjectile hooked: AttackID={value}, Index={projectileIndex}");
		}
		catch (Exception ex2)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[WeaponSync] Error in OnSpawnProjectile: " + ex2.Message);
		}
	}

	public static void OnAttackTimeout(WeaponAttack __instance)
	{
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.None || !_attackIds.TryGetValue(__instance, out var _))
		{
			return;
		}
		try
		{
			_attackIds.Remove(__instance);
			_lastProjectileIndex.Remove(__instance);
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[WeaponSync] Error in OnAttackTimeout: " + ex.Message);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(WeaponAttack), "SetAttack")]
	public static bool SetAttack_Prefix(WeaponAttack __instance, WeaponBase weaponBase, MyPlayer player)
	{
		if (SteamNetworkLobbyManager.State != SteamNetworkLobbyState.Joined)
			return true;
		if (LocalPlayerManager.LocalPlayerState.IsDead)
			return false; // Block attack when dead
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(WeaponAttack), "SpawnProjectile")]
	public static bool SpawnProjectile_Prefix(WeaponAttack __instance, int projectileIndex)
	{
		if (SteamNetworkLobbyManager.State != SteamNetworkLobbyState.Joined)
		{
			return true;
		}
		if (!RemoteAttackController.TryGetRemoteAttackData(__instance, out var data))
		{
			// Local player attack — block if dead
			if (LocalPlayerManager.LocalPlayerState.IsDead)
				return false;
			return true;
		}
		try
		{
			SpawnRemoteProjectile(__instance, projectileIndex, data);
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[RemoteAttackPatch] SpawnProjectile error: " + ex.Message + "\n" + ex.StackTrace);
		}
		return false;
	}

	private static void SpawnRemoteProjectile(WeaponAttack attack, int projectileIndex, RemoteAttackController.RemoteAttackData data)
	{
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		WeaponBase weaponBase = attack.weaponBase;
		if ((Object)(object)((weaponBase != null) ? weaponBase.weaponData : null) == (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning("[RemoteAttackPatch] No weaponBase/data");
			return;
		}
		PoolManager instance = PoolManager.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning("[RemoteAttackPatch] No PoolManager");
			return;
		}
		ProjectileBase projectile = instance.GetProjectile(attack);
		if ((Object)(object)projectile == (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning("[RemoteAttackPatch] GetProjectile returned null");
			return;
		}
		GameObject gameObject = ((Component)projectile).gameObject;
		if ((Object)(object)gameObject == (Object)null)
		{
			return;
		}
		gameObject.SetActive(true);
		Transform transform = ((Component)projectile).transform;
		if ((Object)(object)transform == (Object)null)
		{
			return;
		}
		GameObject prefabProjectile = attack.prefabProjectile;
		if ((Object)(object)prefabProjectile != (Object)null)
		{
			Transform transform2 = prefabProjectile.transform;
			if ((Object)(object)transform2 != (Object)null)
			{
				Vector3 localScale = transform2.localScale * data.Size;
				transform.localScale = localScale;
			}
		}
		transform.rotation = data.Rotation;
		float y = weaponBase.weaponData.spawnOffset.y;
		Vector3 value = (transform.position = data.Position + Vector3.up * y);
		projectile.Set(weaponBase, attack, projectileIndex);
		RemoteAttackController.MarkProjectileAsRemote(projectile, data.PlayerId);
		try
		{
			Il2CppSystem.Action a_SpawnedProjectile = attack.A_SpawnedProjectile;
			if (a_SpawnedProjectile != null)
			{
				a_SpawnedProjectile.Invoke();
			}
		}
		catch
		{
		}
		Melon<BonkWithFriendsMod>.Logger.Msg($"[RemoteAttackPatch] Spawned projectile {projectileIndex} at {value}");
	}
}
