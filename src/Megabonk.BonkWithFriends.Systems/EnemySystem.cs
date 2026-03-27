using Il2CppAssets.Scripts.Actors.Enemies;
using Megabonk.BonkWithFriends.Managers.Enemies;
using Megabonk.BonkWithFriends.MonoBehaviours.Enemies;
using Megabonk.BonkWithFriends.Networking.Messages;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Systems;

public static class EnemySystem
{
	[NetworkMessageHandler(MessageType.EnemySpawned)]
	private static void HandleEnemySpawned(SteamNetworkMessage message)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		EnemySpawnedMessage enemySpawnedMessage = message.Deserialize<EnemySpawnedMessage>();
		RemoteEnemyManager.SpawnRemoteEnemy(enemySpawnedMessage.EnemyId, enemySpawnedMessage.EnemyType, enemySpawnedMessage.Position, enemySpawnedMessage.EulerAngles, enemySpawnedMessage.VelXZ, enemySpawnedMessage.MaxHp, (EEnemyFlag)enemySpawnedMessage.Flags, enemySpawnedMessage.extraSizeMultiplier);
	}

	[NetworkMessageHandler(MessageType.EnemyDied)]
	private static void HandleEnemyDied(SteamNetworkMessage message)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		EnemyDiedMessage enemyDiedMessage = message.Deserialize<EnemyDiedMessage>();
		if (SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer)
		{
			RemoteEnemyManager.RemoveEnemy(enemyDiedMessage.EnemyId, _hostkilled: true);
		}
		else if (!((Object)((Object)(object)HostEnemyManager.GetTrackedEnemy(enemyDiedMessage.EnemyId))))
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"Enemy {enemyDiedMessage.EnemyId} already dead");
		}
		else
		{
			HostEnemyManager.UnregisterHostEnemy(enemyDiedMessage.EnemyId, _clientKilled: true);
			SteamNetworkServer.Instance.BroadcastMessageExcept(enemyDiedMessage, message.SteamUserId);
		}
	}

	[NetworkMessageHandler(MessageType.EnemyStateBatch)]
	private static void HandleEnemyStateBatch(SteamNetworkMessage message)
	{
		foreach (EnemyStateBatchMessage.EnemyState state in message.Deserialize<EnemyStateBatchMessage>().States)
		{
			if (RemoteEnemyManager.HasEnemy(state.EnemyId))
			{
				RemoteEnemyManager.OnEnemyStateSnapshot(state.EnemyId, state.PosX, state.PosY, state.PosZ, state.YawQuantized, state.VelX, state.VelZ, state.AngVelQuantized, state.ServerTime, state.Seq);
			}
		}
	}

	[NetworkMessageHandler(MessageType.EnemyDamaged)]
	private static void HandleEnemyDamaged(SteamNetworkMessage message)
	{
		if (!SteamNetworkManager.IsClient || SteamNetworkManager.IsServer)
			return;

		EnemyDamagedMessage msg = message.Deserialize<EnemyDamagedMessage>();
		Enemy enemy = RemoteEnemyManager.GetEnemyRef(msg.EnemyId);
		if ((Object)(object)enemy == (Object)null)
			return;

		enemy.hp = msg.HpNow;

		EnemyHitFlash flash = ((Component)enemy).GetComponent<EnemyHitFlash>();
		if ((Object)(object)flash != (Object)null)
			flash.TriggerFlash();
	}
}
