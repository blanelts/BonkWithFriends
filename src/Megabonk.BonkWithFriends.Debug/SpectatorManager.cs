using System.Collections.Generic;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.MonoBehaviours.Player;
using Megabonk.BonkWithFriends.Steam;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Debug;

public static class SpectatorManager
{
	public static bool IsSpectating { get; private set; }

	private static readonly List<Transform> _targets = new();
	private static readonly List<string> _targetNames = new();
	private static int _currentIndex;
	private static Transform _cameraTransform;
	private static Vector3 _cameraOffset;
	private static string _currentTargetName = "";

	public static void EnterSpectatorMode()
	{
		if (IsSpectating) return;
		IsSpectating = true;
		RefreshTargets();

		var cam = Camera.main;
		if (cam != null && LocalPlayerManager._myPlayer != null)
		{
			_cameraTransform = cam.transform;
			_cameraOffset = _cameraTransform.position
				- ((Component)LocalPlayerManager._myPlayer).transform.position;
		}

		// Start spectating the first alive ally if available, else corpse
		if (_targets.Count > 1)
			_currentIndex = 1;
		else
			_currentIndex = 0;

		if (_currentIndex < _targetNames.Count)
			_currentTargetName = _targetNames[_currentIndex];

		Melon<BonkWithFriendsMod>.Logger.Msg($"[Spectator] Spectating: {_currentTargetName}");
	}

	public static void ExitSpectatorMode()
	{
		IsSpectating = false;
		_targets.Clear();
		_targetNames.Clear();
		_cameraTransform = null;
	}

	private static void RefreshTargets()
	{
		int prevCount = _targets.Count;
		_targets.Clear();
		_targetNames.Clear();

		if (LocalPlayerManager._myPlayer != null)
		{
			_targets.Add(((Component)LocalPlayerManager._myPlayer).transform);
			_targetNames.Add("My Corpse");
		}

		foreach (var kvp in RemotePlayerManager.GetAllPlayers())
		{
			if ((Object)(object)kvp.Value != (Object)null && !kvp.Value.State.IsDead)
			{
				_targets.Add(((Component)kvp.Value).transform);
				string name = SteamPersonaNameCache.GetCachedName(kvp.Key) ?? kvp.Key.m_SteamID.ToString();
				_targetNames.Add(name);
			}
		}

		if (_currentIndex >= _targets.Count)
			_currentIndex = _targets.Count > 0 ? 0 : 0;

		if (_currentIndex < _targetNames.Count)
			_currentTargetName = _targetNames[_currentIndex];
	}

	public static void Update()
	{
		if (!IsSpectating || _targets.Count == 0 || _cameraTransform == null) return;

		if (Input.GetMouseButtonDown(0))
		{
			SwitchTarget(1);
		}
		if (Input.GetMouseButtonDown(1))
		{
			SwitchTarget(-1);
		}

		if (Time.frameCount % 30 == 0)
			RefreshTargets();

		if (_currentIndex < _targets.Count && (Object)(object)_targets[_currentIndex] != (Object)null)
		{
			_cameraTransform.position = _targets[_currentIndex].position + _cameraOffset;
		}
	}

	private static void SwitchTarget(int direction)
	{
		if (_targets.Count <= 1) return;
		_currentIndex = (_currentIndex + direction + _targets.Count) % _targets.Count;
		_currentTargetName = _currentIndex < _targetNames.Count ? _targetNames[_currentIndex] : "???";
		Melon<BonkWithFriendsMod>.Logger.Msg($"[Spectator] Now watching: {_currentTargetName}");
	}

	public static void DrawGui()
	{
		if (!IsSpectating) return;

		string label = $"<color=yellow>Spectating: {_currentTargetName}</color>";
		GUI.Label(new Rect(Screen.width / 2f - 100f, Screen.height - 50f, 200f, 30f), label);

		if (_targets.Count > 1)
		{
			GUI.Label(new Rect(Screen.width / 2f - 100f, Screen.height - 30f, 200f, 25f),
				"<color=white>LMB / RMB - switch</color>");
		}
	}
}
