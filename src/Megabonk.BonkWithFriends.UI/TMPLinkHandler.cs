using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.UI;

[RegisterTypeInIl2Cpp]
public class TMPLinkHandler : MonoBehaviour
{
	private TMP_Text textComponent;

	private void Awake()
	{
		textComponent = ((Component)this).GetComponent<TMP_Text>();
	}

	private void Update()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetMouseButtonDown(0))
		{
			Vector3 mousePosition = Input.mousePosition;
			int num = TMP_TextUtilities.FindIntersectingLink(textComponent, mousePosition, (Camera)null);
			if (num != -1)
			{
				Application.OpenURL(((Il2CppArrayBase<TMP_LinkInfo>)(object)textComponent.textInfo.linkInfo)[num].GetLinkID());
			}
		}
	}
}
