using MelonLoader;
using Steamworks;

namespace Megabonk.BonkWithFriends.Steam;

internal static class SteamNetworkingImpl
{
	internal delegate void SteamNetConnectionStatusChangedCallbackDelegate(HSteamNetConnection connection, SteamNetConnectionInfo_t connectionInfo, ESteamNetworkingConnectionState oldState);

	internal delegate void SteamNetConnectionStatusChangedConnectionRequestCallbackDelegate(HSteamNetConnection connection, SteamNetConnectionInfo_t connectionInfo, ESteamNetworkingConnectionState oldState);

	internal delegate void SteamNetConnectionStatusChangedConnectionAcceptedCallbackDelegate(HSteamNetConnection connection, SteamNetConnectionInfo_t connectionInfo, ESteamNetworkingConnectionState oldState);

	internal delegate void SteamNetConnectionStatusChangedConnectionClosedOrRejectedCallbackDelegate(HSteamNetConnection connection, SteamNetConnectionInfo_t connectionInfo, ESteamNetworkingConnectionState oldState);

	internal delegate void SteamNetConnectionStatusChangedConnectionProblemCallbackDelegate(HSteamNetConnection connection, SteamNetConnectionInfo_t connectionInfo, ESteamNetworkingConnectionState oldState);

	internal delegate void SteamNetAuthenticationStatusCallbackDelegate(ESteamNetworkingAvailability flags, string debugMessage);

	internal delegate void SteamNetAuthenticationStatusCurrentCallbackDelegate();

	internal delegate void SteamRelayNetworkStatusCallbackDelegate(ESteamNetworkingAvailability availability, bool pingMeasurementInProgress, ESteamNetworkingAvailability networkConfigAvailability, ESteamNetworkingAvailability anyRelayAvailability, string debugMessage);

	internal delegate void SteamRelayNetworkStatusCurrentCallbackDelegate();

	private static Callback<SteamNetConnectionStatusChangedCallback_t> _steamNetConnectionStatusChangedCallback;

	private static Callback<SteamNetAuthenticationStatus_t> _steamNetAuthenticationStatusCallback;

	private static Callback<SteamRelayNetworkStatus_t> _steamRelayNetworkStatusCallback;

	internal static SteamNetConnectionStatusChangedCallbackDelegate OnSteamNetConnectionStatusChanged;

	internal static SteamNetConnectionStatusChangedConnectionRequestCallbackDelegate OnSteamNetConnectionStatusChangedConnectionRequest;

	internal static SteamNetConnectionStatusChangedConnectionAcceptedCallbackDelegate OnSteamNetConnectionStatusChangedConnectionAccepted;

	internal static SteamNetConnectionStatusChangedConnectionClosedOrRejectedCallbackDelegate OnSteamNetConnectionStatusChangedConnectionClosedOrRejected;

	internal static SteamNetConnectionStatusChangedConnectionProblemCallbackDelegate OnSteamNetConnectionStatusChangedConnectionProblem;

	internal static SteamNetAuthenticationStatusCallbackDelegate OnSteamNetAuthenticationStatus;

	internal static SteamNetAuthenticationStatusCurrentCallbackDelegate OnSteamNetAuthenticationStatusCurrent;

	internal static SteamRelayNetworkStatusCallbackDelegate OnSteamRelayNetworkStatus;

	internal static SteamRelayNetworkStatusCurrentCallbackDelegate OnSteamRelayNetworkStatusCurrent;

	internal static bool IsSetup { get; private set; }

	internal static void Setup()
	{
		SetupCallbacksAndCallResults();
		SetupSteamRelayNetworkAccess();
		SetupSteamAuthentication();
	}

	private static void SetupCallbacksAndCallResults()
	{
		_steamNetConnectionStatusChangedCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create((Callback<SteamNetConnectionStatusChangedCallback_t>.DispatchDelegate)OnSteamNetConnectionStatusChangedCallback);
		_steamNetAuthenticationStatusCallback = Callback<SteamNetAuthenticationStatus_t>.Create((Callback<SteamNetAuthenticationStatus_t>.DispatchDelegate)OnSteamNetAuthenticationStatusCallback);
		_steamRelayNetworkStatusCallback = Callback<SteamRelayNetworkStatus_t>.Create((Callback<SteamRelayNetworkStatus_t>.DispatchDelegate)OnSteamRelayNetworkStatusCallback);
		IsSetup = true;
	}

	private static void SetupSteamRelayNetworkAccess()
	{
		SteamNetworkingUtils.InitRelayNetworkAccess();
	}

