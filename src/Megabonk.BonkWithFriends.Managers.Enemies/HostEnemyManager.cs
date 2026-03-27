using System;
using System.Collections.Generic;
using Il2CppAssets.Scripts.Actors.Enemies;
using Megabonk.BonkWithFriends.HarmonyPatches.Enemies;
using Megabonk.BonkWithFriends.Net;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Steam;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Managers.Enemies;

public static class HostEnemyManager
{
	private struct CellKey : IEquatable<CellKey>
	{
		public readonly int X;

		public readonly int Z;

		public CellKey(int x, int z)
		{
			X = x;
			Z = z;
		}

		public bool Equals(CellKey other)
		{
			if (X == other.X)
			{
				return Z == other.Z;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is CellKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (X * 73856093) ^ (Z * 19349663);
		}
	}

	private struct SentState
	{
		public Vector3 Pos;

		public float YawDeg;

		public Vector2 VelXZ;

		public float AngVelDeg;

		public uint Seq;

		public float ServerTime;
	}

	private const int MAX_ENEMIES = 4000;

	private static readonly Enemy[] _syncedEnemies = (Enemy[])(object)new Enemy[4000];

	private static readonly HashSet<uint> _activeEnemyIds = new HashSet<uint>();

	private static readonly Dictionary<CellKey, HashSet<Enemy>> _grid = new Dictionary<CellKey, HashSet<Enemy>>();

	private static readonly CellKey?[] _enemyCells = new CellKey?[4000];

	private const float CELL_SIZE = 100f;

	private static readonly List<Enemy> _statBatchScratch = new List<Enemy>(512);

	private static readonly List<Enemy> _movementBatchScratch = new List<Enemy>(512);

	private static float _lastStatBatchTime;

	private const float STAT_BATCH_INTERVAL = 0.05f;

	private static float _lastMovementBatchTime;

	private const float MOVEMENT_BATCH_INTERVAL = 0.1f;

	private static int _statPhase = 0;

	private const int NUM_BATCH_PHASES = 10;

	private static readonly SentState?[] _lastSent = new SentState?[4000];

	private static readonly EEnemyFlag?[] _lastEnemyFlags = new EEnemyFlag?[4000];

	private static readonly uint[] _sequenceCounters = new uint[4000];

	private const float DR_MAX_POS_ERR = 0.15f;

	private const float DR_MAX_YAW_ERR_DEG = 5f;

	private const float DR_HEARTBEAT_SEC = 0.1f;

	public static void RegisterHostEnemy(Enemy enemy)
	{
		if ((Object)(object)enemy == (Object)null)
		{
			return;
		}
		uint id = enemy.id;
		if (id < 4000)
		{
			if (!((Object)((Object)(object)_syncedEnemies[id])))
			{
				_activeEnemyIds.Add(id);
			}
			_syncedEnemies[id] = enemy;
			UpdateEnemyInGrid(enemy);
		}
	}

	public static void UnregisterHostEnemy(uint id, bool _clientKilled)
	{
		if (id < 4000)
		{
			Enemy val = _syncedEnemies[id];
			EnemyPatches.EnemyMovement_GetTargetPosition_Patch.OnEnemyDied(id);
			if (((Object)((Object)(object)val)))
			{
				RemoveEnemyFromGrid(val);
			}
			if (_clientKilled)
			{
				EnemyPatches.SetProcessingNetworkDeath(true);
				try { val.EnemyDied(); }
				finally { EnemyPatches.SetProcessingNetworkDeath(false); }
			}
			if (((Object)((Object)(object)_syncedEnemies[id])))
			{
				_syncedEnemies[id] = null;
				_activeEnemyIds.Remove(id);
			}
			_lastSent[id] = null;
			_lastEnemyFlags[id] = null;
			_sequenceCounters[id] = 0u;
		}
	}

	public static Enemy GetTrackedEnemy(uint id)
	{
		if (id >= 4000)
		{
			return null;
		}
		return _syncedEnemies[id];
	}

	public static void ClearState()
	{
		Array.Clear(_syncedEnemies, 0, 4000);
		_activeEnemyIds.Clear();
		Array.Clear(_lastSent, 0, 4000);
		Array.Clear(_lastEnemyFlags, 0, 4000);
		Array.Clear(_sequenceCounters, 0, 4000);
		_grid.Clear();
		Array.Clear(_enemyCells, 0, 4000);
		_lastStatBatchTime = 0f;
		_lastMovementBatchTime = 0f;
		_statPhase = 0;
	}

	public static void HostNetworkTick()
	{
		if (SteamNetworkManager.IsServer)
		{
			float unscaledTime = Time.unscaledTime;
			if (unscaledTime - _lastStatBatchTime >= 0.05f)
			{
				ProcessStatBatch(unscaledTime);
				_lastStatBatchTime = unscaledTime;
			}
			if (unscaledTime - _lastMovementBatchTime >= 0.1f)
			{
				ProcessMovementBatch(unscaledTime);
				_lastMovementBatchTime = unscaledTime;
			}
		}
	}

	private static void ProcessStatBatch(float serverTime)
	{
		_statPhase = (_statPhase + 1) % 10;
	}

