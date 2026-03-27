using Il2CppAssets.Scripts.Actors.Enemies;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Managers.Enemies;

public static class EnemySpawnManager
{
	private static uint _nextEnemyNetworkId = 1u;

	public static uint GenerateNextEnemyNetworkId()
	{
		return _nextEnemyNetworkId++;
	}

	public static void BroadcastEnemySpawn(Enemy enemy)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected I4, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected I4, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkManager.IsServer && !((Object)(object)enemy == (Object)null))
		{
			uint num = ((enemy.id != 0) ? enemy.id : GenerateNextEnemyNetworkId());
			int num2 = (int)enemy.enemyData.enemyName;
			Vector3 position = ((Component)enemy).transform.position;
			Quaternion rotation = ((Component)enemy).transform.rotation;
			Vector3 eulerAngles = rotation.eulerAngles;
			float maxHp = enemy.maxHp;
			int flags = (int)enemy.enemyFlag;
			float extraSizeMultiplier = 1f;
			Vector2 zero = Vector2.zero;
			if (((Object)((Object)(object)enemy.enemyMovement)))
			{
				Vector3 baseVelocity = enemy.enemyMovement.baseVelocity;
				zero = new Vector2(baseVelocity.x, baseVelocity.z);
			}
			EnemySpawnedMessage tMsg = new EnemySpawnedMessage
			{
				EnemyId = num,
				EnemyType = num2,
				Position = position,
				EulerAngles = eulerAngles,
				VelXZ = zero,
				MaxHp = maxHp,
				Flags = flags,
				extraSizeMultiplier = extraSizeMultiplier
			};
			SteamNetworkServer.Instance?.BroadcastToRemoteClients(tMsg);
			Melon<BonkWithFriendsMod>.Logger.Msg($"[EnemySpawnManager] Broadcasted spawn for Enemy ID: {num}, Type: {num2}");
		}
	}

	public static void RegisterHostEnemy(Enemy enemy)
	{
		HostEnemyManager.RegisterHostEnemy(enemy);
	}

	public static void ProcessEnemyDeath(Enemy enemy)
	{
		if (((Object)((Object)(object)enemy)))
		{
			uint id = enemy.id;
			if (SteamNetworkManager.IsServer)
			{
				SteamNetworkServer.Instance?.BroadcastToRemoteClients(new EnemyDiedMessage
				{
					EnemyId = id
				});
				HostEnemyManager.UnregisterHostEnemy(id, _clientKilled: false);
			}
			else if (SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer)
			{
				SteamNetworkClient.Instance.SendMessage(new EnemyDiedMessage
				{
					EnemyId = id
				});
				RemoteEnemyManager.RemoveEnemy(id, _hostkilled: false);
			}
			Melon<BonkWithFriendsMod>.Logger.Msg($"[EnemySpawnManager] Broadcasted death for Enemy ID: {id}");
		}
	}
}
