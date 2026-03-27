using Megabonk.BonkWithFriends.Managers.Server;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Managers.Enemies;

public sealed class EnemyInterpolatedTransform
{
	public sealed class Config
	{
		public float InterpSpeedHorizontal = 40f;

		public float InterpSpeedVertical = 40f;

		public float MinExtrap = 0.05f;

		public float MaxExtrapMult = 1.5f;

		public float InterpolationDelay = 0.2f;

		public float TeleportDistSq = 70f;

		public float TeleportAngleDeg = 45f;

		public bool YawOnly;

		public bool EnableGrounding;

		public float RaycastUp = 1f;

		public float RaycastDown = 2.5f;

		public float FootOffset = 0.05f;

		public float VerticalSnapMax = 0.5f;

		public int GroundMask = -1;

		public float GroundCheckInterval = 0.2f;

		public static readonly Config DefaultEnemy = new Config();
	}

	private struct Snapshot
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public Vector3 Velocity;

		public float ServerTime;
	}

	private readonly Config _cfg;

	public Vector3 CurrentPosition;

	public Quaternion CurrentRotation;

	public Vector3 TargetPosition;

	public Quaternion TargetRotation;

	public Vector3 Velocity;

	public float AngularVelDegPerSec;

	public float LastServerTime;

	public uint LastSeq;

	public bool HasBaseline;

	private const int MAX_SNAPSHOTS = 32;

	private readonly Snapshot[] _snapshotBuffer = new Snapshot[32];

	private int _snapshotCount;

	private int _snapshotHead;

	private float _nextGroundCheckTime;

	private float _lastGroundY;

	public EnemyInterpolatedTransform(Config cfg = null)
	{
		_cfg = cfg ?? Config.DefaultEnemy;
	}

	public void SetTarget(float serverTime, uint seq, in Vector3 pos, in Quaternion rotIn, in Vector3 velIn, float angVelDegPerSec = 0f)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		if (!HasBaseline || seq > LastSeq)
		{
			LastSeq = seq;
			LastServerTime = serverTime;
			Quaternion rotIn2 = rotIn;
			if (_cfg.YawOnly)
			{
				Vector3 eulerAngles = rotIn2.eulerAngles;
				rotIn2 = Quaternion.Euler(0f, eulerAngles.y, 0f);
			}
			TargetPosition = pos;
			TargetRotation = rotIn2;
			Velocity = velIn;
			AngularVelDegPerSec = angVelDegPerSec;
			_snapshotHead = (_snapshotHead + 1) % 32;
			_snapshotBuffer[_snapshotHead] = new Snapshot
			{
				Position = pos,
				Rotation = rotIn2,
				Velocity = velIn,
				ServerTime = serverTime
			};
			if (_snapshotCount < 32)
			{
				_snapshotCount++;
			}
			if (!HasBaseline)
			{
				CurrentPosition = pos;
				CurrentRotation = rotIn2;
				HasBaseline = true;
				_lastGroundY = pos.y;
				Teleport(in pos, in rotIn2);
			}
		}
	}

	public void Update(Transform t)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)((Object)(object)t)) || !HasBaseline)
		{
			return;
		}
		if (_snapshotCount == 0)
		{
			t.SetPositionAndRotation(CurrentPosition, CurrentRotation);
			return;
		}
		float num = ((!NetworkTimeSync.IsInitialized) ? LastServerTime : NetworkTimeSync.CurrentServerTime);
		float num2 = num - _cfg.InterpolationDelay;
		bool flag = false;
		int num3 = -1;
		int num4 = -1;
		if (_snapshotCount >= 2)
		{
			for (int i = 0; i < _snapshotCount; i++)
			{
				int num5 = (_snapshotHead - i + 32) % 32;
				if (_snapshotBuffer[num5].ServerTime <= num2)
				{
					num3 = num5;
					if (i > 0)
					{
						num4 = (_snapshotHead - (i - 1) + 32) % 32;
						flag = true;
					}
					break;
				}
			}
		}
		Vector3 desiredPos;
		Quaternion val;
		if (flag)
		{
			Snapshot snapshot = _snapshotBuffer[num3];
			Snapshot snapshot2 = _snapshotBuffer[num4];
			float num6 = snapshot2.ServerTime - snapshot.ServerTime;
			if (num6 < 0.0001f)
			{
				desiredPos = snapshot.Position;
				val = snapshot.Rotation;
			}
			else
			{
				float num7 = (num2 - snapshot.ServerTime) / num6;
				float num8 = num7 * num7;
				float num9 = num8 * num7;
				Vector3 val2 = snapshot.Velocity * num6;
				Vector3 val3 = snapshot2.Velocity * num6;
				desiredPos = (2f * num9 - 3f * num8 + 1f) * snapshot.Position + (num9 - 2f * num8 + num7) * val2 + (-2f * num9 + 3f * num8) * snapshot2.Position + (num9 - num8) * val3;
				val = Quaternion.Slerp(snapshot.Rotation, snapshot2.Rotation, num7);
			}
		}
		else
		{
			float num10 = Mathf.Max(0f, num2 - LastServerTime);
			float num11 = _cfg.InterpolationDelay * _cfg.MaxExtrapMult;
			float num12 = Mathf.Clamp(num10, 0f, num11);
			desiredPos = TargetPosition + Velocity * num12;
			float num13 = TargetRotation.eulerAngles.y + AngularVelDegPerSec * num12;
			val = Quaternion.Euler(0f, num13, 0f);
		}
		Vector3 val4 = CurrentPosition - desiredPos;
		bool num14 = val4.sqrMagnitude >= _cfg.TeleportDistSq;
		bool flag2 = Quaternion.Angle(CurrentRotation, val) >= _cfg.TeleportAngleDeg;
		if (num14 || flag2)
		{
			ApplyGrounding(ref desiredPos, force: true);
			CurrentPosition = desiredPos;
			CurrentRotation = val;
			t.SetPositionAndRotation(CurrentPosition, CurrentRotation);
			return;
		}
		if (_cfg.EnableGrounding)
		{
			ApplyGrounding(ref desiredPos, force: false);
		}
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		Vector2 val5 = new Vector2(CurrentPosition.x, CurrentPosition.z);
		Vector2 val6 = default(Vector2);
		val6 = new Vector2(desiredPos.x, desiredPos.z);
		float num15 = 1f - Mathf.Exp((0f - _cfg.InterpSpeedHorizontal) * unscaledDeltaTime);
		Vector2 val7 = Vector2.Lerp(val5, val6, num15);
		float num16 = 1f - Mathf.Exp((0f - _cfg.InterpSpeedVertical) * unscaledDeltaTime);
		float num17 = Mathf.Lerp(CurrentPosition.y, desiredPos.y, num16);
		CurrentPosition = new Vector3(val7.x, num17, val7.y);
		CurrentRotation = Quaternion.Slerp(CurrentRotation, val, num15);
		t.SetPositionAndRotation(CurrentPosition, CurrentRotation);
	}

	private void ApplyGrounding(ref Vector3 desiredPos, bool force)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (_cfg.GroundMask == 0)
		{
			return;
		}
		if (!force && Time.unscaledTime < _nextGroundCheckTime)
		{
			desiredPos.y = Mathf.Lerp(desiredPos.y, _lastGroundY, 0.5f);
			return;
		}
		Vector3 val = desiredPos + Vector3.up * _cfg.RaycastUp;
		float num = _cfg.RaycastUp + _cfg.RaycastDown;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(val, Vector3.down, out val2, num, _cfg.GroundMask, (QueryTriggerInteraction)1))
		{
			float num2 = Mathf.Clamp(val2.point.y + _cfg.FootOffset - desiredPos.y, 0f - _cfg.VerticalSnapMax, _cfg.VerticalSnapMax);
			desiredPos.y += num2;
			_lastGroundY = desiredPos.y;
		}
		_nextGroundCheckTime = Time.unscaledTime + _cfg.GroundCheckInterval;
	}

	public void Teleport(in Vector3 pos, in Quaternion rotIn)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Quaternion targetRotation = rotIn;
		if (_cfg.YawOnly)
		{
			Vector3 eulerAngles = targetRotation.eulerAngles;
			targetRotation = Quaternion.Euler(0f, eulerAngles.y, 0f);
		}
		CurrentPosition = (TargetPosition = pos);
		CurrentRotation = (TargetRotation = targetRotation);
		Velocity = Vector3.zero;
		AngularVelDegPerSec = 0f;
		_snapshotCount = 0;
		_snapshotHead = 0;
		HasBaseline = true;
		_lastGroundY = pos.y;
	}
}
