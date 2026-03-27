using System;
using Megabonk.BonkWithFriends.Networking.Steam;
using Steamworks;

namespace Megabonk.BonkWithFriends.Steam;

internal static class SteamMatchmakingImpl
{
	internal delegate void LobbyCreatedCallResultDelegate(EResult flags, CSteamID steamLobbyId, bool ioFailure);

	internal delegate void LobbyCreatedOKCallResultDelegate(CSteamID steamLobbyId);

	internal delegate void LobbyCreatedFailCallResultDelegate();

	internal delegate void LobbyCreatedTimeoutCallResultDelegate();

	internal delegate void LobbyCreatedLimitExceededCallResultDelegate();

	internal delegate void LobbyCreatedAccessDeniedCallResultDelegate();

	internal delegate void LobbyCreatedNoConnectionCallResultDelegate();

	internal delegate void LobbyEnterCallResultDelegate(CSteamID steamLobbyId, bool lobbyLocked, EChatRoomEnterResponse flags, bool ioFailure);

	internal delegate void LobbyEnterSuccessCallResultDelegate(CSteamID steamLobbyId, bool lobbyLocked);

	internal delegate void LobbyEnterErrorCallResultDelegate(CSteamID steamLobbyId, bool lobbyLocked);

	internal delegate void LobbyEnterCallbackDelegate(CSteamID steamLobbyId, bool lobbyLocked, EChatRoomEnterResponse flags);

	internal delegate void LobbyEnterSuccessCallbackDelegate(CSteamID steamLobbyId, bool lobbyLocked);

	internal delegate void LobbyEnterErrorCallbackDelegate(CSteamID steamLobbyId, bool lobbyLocked);

	internal delegate void LobbyDataUpdateCallbackDelegate(CSteamID steamLobbyId, CSteamID steamMemberId, bool success);

	internal delegate void LobbyDataUpdateLobbyCallbackDelegate(CSteamID steamLobbyId);

	internal delegate void LobbyDataUpdateMemberCallbackDelegate(CSteamID steamLobbyId, CSteamID steamMemberId);

	internal delegate void LobbyChatUpdateCallbackDelegate(CSteamID steamLobbyId, CSteamID steamUserIdRecipient, CSteamID steamUserIdInitiator, EChatMemberStateChange flags);

	internal delegate void LobbyChatUpdateMemberEnteredCallbackDelegate(CSteamID steamLobbyId, CSteamID steamUserIdRecipient, CSteamID steamUserIdInitiator);

	internal delegate void LobbyChatUpdateMemberLeftCallbackDelegate(CSteamID steamLobbyId, CSteamID steamUserIdRecipient, CSteamID steamUserIdInitiator);

	internal delegate void LobbyChatUpdateMemberDisconnectedCallbackDelegate(CSteamID steamLobbyId, CSteamID steamUserIdRecipient, CSteamID steamUserIdInitiator);

	internal delegate void LobbyChatUpdateMemberKickedCallbackDelegate(CSteamID steamLobbyId, CSteamID steamUserIdRecipient, CSteamID steamUserIdInitiator);

	internal delegate void LobbyChatUpdateMemberBannedCallbackDelegate(CSteamID steamLobbyId, CSteamID steamUserIdRecipient, CSteamID steamUserIdInitiator);

	internal delegate void LobbyInviteCallbackDelegate(CSteamID steamUserIdInviter, CSteamID steamLobbyId, CSteamID steamGameId);

	internal delegate void LobbyLeaveDelegate(CSteamID steamLobbyId);

	internal delegate void LobbyMatchListCallResultDelegate(int lobbiesMatching, bool ioFailure);

	private static CallResult<LobbyCreated_t> _lobbyCreatedCallResult;

	private static CallResult<LobbyMatchList_t> _lobbyMatchListCallResult;

	private static CallResult<LobbyEnter_t> _lobbyEnterCallResult;

	private static Callback<LobbyEnter_t> _lobbyEnterCallback;

	private static Callback<LobbyDataUpdate_t> _lobbyDataUpdateCallback;

	private static Callback<LobbyChatUpdate_t> _lobbyChatUpdateCallback;

	private static Callback<LobbyInvite_t> _lobbyInviteCallback;

	internal static LobbyCreatedCallResultDelegate OnLobbyCreated;

