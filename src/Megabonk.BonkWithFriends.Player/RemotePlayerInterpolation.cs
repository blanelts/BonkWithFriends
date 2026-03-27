using System.Collections.Generic;
using Megabonk.BonkWithFriends.Managers.Server;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Player;

[RegisterTypeInIl2Cpp]
public class RemotePlayerInterpolation : MonoBehaviour
{
	private struct Snapshot
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public Vector3 Velocity;

		public float ServerTime;
	}

	private readonly List<Snapshot> _snapshots = new List<Snapshot>(16);

	private const int MAX_SNAPSHOTS = 16;

	private const float INTERPOLATION_DELAY = 0.15f;

	private const float MAX_EXTRAPOLATION_TIME = 0.5f;

	private float _lastPacketTime;

	private bool _hasBaseline;

	public Vector3 Velocity { get; private set; }

	private void Awake()
	{
		_hasBaseline = false;
	}

	public void OnRemoteMovementUpdate(Vector3 pos, Quaternion rot, Vector3 vel, float serverTime)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (!_hasBaseline || !(serverTime <= _lastPacketTime))
		{
			_lastPacketTime = serverTime;
			Velocity = vel;
			if (!_hasBaseline)
			{
				Teleport(pos, rot);
				_hasBaseline = true;
			}
			_snapshots.Add(new Snapshot
			{
				Position = pos,
				Rotation = rot,
				Velocity = vel,
				ServerTime = serverTime
			});
			if (_snapshots.Count > 16)
			{
				_snapshots.RemoveAt(0);
			}
		}
	}

	public void Teleport(Vector3 pos, Quaternion rot)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.parent.position = pos;
		((Component)this).transform.parent.rotation = rot;
		Velocity = Vector3.zero;
		_snapshots.Clear();
		_hasBaseline = false;
	}

	public void Update()
	{
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		if (!_hasBaseline || _snapshots.Count == 0)
		{
			return;
		}
		float num = (NetworkTimeSync.IsInitialized ? NetworkTimeSync.CurrentServerTime : _lastPacketTime) - 0.15f;
		Snapshot snapshot = _snapshots[0];
		Snapshot snapshot2 = _snapshots[0];
		bool flag = false;
		for (int num2 = _snapshots.Count - 1; num2 >= 0; num2--)
		{
			if (_snapshots[num2].ServerTime <= num)
			{
				snapshot = _snapshots[num2];
				if (num2 + 1 < _snapshots.Count)
				{
					snapshot2 = _snapshots[num2 + 1];
					flag = true;
				}
				break;
			}
		}
		Vector3 position;
		Quaternion rotation;
		if (flag)
		{
			float num3 = snapshot2.ServerTime - snapshot.ServerTime;
			float num4 = 0f;
			if (num3 > 0.0001f)
			{
				num4 = (num - snapshot.ServerTime) / num3;
			}
			position = Vector3.Lerp(snapshot.Position, snapshot2.Position, num4);
			rotation = Quaternion.Slerp(snapshot.Rotation, snapshot2.Rotation, num4);
		}
		else
		{
			Snapshot snapshot3 = _snapshots[_snapshots.Count - 1];
			float num5 = Mathf.Clamp(num - snapshot3.ServerTime, 0f, 0.5f);
			position = snapshot3.Position + snapshot3.Velocity * num5;
			rotation = snapshot3.Rotation;
		}
		((Component)this).transform.parent.position = position;
		((Component)this).transform.parent.rotation = rotation;
	}
}
