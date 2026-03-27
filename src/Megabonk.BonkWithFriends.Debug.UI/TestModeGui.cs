using System;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Megabonk.BonkWithFriends.Managers;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Debug.UI;

[RegisterTypeInIl2Cpp]
public class TestModeGui : MonoBehaviour
{
	public TestModeGui(IntPtr intPtr) : base(intPtr) { }

	private void OnGUI()
	{
		TestModeManager.DrawGui();
		SpectatorManager.DrawGui();
		ResurrectionManager.DrawGui();
	}
}
