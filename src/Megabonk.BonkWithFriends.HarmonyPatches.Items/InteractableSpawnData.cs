using System;
using UnityEngine;

namespace Megabonk.BonkWithFriends.HarmonyPatches.Items;

[Serializable]
internal struct InteractableSpawnData
{
	public int Id;

	public InteractableType Type;

	public Vector3 Position;

	public Quaternion Rotation;

	public int SubType;

	public InteractableSpawnData(int id, InteractableType type, Vector3 pos, Quaternion rot, int subType)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Id = id;
		Type = type;
		Position = pos;
		Rotation = rot;
		SubType = subType;
	}
}
