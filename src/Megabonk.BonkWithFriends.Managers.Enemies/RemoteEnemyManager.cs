using System.Collections.Generic;
using Il2Cpp;
using Il2CppActors.Enemies;
using Il2CppAssets.Scripts.Actors.Enemies;
using Il2CppAssets.Scripts.Managers;
using Megabonk.BonkWithFriends.HarmonyPatches.Enemies;
using Megabonk.BonkWithFriends.HarmonyPatches.Items;
using Megabonk.BonkWithFriends.Managers.Server;
using Megabonk.BonkWithFriends.MonoBehaviours.Enemies;
using Megabonk.BonkWithFriends.Net;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Managers.Enemies;

public static class RemoteEnemyManager
{
	private class ActiveEnemy
	{
		public uint Id;

		public EnemyInterpolatedTransform Interp;

		public Enemy EnemyRef;
	}

	private static readonly Dictionary<uint, ActiveEnemy> _activeEnemiesMap = new Dictionary<uint, ActiveEnemy>();

	private static readonly List<ActiveEnemy> _activeEnemiesList = new List<ActiveEnemy>(64);

	private static readonly List<uint> _deadIdsScratch = new List<uint>(32);

	public static void SpawnRemoteEnemy(uint id, int type, Vector3 pos, Vector3 euler, Vector2 velXZ, float maxHp, EEnemyFlag flags, float extraSizeMultiplier)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		if (_activeEnemiesMap.ContainsKey(id))
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[RemoteEnemyManager] Enemy ID {id} already exists. Skipping spawn.");
			return;
		}
		EnemyData enemyData = DataManager.Instance.GetEnemyData((EEnemy)type);
		if (!((Object)((Object)(object)enemyData)))
		{
			Melon<BonkWithFriendsMod>.Logger.Error($"[RemoteEnemyManager] Could not find EnemyData for type {type}. Cannot spawn remote enemy {id}.");
			return;
		}
		EnemyPatches.SetSpawningFromNetwork(value: true);
		Enemy val = null;
		try
		{
			val = EnemyManager.Instance.SpawnEnemy(enemyData, pos, 0, true, flags, true, extraSizeMultiplier);
		}
		finally
		{
			EnemyPatches.SetSpawningFromNetwork(value: false);
		}
		if (!((Object)((Object)(object)val)))
		{
			Melon<BonkWithFriendsMod>.Logger.Error($"[RemoteEnemyManager] Failed to spawn game enemy for ID {id}, Type {type}.");
			return;
		}
		val.id = id;
		foreach (Renderer renderer in ((Component)val).GetComponentsInChildren<Renderer>(true))
		{
			renderer.enabled = true;
			((Component)renderer).gameObject.SetActive(true);
		}
		EnemyInterpolatedTransform enemyInterpolatedTransform = new EnemyInterpolatedTransform(new EnemyInterpolatedTransform.Config
		{
			GroundMask = ((LayerMask)(GameManager.Instance.whatIsGround)),
			FootOffset = 0.05f
		});
		enemyInterpolatedTransform.Teleport(in pos, Quaternion.Euler(euler));
		float serverTime = (NetworkTimeSync.IsInitialized ? NetworkTimeSync.CurrentServerTime : Time.unscaledTime);
		enemyInterpolatedTransform.SetTarget(serverTime, 0u, in pos, Quaternion.Euler(euler), new Vector3(velXZ.x, 0f, velXZ.y));
		ActiveEnemy activeEnemy = new ActiveEnemy
		{
			Id = id,
			Interp = enemyInterpolatedTransform,
			EnemyRef = val
		};
		_activeEnemiesMap[id] = activeEnemy;
		_activeEnemiesList.Add(activeEnemy);
		((Component)val).gameObject.AddComponent<EnemyHitFlash>();
		Melon<BonkWithFriendsMod>.Logger.Msg($"[RemoteEnemyManager] Spawned remote enemy ID: {id}, Type: {type}");
	}

	public static void RemoveEnemy(uint id, bool _hostkilled)
	{
		if (_activeEnemiesMap.TryGetValue(id, out var value))
		{
			if (_hostkilled)
			{
				EnemyPatches.SetProcessingNetworkDeath(true);
				PickupPatches.SetBlockingEnemyDeathSpawns(true);
				try { value.EnemyRef.EnemyDied(); }
				finally
				{
					PickupPatches.SetBlockingEnemyDeathSpawns(false);
					EnemyPatches.SetProcessingNetworkDeath(false);
				}
			}
			_activeEnemiesMap.Remove(id);
			_activeEnemiesList.Remove(value);
			Melon<BonkWithFriendsMod>.Logger.Msg($"[RemoteEnemyManager] Removed enemy ID: {id}");
		}
	}

	public static bool HasEnemy(uint id)
	{
		return _activeEnemiesMap.ContainsKey(id);
	}

	public static Enemy GetEnemyRef(uint id)
	{
		return _activeEnemiesMap.TryGetValue(id, out var ae) ? ae.EnemyRef : null;
	}

	public static void Update()
	{
		for (int num = _activeEnemiesList.Count - 1; num >= 0; num--)
		{
			ActiveEnemy activeEnemy = _activeEnemiesList[num];
			if (!((Object)((Object)(object)activeEnemy.EnemyRef)))
			{
				_deadIdsScratch.Add(activeEnemy.Id);
			}
			else
			{
				activeEnemy.Interp.Update(((Component)activeEnemy.EnemyRef).transform);
			}
		}
		if (_deadIdsScratch.Count <= 0)
		{
			return;
		}
		foreach (uint item in _deadIdsScratch)
		{
			RemoveEnemy(item, _hostkilled: false);
		}
		_deadIdsScratch.Clear();
	}

	public static void OnEnemyStateSnapshot(uint enemyId, short posX, short posY, short posZ, byte yawQuantized, sbyte velX, sbyte velZ, sbyte angVelQuantized, float serverTime, uint seq)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if (!_activeEnemiesMap.TryGetValue(enemyId, out var value))
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[RemoteEnemyManager] Received snapshot for unknown enemy {enemyId}");
			return;
		}
		Vector3 pos = default(Vector3);
		pos = new Vector3(Quant.DPos(posX), Quant.DPos(posY), Quant.DPos(posZ));
		float num = Quant.DYaw(yawQuantized);
		Quaternion rotIn = Quaternion.Euler(0f, num, 0f);
		Vector3 velIn = default(Vector3);
		velIn = new Vector3(Quant.DVel(velX), 0f, Quant.DVel(velZ));
		float angVelDegPerSec = Quant.DAngVel(angVelQuantized);
		if (velIn.sqrMagnitude > 0.01f)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg($"[Enemy {enemyId}] Vel: {velIn}, Speed: {velIn.magnitude:F2}");
		}
		value.Interp.SetTarget(serverTime, seq, in pos, in rotIn, in velIn, angVelDegPerSec);
	}

	public static void ClearState()
	{
		_activeEnemiesMap.Clear();
		_activeEnemiesList.Clear();
		Melon<BonkWithFriendsMod>.Logger.Msg("[RemoteEnemyManager] State cleared.");
	}
}
