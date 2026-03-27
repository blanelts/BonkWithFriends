using UnityEngine;

namespace Megabonk.BonkWithFriends.UI;

internal static class GuiCompat
{
	private static GUIContent _emptyContent;

	private static GUIStyle _styleNone;

	public static GUIContent Empty
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			object obj = _emptyContent;
			if (obj == null)
			{
				GUIContent val = new GUIContent(string.Empty);
				_emptyContent = val;
				obj = (object)val;
			}
			return (GUIContent)obj;
		}
	}

	public static GUIStyle StyleNone
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected O, but got Unknown
			object obj = _styleNone;
			if (obj == null)
			{
				GUIStyle val = new GUIStyle();
				_styleNone = val;
				obj = (object)val;
			}
			return (GUIStyle)obj;
		}
	}
}