	private static void ProcessMovementBatch(float serverTime)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Expected I4, but got Unknown
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		_movementBatchScratch.Clear();
		EnemyStateBatchMessage enemyStateBatchMessage = new EnemyStateBatchMessage
		{
			States = new List<EnemyStateBatchMessage.EnemyState>()
		};
		int num = 0;
		foreach (uint activeEnemyId in _activeEnemyIds)
		{
			Enemy val = _syncedEnemies[activeEnemyId];
			if (!((Object)((Object)(object)val)))
			{
				continue;
			}
			bool num2 = val.CanMove();
			UpdateEnemyInGrid(val);
			Vector3 position = ((Component)val).transform.position;
			Quaternion rotation = ((Component)val).transform.rotation;
			float y = rotation.eulerAngles.y;
			Vector2 zero = Vector2.zero;
			float num3 = 0f;
			if (num2)
			{
				if (((Object)((Object)(object)val.enemyMovement)))
				{
					Rigidbody rb = val.enemyMovement.rb;
					if ((Object)(object)rb != (Object)null && !rb.isKinematic)
					{
						Vector3 velocity = rb.velocity;
						zero = new Vector2(velocity.x, velocity.z);
						num3 = rb.angularVelocity.y * 57.29578f;
					}
					else
					{
						Vector3 baseVelocity = val.enemyMovement.baseVelocity;
						zero = new Vector2(baseVelocity.x, baseVelocity.z);
					}
				}
			}
			else
			{
				zero = Vector2.zero;
				num3 = 0f;
			}
			if (ShouldSendEnemyState(activeEnemyId, position, y, zero, num3, val.enemyFlag, serverTime))
			{
				EnemyStateBatchMessage.EnemyState enemyState = new EnemyStateBatchMessage.EnemyState
				{
					EnemyId = activeEnemyId,
					PosX = Quant.QPos(position.x),
					PosY = Quant.QPos(position.y),
					PosZ = Quant.QPos(position.z),
					YawQuantized = Quant.QYaw(y),
					VelX = Quant.QVel(zero.x),
					VelZ = Quant.QVel(zero.y),
					AngVelQuantized = Quant.QAngVel(num3),
					Hp = (ushort)Mathf.Clamp(val.hp, 0f, 65535f),
					MaxHp = (ushort)Mathf.Clamp(val.maxHp, 0f, 65535f),
					Flags = (int)val.enemyFlag,
					ServerTime = serverTime,
					Seq = _sequenceCounters[activeEnemyId]++
				};
				if (num + 40 > 1150 && enemyStateBatchMessage.States.Count > 0)
				{
					SendBatch(enemyStateBatchMessage);
					enemyStateBatchMessage.States.Clear();
					num = 0;
				}
				enemyStateBatchMessage.States.Add(enemyState);
				num += 40;
				_lastSent[activeEnemyId] = new SentState
				{
					Pos = position,
					YawDeg = y,
					VelXZ = zero,
					AngVelDeg = num3,
					Seq = enemyState.Seq,
					ServerTime = serverTime
				};
				_lastEnemyFlags[activeEnemyId] = val.enemyFlag;
			}
		}
		if (enemyStateBatchMessage.States.Count > 0)
		{
			SendBatch(enemyStateBatchMessage);
		}
	}

	private static bool ShouldSendEnemyState(uint id, Vector3 pos, float yawDeg, Vector2 velXZ, float angVelDeg, EEnemyFlag flags, float serverTime)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (!_lastSent[id].HasValue)
		{
			return true;
		}
		SentState value = _lastSent[id].Value;
		if (serverTime - value.ServerTime > 0.1f)
		{
			return true;
		}
		if (_lastEnemyFlags[id] != (EEnemyFlag?)flags)
		{
			return true;
		}
		if (Vector3.SqrMagnitude(pos - value.Pos) > 0.0225f)
		{
			return true;
		}
		if (Mathf.Abs(Mathf.DeltaAngle(yawDeg, value.YawDeg)) > 5f)
		{
			return true;
		}
		if (Vector2.SqrMagnitude(velXZ - value.VelXZ) > 0.25f)
		{
			return true;
		}
		if (Mathf.Abs(angVelDeg - value.AngVelDeg) > 5f)
		{
			return true;
		}
		return false;
	}

	private static void SendBatch(EnemyStateBatchMessage batch)
	{
		if (SteamNetworkServer.Instance != null)
		{
			SteamNetworkServer.Instance.BroadcastToRemoteClients(batch);
		}
	}

	private static CellKey CellFor(in Vector3 pos)
	{
		return new CellKey(Mathf.FloorToInt(pos.x / 100f), Mathf.FloorToInt(pos.z / 100f));
	}

	private static void UpdateEnemyInGrid(Enemy enemy)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		CellKey cellKey = CellFor(((Component)enemy).transform.position);
		uint id = enemy.id;
		CellKey? cellKey2 = _enemyCells[id];
		if (cellKey2.HasValue)
		{
			CellKey value = cellKey2.Value;
			if (value.Equals(cellKey))
			{
				return;
			}
			if (_grid.TryGetValue(value, out var value2))
			{
				value2.Remove(enemy);
				if (value2.Count == 0)
				{
					_grid.Remove(value);
				}
			}
		}
		_enemyCells[id] = cellKey;
		if (!_grid.TryGetValue(cellKey, out var value3))
		{
			value3 = new HashSet<Enemy>();
			_grid[cellKey] = value3;
		}
		value3.Add(enemy);
	}

	private static void RemoveEnemyFromGrid(Enemy enemy)
	{
		uint id = enemy.id;
		if (id >= 4000)
		{
			return;
		}
		CellKey? cellKey = _enemyCells[id];
		if (!cellKey.HasValue)
		{
			return;
		}
		CellKey value = cellKey.Value;
		if (_grid.TryGetValue(value, out var value2))
		{
			value2.Remove(enemy);
			if (value2.Count == 0)
			{
				_grid.Remove(value);
			}
		}
		_enemyCells[id] = null;
	}
}
