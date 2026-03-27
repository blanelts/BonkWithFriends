using System;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Debug.UI;

public class NetProfilerGui : MonoBehaviour
{
	private bool _showProfiler;

	private float _nextUpdateTime;

	private const float UpdateInterval = 1f;

	private GUIStyle _style;

	private Rect _windowRect = new Rect(20f, 20f, 450f, 600f);

	private void Awake()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		_style = new GUIStyle();
		_style.font = Font.CreateDynamicFontFromOSFont("Consolas", 12);
		_style.normal.textColor = Color.white;
		RectOffset val = new RectOffset();
		val.left = 5;
		val.right = 5;
		val.top = 5;
		val.bottom = 5;
		_style.padding = val;
		_style.wordWrap = false;
		Texture2D val2 = new Texture2D(1, 1);
		val2.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.85f));
		val2.Apply();
		_style.normal.background = val2;
	}

	private void Update()
	{
		if (Input.GetKeyDown((KeyCode)284))
		{
			_showProfiler = !_showProfiler;
		}
		if (_showProfiler && Time.unscaledTime > _nextUpdateTime)
		{
			_nextUpdateTime = Time.unscaledTime + 1f;
			NetProfiler.UpdateDisplay();
		}
	}

	private void OnGUI()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (_showProfiler)
		{
			_windowRect = GUI.Window(1234, _windowRect, ((GUI.WindowFunction)((Action<int>)DrawProfilerWindow)), "Network Profiler");
		}
	}

	private void DrawProfilerWindow(int windowId)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		GUI.Label(new Rect(10f, 20f, _windowRect.width - 20f, _windowRect.height - 30f), NetProfiler.DisplayString, _style);
		GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
	}
}
