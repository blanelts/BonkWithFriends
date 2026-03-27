using System;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Managers.Server;

public static class NetworkTimeSync
{
	private const int SYNC_SAMPLE_COUNT = 8;

	private const float STEADY_INTERVAL = 2f;

	private const float BURST_INTERVAL = 0.1f;

	private const float MAX_ACCEPTABLE_RTT = 1f;

	private static readonly float[] _offsetSamples = new float[8];

	private static readonly float[] _sortBuffer = new float[8];

	private static int _sampleIndex = 0;

	private static int _sampleCount = 0;

	private static float _smoothedOffset = 0f;

	private static bool _isInitialized = false;

	private static float _nextSyncRequestTime = 0f;

	public static float CurrentServerTime => Time.unscaledTime - _smoothedOffset;

	public static bool IsInitialized => _isInitialized;

	public static void Initialize()
	{
		Reset();
		_nextSyncRequestTime = Time.unscaledTime;
		Melon<BonkWithFriendsMod>.Logger.Msg("[TimeSync] Initialized, requesting initial sync...");
	}

	public static void Update()
	{
		if (((Object)((Object)(object)LocalPlayerManager.LocalPlayer)))
		{
			float unscaledTime = Time.unscaledTime;
			if (unscaledTime >= _nextSyncRequestTime)
			{
				RequestTimeSync(unscaledTime);
				float num = ((_sampleCount < 8) ? 0.1f : 2f);
				_nextSyncRequestTime = unscaledTime + num;
			}
		}
	}

	private static void RequestTimeSync(float now)
	{
		if (SteamNetworkClient.Instance != null)
		{
			TimeSyncRequestMessage tMsg = new TimeSyncRequestMessage
			{
				ClientSendTime = now
			};
			SteamNetworkClient.Instance.SendMessage(tMsg);
		}
	}

	public static void ProcessTimeSyncResponse(float serverTime, float clientSendTime)
	{
		float unscaledTime = Time.unscaledTime;
		float num = unscaledTime - clientSendTime;
		if (!(num > 1f))
		{
			float num2 = unscaledTime - (serverTime + num * 0.5f);
			_offsetSamples[_sampleIndex] = num2;
			_sampleIndex = (_sampleIndex + 1) % 8;
			bool num3 = !_isInitialized;
			if (_sampleCount < 8)
			{
				_sampleCount++;
			}
			float smoothedOffset = _smoothedOffset;
			_smoothedOffset = CalculateMedianOffset();
			_isInitialized = true;
			if (num3 || Mathf.Abs(_smoothedOffset - smoothedOffset) > 0.1f)
			{
				Melon<BonkWithFriendsMod>.Logger.Msg($"[TimeSync] RTT: {num * 1000f:F1}ms | Offset: {_smoothedOffset:F3}s");
			}
		}
	}

	private static float CalculateMedianOffset()
	{
		if (_sampleCount == 0)
		{
			return 0f;
		}
		if (_sampleCount == 1)
		{
			return _offsetSamples[0];
		}
		Array.Copy(_offsetSamples, 0, _sortBuffer, 0, _sampleCount);
		Array.Sort(_sortBuffer, 0, _sampleCount);
		int num = _sampleCount / 2;
		if ((_sampleCount & 1) == 0)
		{
			return (_sortBuffer[num - 1] + _sortBuffer[num]) * 0.5f;
		}
		return _sortBuffer[num];
	}

	public static void Reset()
	{
		_sampleIndex = 0;
		_sampleCount = 0;
		_smoothedOffset = 0f;
		_isInitialized = false;
		_nextSyncRequestTime = 0f;
		Array.Clear(_offsetSamples, 0, _offsetSamples.Length);
		Melon<BonkWithFriendsMod>.Logger.Msg("[TimeSync] Reset.");
	}
}
