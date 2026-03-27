using System;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.UI;

[RegisterTypeInIl2Cpp]
public class Billboard : MonoBehaviour
{
	private Transform _cameraTransform;
	private Vector3 _baseScale;
	private float _baseDistance;
	private bool _constantSize;

	public Billboard(IntPtr intPtr)
		: base(intPtr)
	{
	}

	public Billboard()
		: base(ClassInjector.DerivedConstructorPointer<Billboard>())
	{
		ClassInjector.DerivedConstructorBody((Il2CppObjectBase)(object)this);
	}

	/// <summary>
	/// Call after creation to enable constant screen-size mode.
	/// The nameplate will keep the same apparent size regardless of distance.
	/// </summary>
	public void EnableConstantSize(float referenceDistance = 15f)
	{
		_constantSize = true;
		_baseScale = ((Component)this).transform.localScale;
		_baseDistance = referenceDistance;
	}

	private void Start()
	{
		if ((Object)(object)Camera.main != (Object)null)
		{
			_cameraTransform = ((Component)Camera.main).transform;
		}
	}

	private void LateUpdate()
	{
		if (_cameraTransform == null)
			return;

		Transform t = ((Component)this).transform;
		t.LookAt(t.position + _cameraTransform.rotation * Vector3.forward, _cameraTransform.rotation * Vector3.up);

		if (_constantSize)
		{
			float dist = Vector3.Distance(t.position, _cameraTransform.position);
			float scale = dist / _baseDistance;
			t.localScale = _baseScale * scale;
		}
	}
}
