using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.MonoBehaviours.Player;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Debugging;

public static class SceneDumper
{
	public static void Dump(int maxDepth = 3)
	{
		Camera main = Camera.main;
		if ((Object)(object)main == (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("[DUMP] MainCamera not found.");
		}
		else
		{
			Transform transform = ((Component)main).transform;
			Melon<BonkWithFriendsMod>.Logger.Msg("[DUMP] MainCamera path: " + GetPath(transform));
			Transform val = transform;
			while ((Object)(object)val.parent != (Object)null)
			{
				val = val.parent;
			}
			Melon<BonkWithFriendsMod>.Logger.Msg($"[DUMP] Dumping camera root subtree: '{((Object)val).name}' (depth {maxDepth})");
			DumpTransform(val, 0, maxDepth);
		}
		NetworkedPlayer localPlayer = LocalPlayerManager.LocalPlayer;
		if ((Object)(object)localPlayer != (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("[DUMP] LocalPlayer bound: '" + ((Object)localPlayer).name + "' path: " + GetPath(((Component)localPlayer).transform));
			DumpTransform(((Component)localPlayer).transform, 0, 2);
		}
		else
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("[DUMP] LocalPlayer is not bound yet.");
		}
	}

	private static void DumpTransform(Transform t, int depth, int maxDepth)
	{
		if ((Object)(object)t == (Object)null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < depth; i++)
		{
			stringBuilder.Append("  ");
		}
		GameObject gameObject = ((Component)t).gameObject;
		Il2CppArrayBase<Component> components = gameObject.GetComponents<Component>();
		stringBuilder.Append("- ").Append(((Object)gameObject).name).Append(" [");
		for (int j = 0; j < components.Length; j++)
		{
			Component val = components[j];
			if (!((Object)(object)val == (Object)null))
			{
				string name = ((object)val).GetType().Name;
				stringBuilder.Append(name);
				if (j < components.Length - 1)
				{
					stringBuilder.Append(',');
				}
			}
		}
		stringBuilder.Append(']');
		string text = ((Object)gameObject).name.ToLowerInvariant();
		if (text.Contains("player") || text.Contains("pawn") || text.Contains("character"))
		{
			stringBuilder.Append("  <-- LIKELY PLAYER");
		}
		Melon<BonkWithFriendsMod>.Logger.Msg(stringBuilder.ToString());
		if (depth < maxDepth)
		{
			for (int k = 0; k < t.childCount; k++)
			{
				DumpTransform(t.GetChild(k), depth + 1, maxDepth);
			}
		}
	}

	private static string GetPath(Transform t)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<Transform> list = new List<Transform>();
		Transform val = t;
		while ((Object)(object)val != (Object)null)
		{
			list.Add(val);
			val = val.parent;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			stringBuilder.Append('/').Append(((Object)list[num]).name);
		}
		return stringBuilder.ToString();
	}
}
