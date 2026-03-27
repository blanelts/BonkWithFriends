using System;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Debug;

[RegisterTypeInIl2Cpp]
public class DebugVisualizer : MonoBehaviour
{
	public int lineCount = 100;

	public float radius = 3f;

	private static Material _lineMaterial;

	public DebugVisualizer(IntPtr intPtr)
		: base(intPtr)
	{
	}

	public DebugVisualizer()
		: base(ClassInjector.DerivedConstructorPointer<DebugVisualizer>())
	{
		ClassInjector.DerivedConstructorBody((Il2CppObjectBase)(object)this);
	}

	private static void CreateLineMaterial()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		if (!((Object)(object)_lineMaterial != (Object)null))
		{
			_lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
			((Object)_lineMaterial).hideFlags = (HideFlags)61;
			_lineMaterial.SetInt("_SrcBlend", 5);
			_lineMaterial.SetInt("_DstBlend", 10);
			_lineMaterial.SetInt("_Cull", 0);
			_lineMaterial.SetInt("_ZWrite", 0);
		}
	}

	public void OnRenderObject()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		CreateLineMaterial();
		_lineMaterial.SetPass(0);
		GL.PushMatrix();
		GL.MultMatrix(((Component)this).transform.localToWorldMatrix);
		GL.Begin(1);
		for (int i = 0; i < lineCount; i++)
		{
			float num = (float)i / (float)lineCount;
			float num2 = num * (float)Math.PI * 2f;
			GL.Color(new Color(num, 1f - num, 0f, 0.8f));
			GL.Vertex3(0f, 0f, 0f);
			GL.Vertex3(Mathf.Cos(num2) * radius, Mathf.Sin(num2) * radius, 0f);
		}
		GL.End();
		GL.PopMatrix();
	}
}
