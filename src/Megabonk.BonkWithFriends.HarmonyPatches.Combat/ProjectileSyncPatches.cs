using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Weapons;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Weapons.Projectiles;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.MonoBehaviours.Player;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace Megabonk.BonkWithFriends.HarmonyPatches.Combat;

[HarmonyPatch]
public static class ProjectileSyncPatches
{
	private static readonly Dictionary<ProjectileBase, int> _projectileIndexes = new Dictionary<ProjectileBase, int>();

	public static int GetProjectileIndex(ProjectileBase projectile)
	{
		if ((Object)(object)projectile != (Object)null && _projectileIndexes.TryGetValue(projectile, out var value))
		{
			return value;
		}
		return -1;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ProjectileBase), "Set")]
	public static void StoreProjectileIndex(ProjectileBase __instance, int projectileIndex)
	{
		if (SteamNetworkLobbyManager.State != SteamNetworkLobbyState.None)
		{
			_projectileIndexes[__instance] = projectileIndex;
			Melon<BonkWithFriendsMod>.Logger.Msg($"[ProjectileSync] Stored projectile index {projectileIndex} for instance {((Object)__instance).GetInstanceID()}");
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ProjectileMelee), "MyUpdate")]
	public static bool ProjectileMelee_MyUpdate_Prefix(ProjectileMelee __instance)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (!RemoteAttackController.TryGetProjectileOwner((ProjectileBase)(object)__instance, out var playerId))
		{
			return true;
		}
		NetworkedPlayer player = RemotePlayerManager.GetPlayer(new CSteamID(playerId));
		if (!((Object)((Object)(object)player)))
		{
			Object.Destroy((Object)(object)((Component)__instance).gameObject);
			RemoteAttackController.CleanupRemoteProjectile((ProjectileBase)(object)__instance);
			return false;
		}
		if (player.State.IsDead)
		{
			Object.Destroy((Object)(object)((Component)__instance).gameObject);
			RemoteAttackController.CleanupRemoteProjectile((ProjectileBase)(object)__instance);
			return false;
		}
		Transform transform = ((Component)player).transform;
		Vector3 val = transform.position + transform.forward * __instance.forwardOffset;
		WeaponBase weaponBase = ((ProjectileBase)__instance).weaponBase;
		if (((Object)((Object)(object)((weaponBase != null) ? weaponBase.weaponData : null))))
		{
			val += Vector3.up * ((ProjectileBase)__instance).weaponBase.weaponData.spawnOffset.y;
		}
		((Component)__instance).transform.position = val;
		((Component)__instance).transform.rotation = transform.rotation;
		Melon<BonkWithFriendsMod>.Logger.Msg($"[ProjectileMelee] projectile position {((Component)__instance).transform.position} and rotation {((Component)__instance).transform.rotation}");
		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ProjectileBase), "ProjectileDone")]
	public static void CleanupProjectileIndex(ProjectileBase __instance)
	{
		if (_projectileIndexes.Remove(__instance))
		{
			Melon<BonkWithFriendsMod>.Logger.Msg($"[ProjectileSync] Cleaned up projectile index for instance {((Object)__instance).GetInstanceID()}");
		}
	}
}
