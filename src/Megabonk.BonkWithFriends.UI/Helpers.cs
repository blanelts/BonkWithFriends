using System;
using Il2Cpp;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace Megabonk.BonkWithFriends.UI;

public class Helpers
{
	public static bool ErrorIfNull<T>(T item, string errorMessage)
	{
		if (item == null)
		{
			Melon<BonkWithFriendsMod>.Logger.Error(errorMessage);
			return true;
		}
		return false;
	}

	public static void DestroyAllChildren(Transform parent)
	{
		if ((Object)(object)parent == (Object)null)
		{
			return;
		}
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if ((Object)(object)child != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)child).gameObject);
			}
		}
	}

	public static GameObject CreateButton(GameObject exampleButton, Transform parent, string objectName, string buttonLabel, UnityAction onClick)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		if (ErrorIfNull<GameObject>(exampleButton, "[HELPER] Example button was null!"))
		{
			return null;
		}
		GameObject val = Object.Instantiate<GameObject>(exampleButton, parent, false);
		((Object)val).name = objectName;
		Transform val2 = val.transform.Find("DisabledOverlay");
		if (((Object)((Object)(object)val2)))
		{
			((Component)val2).gameObject.SetActive(false);
		}
		Transform val3 = val.transform.Find("T_Text");
		TextMeshProUGUI val4 = ((!((Object)((Object)(object)val3))) ? val.GetComponentInChildren<TextMeshProUGUI>(true) : ((Component)val3).GetComponent<TextMeshProUGUI>());
		((TMP_Text)val4).text = buttonLabel;
		LocalizeStringEvent component = ((Component)val4).GetComponent<LocalizeStringEvent>();
		if (((Object)((Object)(object)component)))
		{
			((Behaviour)component).enabled = false;
		}
		MyButtonNormal component2 = val.GetComponent<MyButtonNormal>();
		Button component3 = val.GetComponent<Button>();
		if ((Object)(object)component3 != (Object)null)
		{
			component3.onClick = new Button.ButtonClickedEvent();
			((UnityEvent)component3.onClick).AddListener(onClick);
			((UnityEvent)component3.onClick).AddListener(((UnityAction)((Action)((MyButton)component2).PlaySfx)));
			((UnityEvent)component3.onClick).AddListener(((UnityAction)((Action)((MyButton)component2).OnClick)));
		}
		((Behaviour)component3).enabled = true;
		Selectable component4 = val.GetComponent<Selectable>();
		if (((Object)((Object)(object)component4)))
		{
			component4.OnDeselect((BaseEventData)null);
		}
		return val;
	}
}