	internal static LobbyCreatedOKCallResultDelegate OnLobbyCreatedOK;

	internal static LobbyCreatedFailCallResultDelegate OnLobbyCreatedFail;

	internal static LobbyCreatedTimeoutCallResultDelegate OnLobbyCreatedTimeout;

	internal static LobbyCreatedLimitExceededCallResultDelegate OnLobbyCreatedLimitExceeded;

	internal static LobbyCreatedAccessDeniedCallResultDelegate OnLobbyCreatedAccessDenied;

	internal static LobbyCreatedNoConnectionCallResultDelegate OnLobbyCreatedNoConnection;

	internal static LobbyEnterCallResultDelegate OnLobbyEnterInitiated;

	internal static LobbyEnterSuccessCallResultDelegate OnLobbyEnterInitiatedSuccess;

	internal static LobbyEnterErrorCallResultDelegate OnLobbyEnterInitiatedError;

	internal static LobbyEnterCallbackDelegate OnLobbyEnterReceived;

	internal static LobbyEnterSuccessCallbackDelegate OnLobbyEnterReceivedSuccess;

	internal static LobbyEnterErrorCallbackDelegate OnLobbyEnterReceivedError;

	internal static LobbyDataUpdateCallbackDelegate OnLobbyDataUpdate;

	internal static LobbyDataUpdateLobbyCallbackDelegate OnLobbyDataUpdateLobby;

	internal static LobbyDataUpdateMemberCallbackDelegate OnLobbyDataUpdateMember;

	internal static LobbyChatUpdateCallbackDelegate OnLobbyChatUpdate;

	internal static LobbyChatUpdateMemberEnteredCallbackDelegate OnLobbyChatUpdateMemberEntered;

	internal static LobbyChatUpdateMemberLeftCallbackDelegate OnLobbyChatUpdateMemberLeft;

	internal static LobbyChatUpdateMemberDisconnectedCallbackDelegate OnLobbyChatUpdateMemberDisconnected;

	internal static LobbyChatUpdateMemberKickedCallbackDelegate OnLobbyChatUpdateMemberKicked;

	internal static LobbyChatUpdateMemberBannedCallbackDelegate OnLobbyChatUpdateMemberBanned;

	internal static LobbyInviteCallbackDelegate OnLobbyInvite;

	internal static LobbyLeaveDelegate OnLobbyLeave;

	internal static LobbyMatchListCallResultDelegate OnLobbyMatchList;

	internal static bool IsSetup { get; private set; }

	internal static void Setup()
	{
		SetupCallbacksAndCallResults();
	}

	private static void SetupCallbacksAndCallResults()
	{
		_lobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create((CallResult<LobbyCreated_t>.APIDispatchDelegate)OnLobbyCreatedCallResult);
		_lobbyEnterCallResult = CallResult<LobbyEnter_t>.Create((CallResult<LobbyEnter_t>.APIDispatchDelegate)OnLobbyEnterCallResult);
		_lobbyEnterCallback = Callback<LobbyEnter_t>.Create((Callback<LobbyEnter_t>.DispatchDelegate)OnLobbyEnterCallback);
		_lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create((Callback<LobbyDataUpdate_t>.DispatchDelegate)OnLobbyDataUpdateCallback);
		_lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create((Callback<LobbyChatUpdate_t>.DispatchDelegate)OnLobbyChatUpdateCallback);
		_lobbyInviteCallback = Callback<LobbyInvite_t>.Create((Callback<LobbyInvite_t>.DispatchDelegate)OnLobbyInviteCallback);
		_lobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create((CallResult<LobbyMatchList_t>.APIDispatchDelegate)OnLobbyMatchListCallResult);
	}

	internal static void Reset()
	{
		DisposeCallbacksAndCallResults();
	}

	private static void DisposeCallbacksAndCallResults()
	{
		_lobbyCreatedCallResult?.Dispose();
		_lobbyEnterCallResult?.Dispose();
		_lobbyEnterCallback?.Dispose();
		_lobbyDataUpdateCallback?.Dispose();
		_lobbyChatUpdateCallback?.Dispose();
		_lobbyInviteCallback?.Dispose();
		_lobbyMatchListCallResult?.Dispose();
		IsSetup = false;
	}

