using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppTMPro;
using Megabonk.BonkWithFriends.UI;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Player;

[RegisterTypeInIl2Cpp]
public class NameplateController : MonoBehaviour
{
	public NameplateController(System.IntPtr intPtr) : base(intPtr) { }

	public NameplateController()
		: base(ClassInjector.DerivedConstructorPointer<NameplateController>())
	{
		ClassInjector.DerivedConstructorBody((Il2CppObjectBase)(object)this);
	}

	public void Initialize(string playerName)
	{
		// Use 3D TextMeshPro directly — no Canvas needed, works reliably in Il2Cpp
		GameObject textObj = new GameObject("Nameplate_" + playerName);
		textObj.transform.SetParent(((Component)this).transform, false);
		textObj.transform.localPosition = new Vector3(0f, 5f, 0f);
		textObj.transform.localRotation = Quaternion.identity;
		textObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		Billboard billboard = textObj.AddComponent<Billboard>();
		billboard.EnableConstantSize(15f);

		TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();

		// Find font from any existing TMP component in the scene
		TMP_Text existingTmp = Object.FindObjectOfType<TMP_Text>();
		if ((Object)(object)existingTmp != (Object)null && (Object)(object)existingTmp.font != (Object)null)
		{
			((TMP_Text)tmp).font = existingTmp.font;
			Melon<BonkWithFriendsMod>.Logger.Msg("[Nameplate] Found font: " + ((Object)existingTmp.font).name);
		}
		else
		{
			Melon<BonkWithFriendsMod>.Logger.Warning("[Nameplate] No TMP font found in scene!");
		}

		((TMP_Text)tmp).text = playerName;
		((TMP_Text)tmp).fontSize = 8f;
		((TMP_Text)tmp).color = Color.white;
		((TMP_Text)tmp).alignment = (TextAlignmentOptions)514; // Center
		((TMP_Text)tmp).enableWordWrapping = false;
		((TMP_Text)tmp).overflowMode = (TextOverflowModes)0;
		((TMP_Text)tmp).outlineWidth = 0.2f;
		((TMP_Text)tmp).outlineColor = new Color32(0, 0, 0, 255);

		// Render through walls using TMP Overlay shader
		MeshRenderer meshRenderer = textObj.GetComponent<MeshRenderer>();
		if ((Object)(object)meshRenderer != (Object)null)
		{
			meshRenderer.sortingOrder = 32000;
			Material mat = meshRenderer.material;
			if ((Object)(object)mat != (Object)null)
			{
				bool shaderApplied = false;
				// Overlay shader variants have ZTest Always built-in
				string[] overlayShaderNames = new[]
				{
					"TextMeshPro/Distance Field Overlay",
					"TextMeshPro/Mobile/Distance Field Overlay"
				};
				foreach (string shaderName in overlayShaderNames)
				{
					Shader overlayShader = Shader.Find(shaderName);
					if ((Object)(object)overlayShader != (Object)null)
					{
						mat.shader = overlayShader;
						shaderApplied = true;
						Melon<BonkWithFriendsMod>.Logger.Msg($"[Nameplate] Applied shader: {shaderName}");
						break;
					}
				}
				if (!shaderApplied)
				{
					// Fallback: force ZTest Always via SetFloat (SetInt may not work on some TMP shaders)
					mat.SetFloat("_ZTest", 8f);
					mat.SetFloat("_ZWrite", 0f);
					Melon<BonkWithFriendsMod>.Logger.Warning("[Nameplate] Overlay shader not found, using ZTest fallback");
				}
				mat.renderQueue = 5000;
			}
		}

		// Set RectTransform size for 3D text
		RectTransform textRect = textObj.GetComponent<RectTransform>();
		if ((Object)(object)textRect != (Object)null)
		{
			textRect.sizeDelta = new Vector2(10f, 2f);
		}

		Melon<BonkWithFriendsMod>.Logger.Msg($"[Nameplate] Created 3D nameplate for '{playerName}'");
	}
}
