using Il2CppAssets.Scripts.Actors.Enemies;
using Megabonk.BonkWithFriends.HarmonyPatches.Enemies;
using Megabonk.BonkWithFriends.Managers.Enemies;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.MonoBehaviours.Player;
using Megabonk.BonkWithFriends.Networking.Messages;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Networking.Handlers;

public static class PlayerAttackSystem
{
	[NetworkMessageHandler(MessageType.WeaponAttackStartedRelay)]
	private static void RouteWeaponAttackStarted(SteamNetworkMessage message)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		WeaponAttackStartedRelayMessage weaponAttackStartedRelayMessage = message.Deserialize<WeaponAttackStartedRelayMessage>();
		if (SteamNetworkManager.IsServer)
		{
			SteamNetworkServer.Instance?.BroadcastMessageExcept(weaponAttackStartedRelayMessage, message.SteamUserId);
		}
		NetworkedPlayer player = RemotePlayerManager.GetPlayer(message.SteamUserId);
		if (!((Object)((Object)(object)player)))
		{
			Melon<BonkWithFriendsMod>.Logger.Error($"[PlayerAttackSystem] RemotePlayerManager.GetPlayer returned null for Steam ID: {message.SteamUserId.m_SteamID}");
			return;
		}
		RemoteAttackController componentInChildren = ((Component)player).GetComponentInChildren<RemoteAttackController>();
		if (!((Object)((Object)(object)componentInChildren)))
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[PlayerAttackSystem] No RemoteAttackController found on player " + ((Object)((Component)player).gameObject).name);
			return;
		}
		componentInChildren.OnAttackReceived(weaponAttackStartedRelayMessage.AttackId, weaponAttackStartedRelayMessage.WeaponType, weaponAttackStartedRelayMessage.ProjectileCount, weaponAttackStartedRelayMessage.BurstInterval, weaponAttackStartedRelayMessage.ProjectileSize, weaponAttackStartedRelayMessage.SpawnPosition, weaponAttackStartedRelayMessage.SpawnRotation);
		Melon<BonkWithFriendsMod>.Logger.Msg($"[PlayerAttackSystem] Remote attack started: ID={weaponAttackStartedRelayMessage.AttackId}, Weapon={weaponAttackStartedRelayMessage.WeaponType}, Position={weaponAttackStartedRelayMessage.SpawnPosition}");
	}

	[NetworkMessageHandler(MessageType.WeaponAttackStarted)]
	private static void HandleWeaponAttackStarted(SteamNetworkMessage message)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		WeaponAttackStartedMessage weaponAttackStartedMessage = message.Deserialize<WeaponAttackStartedMessage>();
		NetworkedPlayer player = RemotePlayerManager.GetPlayer(message.SteamUserId);
		if (!((Object)((Object)(object)player)))
		{
			Melon<BonkWithFriendsMod>.Logger.Error($"[PlayerAttackSystem] RemotePlayerManager.GetPlayer returned null for Steam ID: {message.SteamUserId.m_SteamID}");
			return;
		}
		RemoteAttackController componentInChildren = ((Component)player).GetComponentInChildren<RemoteAttackController>();
		if (!((Object)((Object)(object)componentInChildren)))
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[PlayerAttackSystem] No RemoteAttackController found on player " + ((Object)((Component)player).gameObject).name);
			return;
		}
		componentInChildren.OnAttackReceived(weaponAttackStartedMessage.AttackId, weaponAttackStartedMessage.WeaponType, weaponAttackStartedMessage.ProjectileCount, weaponAttackStartedMessage.BurstInterval, weaponAttackStartedMessage.ProjectileSize, weaponAttackStartedMessage.SpawnPosition, weaponAttackStartedMessage.SpawnRotation);
		Melon<BonkWithFriendsMod>.Logger.Msg($"[PlayerAttackSystem] Remote attack started: ID={weaponAttackStartedMessage.AttackId}, Weapon={weaponAttackStartedMessage.WeaponType}, Position={weaponAttackStartedMessage.SpawnPosition}");
	}

	[NetworkMessageHandler(MessageType.WeaponProjectileSpawned)]
	private static void HandleWeaponProjectileSpawned(SteamNetworkMessage message)
	{
		if (SteamNetworkManager.IsServer)
		{
			WeaponProjectileSpawnedMessage weaponProjectileSpawnedMessage = message.Deserialize<WeaponProjectileSpawnedMessage>();
			SteamNetworkServer.Instance?.BroadcastMessageExcept(weaponProjectileSpawnedMessage, message.SteamUserId);
		}
	}

	[NetworkMessageHandler(MessageType.WeaponProjectileHit)]
	private static void HandleWeaponProjectileHit(SteamNetworkMessage message)
	{
		WeaponProjectileHitMessage weaponProjectileHitMessage = message.Deserialize<WeaponProjectileHitMessage>();
		if (SteamNetworkManager.IsServer)
		{
			Enemy enemy = HostEnemyManager.GetTrackedEnemy(weaponProjectileHitMessage.TargetId);
			if (((Object)((Object)(object)enemy)) && enemy.hp > 0f)
			{
				enemy.hp -= weaponProjectileHitMessage.Damage;
				Melon<BonkWithFriendsMod>.Logger.Msg($"[Host] Client hit enemy {weaponProjectileHitMessage.TargetId} for {weaponProjectileHitMessage.Damage:F1} dmg, HP now {enemy.hp:F1}");
				SteamNetworkServer.Instance?.BroadcastToRemoteClients(new EnemyDamagedMessage
				{
					EnemyId = weaponProjectileHitMessage.TargetId,
					HpNow = enemy.hp,
					DamageForFx = weaponProjectileHitMessage.Damage
				});
				if (enemy.hp <= 0f)
				{
					EnemyPatches.SetProcessingNetworkDeath(true);
					try { enemy.EnemyDied(); }
					finally { EnemyPatches.SetProcessingNetworkDeath(false); }
				}
			}
			SteamNetworkServer.Instance?.BroadcastMessageExcept(weaponProjectileHitMessage, message.SteamUserId);
		}
	}
}