	internal static void SetCallResult<T>(SteamAPICall_t steamApiCallHandle) where T : struct
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == typeof(LobbyCreated_t))
		{
			_lobbyCreatedCallResult.Set(steamApiCallHandle, (CallResult<LobbyCreated_t>.APIDispatchDelegate)null);
		}
		else if (typeFromHandle == typeof(LobbyEnter_t))
		{
			_lobbyEnterCallResult.Set(steamApiCallHandle, (CallResult<LobbyEnter_t>.APIDispatchDelegate)null);
		}
		else if (typeFromHandle == typeof(LobbyMatchList_t))
		{
			_lobbyMatchListCallResult.Set(steamApiCallHandle, (CallResult<LobbyMatchList_t>.APIDispatchDelegate)null);
		}
	}

	private static void OnLobbyCreatedCallResult(LobbyCreated_t lobbyCreated_t, bool ioFailure)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected I4, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Invalid comparison between Unknown and I4
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Invalid comparison between Unknown and I4
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		EResult eResult = lobbyCreated_t.m_eResult;
		ulong ulSteamIDLobby = lobbyCreated_t.m_ulSteamIDLobby;
		CSteamID steamLobbyId = default(CSteamID);
		steamLobbyId = new CSteamID(ulSteamIDLobby);
		OnLobbyCreated?.Invoke(eResult, steamLobbyId, ioFailure);
		if (ioFailure)
		{
			SteamNetworkLobbyManager.LobbyTypeQueue?.Dequeue();
		}
		else if ((int)eResult <= 15)
		{
			switch ((int)eResult - 1)
			{
			case 0:
				OnLobbyCreatedOK?.Invoke(steamLobbyId);
				return;
			case 1:
				OnLobbyCreatedFail?.Invoke();
				return;
			case 2:
				OnLobbyCreatedNoConnection?.Invoke();
				return;
			}
			if ((int)eResult == 15)
			{
				OnLobbyCreatedAccessDenied?.Invoke();
			}
		}
		else if ((int)eResult != 16)
		{
			if ((int)eResult == 25)
			{
				OnLobbyCreatedLimitExceeded?.Invoke();
			}
		}
		else
		{
			OnLobbyCreatedTimeout?.Invoke();
		}
	}

	private static void OnLobbyEnterCallResult(LobbyEnter_t lobbyEnter_t, bool ioFailure)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		ulong ulSteamIDLobby = lobbyEnter_t.m_ulSteamIDLobby;
		CSteamID steamLobbyId = default(CSteamID);
		steamLobbyId = new CSteamID(ulSteamIDLobby);
		bool bLocked = lobbyEnter_t.m_bLocked;
		EChatRoomEnterResponse val = (EChatRoomEnterResponse)lobbyEnter_t.m_EChatRoomEnterResponse;
		OnLobbyEnterInitiated?.Invoke(steamLobbyId, bLocked, val, ioFailure);
		if (ioFailure)
		{
			return;
		}
		if ((int)val != 1)
		{
			if ((int)val == 5)
			{
				OnLobbyEnterInitiatedError?.Invoke(steamLobbyId, bLocked);
			}
		}
		else
		{
			OnLobbyEnterInitiatedSuccess?.Invoke(steamLobbyId, bLocked);
		}
	}

	private static void OnLobbyEnterCallback(LobbyEnter_t lobbyEnter_t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		ulong ulSteamIDLobby = lobbyEnter_t.m_ulSteamIDLobby;
		CSteamID steamLobbyId = default(CSteamID);
		steamLobbyId = new CSteamID(ulSteamIDLobby);
		bool bLocked = lobbyEnter_t.m_bLocked;
		EChatRoomEnterResponse val = (EChatRoomEnterResponse)lobbyEnter_t.m_EChatRoomEnterResponse;
		OnLobbyEnterReceived?.Invoke(steamLobbyId, bLocked, val);
		if ((int)val != 1)
		{
			if ((int)val == 5)
			{
				OnLobbyEnterReceivedError?.Invoke(steamLobbyId, bLocked);
			}
		}
		else
		{
			OnLobbyEnterReceivedSuccess?.Invoke(steamLobbyId, bLocked);
		}
	}

	private static void OnLobbyDataUpdateCallback(LobbyDataUpdate_t lobbyDataUpdate_t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		ulong ulSteamIDLobby = lobbyDataUpdate_t.m_ulSteamIDLobby;
		CSteamID steamLobbyId = default(CSteamID);
		steamLobbyId = new CSteamID(ulSteamIDLobby);
		ulong ulSteamIDMember = lobbyDataUpdate_t.m_ulSteamIDMember;
		CSteamID steamMemberId = default(CSteamID);
		steamMemberId = new CSteamID(ulSteamIDMember);
		bool flag = lobbyDataUpdate_t.m_bSuccess >= 1;
		OnLobbyDataUpdate?.Invoke(steamLobbyId, steamMemberId, flag);
		if (flag)
		{
			if (ulSteamIDLobby == ulSteamIDMember)
			{
				OnLobbyDataUpdateLobby?.Invoke(steamLobbyId);
			}
			else
			{
				OnLobbyDataUpdateMember?.Invoke(steamLobbyId, steamMemberId);
			}
		}
	}

	private static void OnLobbyChatUpdateCallback(LobbyChatUpdate_t lobbyChatUpdate_t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		ulong ulSteamIDLobby = lobbyChatUpdate_t.m_ulSteamIDLobby;
		CSteamID steamLobbyId = default(CSteamID);
		steamLobbyId = new CSteamID(ulSteamIDLobby);
		ulong ulSteamIDUserChanged = lobbyChatUpdate_t.m_ulSteamIDUserChanged;
		CSteamID steamUserIdRecipient = default(CSteamID);
		steamUserIdRecipient = new CSteamID(ulSteamIDUserChanged);
		ulong ulSteamIDMakingChange = lobbyChatUpdate_t.m_ulSteamIDMakingChange;
		CSteamID steamUserIdInitiator = default(CSteamID);
		steamUserIdInitiator = new CSteamID(ulSteamIDMakingChange);
		EChatMemberStateChange val = (EChatMemberStateChange)lobbyChatUpdate_t.m_rgfChatMemberStateChange;
		OnLobbyChatUpdate?.Invoke(steamLobbyId, steamUserIdRecipient, steamUserIdInitiator, val);
		if (((int)val & 1) != 0)
		{
			OnLobbyChatUpdateMemberEntered?.Invoke(steamLobbyId, steamUserIdRecipient, steamUserIdInitiator);
		}
		if (((int)val & 2) != 0)
		{
			OnLobbyChatUpdateMemberLeft?.Invoke(steamLobbyId, steamUserIdRecipient, steamUserIdInitiator);
		}
		if (((int)val & 4) != 0)
		{
			OnLobbyChatUpdateMemberDisconnected?.Invoke(steamLobbyId, steamUserIdRecipient, steamUserIdInitiator);
		}
		if (((int)val & 8) != 0)
		{
			OnLobbyChatUpdateMemberKicked?.Invoke(steamLobbyId, steamUserIdRecipient, steamUserIdInitiator);
		}
		if (((int)val & 0x10) != 0)
		{
			OnLobbyChatUpdateMemberBanned?.Invoke(steamLobbyId, steamUserIdRecipient, steamUserIdInitiator);
		}
	}

	private static void OnLobbyInviteCallback(LobbyInvite_t lobbyInvite_t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		ulong ulSteamIDUser = lobbyInvite_t.m_ulSteamIDUser;
		CSteamID steamUserIdInviter = default(CSteamID);
		steamUserIdInviter = new CSteamID(ulSteamIDUser);
		ulong ulSteamIDLobby = lobbyInvite_t.m_ulSteamIDLobby;
		CSteamID steamLobbyId = default(CSteamID);
		steamLobbyId = new CSteamID(ulSteamIDLobby);
		ulong ulGameID = lobbyInvite_t.m_ulGameID;
		CSteamID steamGameId = default(CSteamID);
		steamGameId = new CSteamID(ulGameID);
		OnLobbyInvite?.Invoke(steamUserIdInviter, steamLobbyId, steamGameId);
	}

	internal static void OnLobbyLeaveManual(CSteamID steamLobbyId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!(steamLobbyId == CSteamID.Nil))
		{
			OnLobbyLeave?.Invoke(steamLobbyId);
		}
	}

	private static void OnLobbyMatchListCallResult(LobbyMatchList_t pCallback, bool bIOFailure)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		int lobbiesMatching = (int)((!bIOFailure) ? pCallback.m_nLobbiesMatching : 0);
		OnLobbyMatchList?.Invoke(lobbiesMatching, bIOFailure);
	}
}
