using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using Il2CppActors.Enemies;
using Il2CppAssets.Scripts.Actors;
using Il2CppAssets.Scripts.Actors.Enemies;
using Il2CppAssets.Scripts.Game.Spawning;
using Il2CppAssets.Scripts.Managers;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Megabonk.BonkWithFriends.Managers.Enemies;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Megabonk.BonkWithFriends.HarmonyPatches.Enemies;

[HarmonyPatch]
public static class EnemyPatches
{
	[HarmonyPatch]
	public static class SpawnPositions_Patch
	{
		private struct PlayerSpawnData
		{
			public Vector3 Position;

			public Vector3 Forward;

			public Vector3 Velocity;

			public bool IsValid;
		}

		private static readonly PlayerSpawnData[] _playerCache = new PlayerSpawnData[16];

		private static readonly List<RemotePlayerManager.PlayerTarget> _tempPlayerList = new List<RemotePlayerManager.PlayerTarget>(16);

		private static readonly RaycastHit[] _hitBuffer = (RaycastHit[])(object)new RaycastHit[8];

		private static int _activePlayerCount = 0;

		private static int _lastCacheFrame = -1;

		private static int _cachedLayerMask = -1;

		private static int _roundRobinIndex = 0;

		private static int _roundRobinIndexBiased = 0;

		private const int CACHE_REFRESH_INTERVAL = 30;

		private const float RAYCAST_START_HEIGHT = 500f;

		private const float RAYCAST_DISTANCE = 1000f;

