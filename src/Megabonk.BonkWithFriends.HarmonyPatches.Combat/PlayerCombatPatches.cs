using System;
using HarmonyLib;
using Il2CppAssets.Scripts.Inventory__Items__Pickups;
using Megabonk.BonkWithFriends.Networking.Messages.Client;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace Megabonk.BonkWithFriends.HarmonyPatches.Combat;

[HarmonyPatch]
public static class PlayerCombatPatches
{
	[HarmonyPatch(typeof(PlayerHealth), "Heal")]
	public static class Patch_PlayerHealth_Heal
	{
		[HarmonyPostfix]
		private static void Postfix(PlayerHealth __instance, float amount, int __result)
		{
			try
			{
				if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined && __result > 0)
				{
					Melon<BonkWithFriendsMod>.Logger.Msg($"[MP] Player healed: {__result}");
					if (SteamNetworkManager.IsMultiplayer)
					{
						SteamNetworkClient.Instance?.SendMessage(new PlayerHealedMessage
						{
							HealAmount = __result,
							Hp = __instance.hp,
							MaxHp = __instance.maxHp
						});
					}
				}
			}
			catch (Exception ex)
			{
				Melon<BonkWithFriendsMod>.Logger.Error("Error in ChestOpenPatch: " + ex.Message);
				Melon<BonkWithFriendsMod>.Logger.Error("Stack trace: " + ex.StackTrace);
			}
		}
	}

	private static int _lastKnownLevel = 1;

	public static void OnPlayerDamaged(PlayerHealth __instance, float damage, Vector3 direction, string damageSource)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined)
		{
			SteamUser.GetSteamID();
			Melon<BonkWithFriendsMod>.Logger.Msg($"[MP] Player damaged: {damage} from {damageSource}");
			if (SteamNetworkManager.IsMultiplayer)
			{
				SteamNetworkClient.Instance?.SendMessage(new PlayerDamagedMessage
				{
					Damage = damage,
					Hp = __instance.hp,
					MaxHp = __instance.maxHp,
					Shield = __instance.shield,
					MaxShield = __instance.maxShield
				});
			}
		}
	}

	public static void OnPlayerDied(PlayerHealth __instance)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined)
		{
			ulong steamID = SteamUser.GetSteamID().m_SteamID;
			Melon<BonkWithFriendsMod>.Logger.Msg("[MP] Player died!");
			if (SteamNetworkManager.IsMultiplayer)
			{
				SteamNetworkClient.Instance?.SendMessage(new PlayerDiedMessage
				{
					SteamUserId = steamID
				});
			}
		}
	}
}