	private static void SetupSteamAuthentication()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		SteamNetworkingSockets.InitAuthentication();
	}

	internal static void Reset()
	{
		DisposeCallbacksAndCallResults();
	}

	private static void DisposeCallbacksAndCallResults()
	{
		_steamNetConnectionStatusChangedCallback?.Dispose();
		_steamNetAuthenticationStatusCallback?.Dispose();
		_steamRelayNetworkStatusCallback?.Dispose();
		IsSetup = false;
	}

	private static void OnSteamNetConnectionStatusChangedCallback(SteamNetConnectionStatusChangedCallback_t steamNetConnectionStatusChangedCallback_t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Invalid comparison between Unknown and I4
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Invalid comparison between Unknown and I4
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Invalid comparison between Unknown and I4
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Invalid comparison between Unknown and I4
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Invalid comparison between Unknown and I4
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Invalid comparison between Unknown and I4
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Invalid comparison between Unknown and I4
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Invalid comparison between Unknown and I4
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Invalid comparison between Unknown and I4
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Invalid comparison between Unknown and I4
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		HSteamNetConnection hConn = steamNetConnectionStatusChangedCallback_t.m_hConn;
		SteamNetConnectionInfo_t info = steamNetConnectionStatusChangedCallback_t.m_info;
		ESteamNetworkingConnectionState eOldState = steamNetConnectionStatusChangedCallback_t.m_eOldState;
		SteamNetworkingIdentity identityRemote = info.m_identityRemote;
		HSteamListenSocket hListenSocket = info.m_hListenSocket;
		ESteamNetworkingConnectionState eState = info.m_eState;
		Melon<BonkWithFriendsMod>.Logger.Msg($"{"OnSteamNetConnectionStatusChangedCallback"} | Old: {eOldState}, New: {eState}, CH: {hConn}, LSH: {hListenSocket}, Id: {identityRemote}");
		OnSteamNetConnectionStatusChanged?.Invoke(hConn, info, eOldState);
		if ((int)eOldState == 0 && (int)eState == 1 && hListenSocket != HSteamListenSocket.Invalid)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("OnSteamNetConnectionStatusChangedConnectionRequest");
			OnSteamNetConnectionStatusChangedConnectionRequest?.Invoke(hConn, info, eOldState);
		}
		else if ((int)eOldState == 1 && ((int)eState == 3 || (int)eState == 2) && hListenSocket == HSteamListenSocket.Invalid)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("OnSteamNetConnectionStatusChangedConnectionAccepted");
			OnSteamNetConnectionStatusChangedConnectionAccepted?.Invoke(hConn, info, eOldState);
		}
		else if (((int)eOldState == 1 || (int)eOldState == 3) && (int)eState == 4)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("OnSteamNetConnectionStatusChangedConnectionClosedOrRejected");
			OnSteamNetConnectionStatusChangedConnectionClosedOrRejected?.Invoke(hConn, info, eOldState);
		}
		else if (((int)eOldState == 1 || (int)eOldState == 3) && (int)eState == 5)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("OnSteamNetConnectionStatusChangedConnectionProblem");
			OnSteamNetConnectionStatusChangedConnectionProblem?.Invoke(hConn, info, eOldState);
		}
	}

	private static void OnSteamNetAuthenticationStatusCallback(SteamNetAuthenticationStatus_t steamNetAuthenticationStatus_t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		ESteamNetworkingAvailability eAvail = steamNetAuthenticationStatus_t.m_eAvail;
		string debugMsg = steamNetAuthenticationStatus_t.m_debugMsg;
		OnSteamNetAuthenticationStatus?.Invoke(eAvail, debugMsg);
		if ((int)eAvail == 100)
		{
			OnSteamNetAuthenticationStatusCurrent?.Invoke();
		}
	}

	private static void OnSteamRelayNetworkStatusCallback(SteamRelayNetworkStatus_t steamRelayNetworkStatus_t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		ESteamNetworkingAvailability eAvail = steamRelayNetworkStatus_t.m_eAvail;
		bool pingMeasurementInProgress = steamRelayNetworkStatus_t.m_bPingMeasurementInProgress != 0;
		ESteamNetworkingAvailability eAvailNetworkConfig = steamRelayNetworkStatus_t.m_eAvailNetworkConfig;
		ESteamNetworkingAvailability eAvailAnyRelay = steamRelayNetworkStatus_t.m_eAvailAnyRelay;
		string debugMsg = steamRelayNetworkStatus_t.m_debugMsg;
		OnSteamRelayNetworkStatus?.Invoke(eAvail, pingMeasurementInProgress, eAvailNetworkConfig, eAvailAnyRelay, debugMsg);
		if ((int)eAvail == 100 && (int)eAvailNetworkConfig == 100 && (int)eAvailAnyRelay == 100)
		{
			OnSteamRelayNetworkStatusCurrent?.Invoke();
		}
	}
}