		private static void UpdatePlayerCache()
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			int frameCount = Time.frameCount;
			if (frameCount == _lastCacheFrame)
			{
				return;
			}
			_lastCacheFrame = frameCount;
			if (_cachedLayerMask == -1 && ((Object)((Object)(object)GameManager.Instance)))
			{
				LayerMask whatIsGround = GameManager.Instance.whatIsGround;
				_cachedLayerMask = whatIsGround.value;
			}
			if (frameCount % 30 == 0 || _activePlayerCount == 0)
			{
				_tempPlayerList.Clear();
				RemotePlayerManager.FillAllPlayerTargets(_tempPlayerList);
				_activePlayerCount = 0;
				for (int i = 0; i < _tempPlayerList.Count && i < _playerCache.Length; i++)
				{
					if (((Object)((Object)(object)_tempPlayerList[i].Transform)))
					{
						_activePlayerCount++;
					}
				}
			}
			for (int j = 0; j < _activePlayerCount; j++)
			{
				RemotePlayerManager.PlayerTarget playerTarget = _tempPlayerList[j];
				if (((Object)((Object)(object)playerTarget.Transform)))
				{
					_playerCache[j].Position = playerTarget.Transform.position;
					_playerCache[j].Forward = playerTarget.Transform.forward;
					_playerCache[j].IsValid = true;
					_playerCache[j].Velocity = (((Object)(object)playerTarget.Rigidbody != (Object)null) ? playerTarget.Rigidbody.velocity : Vector3.zero);
				}
				else
				{
					_playerCache[j].IsValid = false;
				}
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SpawnPositions), "GetEnemySpawnPosition")]
		public static bool GetEnemySpawnPosition(ref Vector3 __result, EnemyData enemyData, int attempts, bool useDirectionBias)
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			if (SteamNetworkManager.Mode == SteamNetworkMode.None)
			{
				return true;
			}
			UpdatePlayerCache();
			if (_activePlayerCount == 0)
			{
				return true;
			}
			int num = 0;
			int num2 = _roundRobinIndex % _activePlayerCount;
			_roundRobinIndex++;
			while (!_playerCache[num2].IsValid && num < _activePlayerCount)
			{
				num2 = (num2 + 1) % _activePlayerCount;
				num++;
			}
			if (!_playerCache[num2].IsValid)
			{
				return true;
			}
			Vector3 position = _playerCache[num2].Position;
			float num3 = 20f;
			float num4 = 80f;
			for (int i = 0; i < attempts; i++)
			{
				Vector2 val = Random.insideUnitCircle;
				if (val == Vector2.zero)
				{
					val = Vector2.one;
				}
				val.Normalize();
				val *= Random.Range(num3, num4);
				float num5 = position.x + val.x;
				float num6 = position.z + val.y;
				int num7 = Physics.RaycastNonAlloc(new Vector3(num5, 500f, num6), Vector3.down, ((Il2CppStructArray<RaycastHit>)(_hitBuffer)), 1000f, _cachedLayerMask);
				if (num7 <= 0)
				{
					continue;
				}
				float num8 = float.MaxValue;
				int num9 = -1;
				for (int j = 0; j < num7; j++)
				{
					float num10 = Mathf.Abs(_hitBuffer[j].point.y - position.y);
					if (num10 < 8f && num10 < num8)
					{
						num8 = num10;
						num9 = j;
					}
				}
				if (num9 != -1)
				{
					__result = _hitBuffer[num9].point;
					return false;
				}
			}
			MelonLogger.Warning("Multiplayer Spawn: All attempts failed, falling back to default.");
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SpawnPositions), "GetEnemySpawnPositionBiased")]
		public static bool GetEnemySpawnPositionBiased_Prefix(ref Vector3 __result, EnemyData enemyData, float playerDirectionBias, int attempts)
		{
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			if (SteamNetworkManager.Mode == SteamNetworkMode.None)
			{
				return true;
			}
			UpdatePlayerCache();
			if (_activePlayerCount == 0)
			{
				return true;
			}
			int num = 0;
			int num2 = _roundRobinIndexBiased % _activePlayerCount;
			_roundRobinIndexBiased++;
			while (!_playerCache[num2].IsValid && num < _activePlayerCount)
			{
				num2 = (num2 + 1) % _activePlayerCount;
				num++;
			}
			if (!_playerCache[num2].IsValid)
			{
				return true;
			}
			PlayerSpawnData playerSpawnData = _playerCache[num2];
			if ((Object)(object)enemyData != (Object)null)
			{
				_ = enemyData.speed;
			}
			Vector3 val = playerSpawnData.Position + playerSpawnData.Velocity * 2f;
			Vector3 forward = playerSpawnData.Forward;
			forward.y = 0f;
			forward.Normalize();
			float num3 = 20f;
			float num4 = 45f;
			for (int i = 0; i < attempts; i++)
			{
				Vector2 insideUnitCircle = Random.insideUnitCircle;
				Vector2 normalized = insideUnitCircle.normalized;
				Vector3 val2 = Vector3.Slerp(new Vector3(normalized.x, 0f, normalized.y), forward, playerDirectionBias);
				Vector3 normalized2 = val2.normalized;
				float num5 = Random.Range(num3, num4);
				Vector3 val3 = val + normalized2 * num5;
				float num6 = playerSpawnData.Position.y + 100f;
				float num7 = 20f;
				int num8 = Physics.RaycastNonAlloc(new Vector3(val3.x, num6, val3.z), Vector3.down, ((Il2CppStructArray<RaycastHit>)(_hitBuffer)), num7, _cachedLayerMask);
				if (num8 <= 0)
				{
					continue;
				}
				float num9 = float.MaxValue;
				int num10 = -1;
				for (int j = 0; j < num8; j++)
				{
					float num11 = Mathf.Abs(_hitBuffer[j].point.y - playerSpawnData.Position.y);
					if (num11 < 8f && num11 < num9)
					{
						num9 = num11;
						num10 = j;
					}
				}
				if (num10 != -1)
				{
					__result = _hitBuffer[num10].point;
					return false;
				}
			}
			return true;
		}
	}

	[HarmonyPatch]
	public static class EnemyMovement_GetTargetPosition_Patch
	{
		private struct PlayerCacheEntry
		{
			public Transform Transform;

			public Rigidbody Rigidbody;

			public Vector3 Position;
		}

		private struct EnemyTarget
		{
			public int PlayerIndex;

			public Rigidbody Rigidbody;

			public int LastUpdateFrame;
		}

		private const int MAX_ENEMIES_CAP = 4000;

		private static readonly EnemyTarget[] _enemyTargets = new EnemyTarget[4000];

		private static readonly bool[] _enemyHasTarget = new bool[4000];

		private static readonly PlayerCacheEntry[] _playerCache = new PlayerCacheEntry[16];

		private static readonly List<RemotePlayerManager.PlayerTarget> _tempTargets = new List<RemotePlayerManager.PlayerTarget>(16);

		private static int _playerCount = 0;

		private static int _lastGlobalUpdateFrame = -1;

		private const int PLAYER_REFRESH_INTERVAL = 30;

		private const int UPDATE_INTERVAL = 16;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(EnemyMovementRb), "GetTargetPosition")]
		public static void MultiplayerTargetOverride(EnemyMovementRb __instance, ref Vector3 __result)
		{
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			if (!SteamNetworkManager.IsServer)
			{
				return;
			}
			int frameCount = Time.frameCount;
			if (_lastGlobalUpdateFrame != frameCount)
			{
				UpdatePlayerCache(frameCount);
				_lastGlobalUpdateFrame = frameCount;
			}
			if (_playerCount == 0)
			{
				return;
			}
			uint id = __instance.enemy.id;
			if (id >= 4000)
			{
				return;
			}
			bool num = _enemyHasTarget[id];
			ref EnemyTarget reference = ref _enemyTargets[id];
			bool flag = ((frameCount + (int)id) & 0xF) == 0;
			if (!num || flag || reference.PlayerIndex < 0 || reference.PlayerIndex >= _playerCount || !((Object)((Object)(object)_playerCache[reference.PlayerIndex].Transform)))
			{
				Vector3 position = ((Component)__instance).transform.position;
				float num2 = float.MaxValue;
				int num3 = -1;
				for (int i = 0; i < _playerCount; i++)
				{
					Vector3 val = _playerCache[i].Position - position;
					float sqrMagnitude = val.sqrMagnitude;
					if (sqrMagnitude < num2)
					{
						num2 = sqrMagnitude;
						num3 = i;
					}
				}
				if (num3 >= 0)
				{
					reference.PlayerIndex = num3;
					reference.Rigidbody = _playerCache[num3].Rigidbody;
					reference.LastUpdateFrame = frameCount;
					_enemyHasTarget[id] = true;
					if (((Object)((Object)(object)reference.Rigidbody)))
					{
						__instance.enemy.target = reference.Rigidbody;
					}
					__result = _playerCache[num3].Position;
				}
			}
			else
			{
				__result = _playerCache[reference.PlayerIndex].Position;
			}
		}

		private static void UpdatePlayerCache(int currentFrame)
		{
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			if (currentFrame % 30 == 0 || _playerCount == 0)
			{
				_tempTargets.Clear();
				RemotePlayerManager.FillAllPlayerTargets(_tempTargets);
				_playerCount = 0;
				for (int i = 0; i < _tempTargets.Count && i < _playerCache.Length; i++)
				{
					if (((Object)((Object)(object)_tempTargets[i].Transform)))
					{
						_playerCache[_playerCount].Transform = _tempTargets[i].Transform;
						_playerCache[_playerCount].Rigidbody = _tempTargets[i].Rigidbody;
						_playerCount++;
					}
				}
			}
			for (int j = 0; j < _playerCount; j++)
			{
				if (((Object)((Object)(object)_playerCache[j].Transform)))
				{
					_playerCache[j].Position = _playerCache[j].Transform.position;
				}
			}
		}

		public static void OnEnemyDied(uint enemyId)
		{
			if (enemyId < 4000)
			{
				_enemyHasTarget[enemyId] = false;
			}
		}

		public static void ClearCache()
		{
			Array.Clear(_enemyHasTarget, 0, _enemyHasTarget.Length);
			_playerCount = 0;
		}
	}

	[HarmonyPatch]
	public static class EnemyManager_GetNumMaxEnemies_Patch
	{
		private static float SpawnRateMultiplier => Preferences.EnemySpawnRate.Value;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(EnemyManager), "GetNumMaxEnemies")]
		public static void GetNumMaxEnemies(ref int __result)
		{
			if (SteamNetworkManager.IsServer && SpawnRateMultiplier >= 1f)
			{
				float num = (float)__result * SpawnRateMultiplier;
				__result = Mathf.CeilToInt(num);
			}
		}
	}

	private static bool _isSpawningFromNetwork;

	internal static bool IsSpawningFromNetwork => _isSpawningFromNetwork;

	private static bool _processingNetworkDeath;

	private static float _preDamageHp;

	internal static void SetProcessingNetworkDeath(bool value) => _processingNetworkDeath = value;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(EnemyManager), "SpawnEnemy", new Type[]
	{
		typeof(EnemyData),
		typeof(int),
		typeof(bool),
		typeof(EEnemyFlag),
		typeof(bool)
	})]
	private static bool SpawnEnemy_BySummoner_Prefix()
	{
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined && SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer)
		{
			return _isSpawningFromNetwork;
		}
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(EnemyManager), "SpawnEnemy", new Type[]
	{
		typeof(EnemyData),
		typeof(Vector3),
		typeof(int),
		typeof(bool),
		typeof(EEnemyFlag),
		typeof(bool),
		typeof(float)
	})]
	private static bool SpawnEnemy_ByPos_Prefix()
	{
		if (SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined && SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer)
		{
			return _isSpawningFromNetwork;
		}
		return true;
	}

	internal static void SetSpawningFromNetwork(bool value)
	{
		_isSpawningFromNetwork = value;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Enemy), "InitEnemy")]
	private static void OnEnemyAwake_Postfix(Enemy __instance)
	{
		if (SteamNetworkManager.IsServer && !((Object)(object)__instance == (Object)null) && __instance.id != 0 && !IsSpawningFromNetwork)
		{
			EnemySpawnManager.RegisterHostEnemy(__instance);
			EnemySpawnManager.BroadcastEnemySpawn(__instance);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Enemy), "MyFixedUpdate")]
	public static bool Enemy_MyFixedUpdate_Prefix(Enemy __instance)
	{
		if (SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer)
		{
			return false;
		}
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Enemy), "MyUpdate")]
	public static bool Enemy_MyUpdate_Prefix(Enemy __instance)
	{
		// Разрешаем MyUpdate на клиенте для визуальных обновлений
		// (material flash reset, анимации, эффекты)
		// Движение врагов на клиенте блокируется Enemy_Move_Prefix
		// и перезаписывается RemoteEnemyManager.Update() через интерполяцию
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Enemy), "DamageFromPlayerWeapon")]
	public static bool Enemy_DamageFromPlayerWeapon_Prefix(Enemy __instance)
	{
		if (SteamNetworkManager.Mode == SteamNetworkMode.None)
			return true;

		_preDamageHp = __instance.hp;
		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Enemy), "DamageFromPlayerWeapon")]
	public static void Enemy_DamageFromPlayerWeapon_Postfix(Enemy __instance)
	{
		if (SteamNetworkManager.Mode == SteamNetworkMode.None)
			return;

		float damage = _preDamageHp - __instance.hp;
		if (damage <= 0f || __instance.id == 0)
			return;

		if (SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer)
		{
			// Client: send damage to server, revert native HP change (server is authoritative)
			LocalPlayerManager.SendProjectileHit(0, 0, Vector3.zero, Vector3.zero, __instance.id, damage);
			__instance.hp = _preDamageHp;
		}
		else if (SteamNetworkManager.IsServer)
		{
			// Host: broadcast damage to clients so they stay in sync
			SteamNetworkServer.Instance?.BroadcastToRemoteClients(new EnemyDamagedMessage
			{
				EnemyId = __instance.id,
				HpNow = __instance.hp,
				DamageForFx = damage
			});
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Enemy), "EnemyDied", new Type[] { })]
	public static void OnEnemyDied_NoArgs_Prefix(Enemy __instance)
	{
		if (SteamNetworkManager.Mode == SteamNetworkMode.None)
			return;
		// Skip if this death was already triggered via network (prevents cascade re-broadcast)
		if (_processingNetworkDeath)
			return;
		EnemySpawnManager.ProcessEnemyDeath(__instance);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Enemy), "EnemyDied", new Type[] { typeof(DamageContainer) })]
	public static void OnEnemyDied_WithDamage_Prefix(Enemy __instance)
	{
		if (SteamNetworkManager.Mode == SteamNetworkMode.None)
			return;
		// Skip if this death was already triggered via network (prevents cascade re-broadcast)
		if (_processingNetworkDeath)
			return;
		EnemySpawnManager.ProcessEnemyDeath(__instance);
	}
}
