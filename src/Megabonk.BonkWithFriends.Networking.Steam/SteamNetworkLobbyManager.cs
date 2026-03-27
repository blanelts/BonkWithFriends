using System;
using System.Collections;
using System.Collections.Generic;
using Megabonk.BonkWithFriends.Steam;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Networking.Steam;

internal static class SteamNetworkLobbyManager
{
	private const int CMaxPlayers = 4;

	private const float LOBBY_SEARCH_TIMEOUT = 10f;

	private static object _searchTimeoutCoroutine;

	internal static int MaxPlayers => Preferences.MaxPlayers.Value;

	internal static SteamNetworkLobbyState State { get; private set; }

	internal static Queue<SteamNetworkLobbyType> LobbyTypeQueue { get; private set; }

	static SteamNetworkLobbyManager()
	{
		LobbyTypeQueue = new Queue<SteamNetworkLobbyType>();
		SubscribeToCallbacksAndCallResults();
	}

	private static void SubscribeToCallbacksAndCallResults()
	{
		SteamMatchmakingImpl.OnLobbyCreatedOK = (SteamMatchmakingImpl.LobbyCreatedOKCallResultDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyCreatedOK, new SteamMatchmakingImpl.LobbyCreatedOKCallResultDelegate(OnLobbyCreatedOK));
		SteamMatchmakingImpl.OnLobbyCreatedFail = (SteamMatchmakingImpl.LobbyCreatedFailCallResultDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyCreatedFail, new SteamMatchmakingImpl.LobbyCreatedFailCallResultDelegate(OnLobbyCreatedFail));
		SteamMatchmakingImpl.OnLobbyCreatedAccessDenied = (SteamMatchmakingImpl.LobbyCreatedAccessDeniedCallResultDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyCreatedAccessDenied, new SteamMatchmakingImpl.LobbyCreatedAccessDeniedCallResultDelegate(OnLobbyCreatedFail));
		SteamMatchmakingImpl.OnLobbyCreatedLimitExceeded = (SteamMatchmakingImpl.LobbyCreatedLimitExceededCallResultDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyCreatedLimitExceeded, new SteamMatchmakingImpl.LobbyCreatedLimitExceededCallResultDelegate(OnLobbyCreatedFail));
		SteamMatchmakingImpl.OnLobbyCreatedNoConnection = (SteamMatchmakingImpl.LobbyCreatedNoConnectionCallResultDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyCreatedNoConnection, new SteamMatchmakingImpl.LobbyCreatedNoConnectionCallResultDelegate(OnLobbyCreatedFail));
		SteamMatchmakingImpl.OnLobbyCreatedTimeout = (SteamMatchmakingImpl.LobbyCreatedTimeoutCallResultDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyCreatedTimeout, new SteamMatchmakingImpl.LobbyCreatedTimeoutCallResultDelegate(OnLobbyCreatedFail));
		SteamMatchmakingImpl.OnLobbyEnterInitiatedSuccess = (SteamMatchmakingImpl.LobbyEnterSuccessCallResultDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyEnterInitiatedSuccess, new SteamMatchmakingImpl.LobbyEnterSuccessCallResultDelegate(OnLobbyEnterInitiatedSuccess));
		SteamMatchmakingImpl.OnLobbyEnterInitiatedError = (SteamMatchmakingImpl.LobbyEnterErrorCallResultDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyEnterInitiatedError, new SteamMatchmakingImpl.LobbyEnterErrorCallResultDelegate(OnLobbyEnterInitiatedError));
		SteamMatchmakingImpl.OnLobbyEnterReceivedSuccess = (SteamMatchmakingImpl.LobbyEnterSuccessCallbackDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyEnterReceivedSuccess, new SteamMatchmakingImpl.LobbyEnterSuccessCallbackDelegate(OnLobbyEnterReceivedSuccess));
		SteamMatchmakingImpl.OnLobbyEnterReceivedError = (SteamMatchmakingImpl.LobbyEnterErrorCallbackDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyEnterReceivedError, new SteamMatchmakingImpl.LobbyEnterErrorCallbackDelegate(OnLobbyEnterReceivedError));
		SteamMatchmakingImpl.OnLobbyInvite = (SteamMatchmakingImpl.LobbyInviteCallbackDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyInvite, new SteamMatchmakingImpl.LobbyInviteCallbackDelegate(OnLobbyInvite));
		SteamFriendsImpl.OnGameLobbyJoinRequested = (SteamFriendsImpl.GameLobbyJoinRequestedCallbackDelegate)Delegate.Combine(SteamFriendsImpl.OnGameLobbyJoinRequested, new SteamFriendsImpl.GameLobbyJoinRequestedCallbackDelegate(OnGameLobbyJoinRequested));
		SteamFriendsImpl.OnGameRichPresenceJoinRequested = (SteamFriendsImpl.GameRichPresenceJoinRequestedCallbackDelegate)Delegate.Combine(SteamFriendsImpl.OnGameRichPresenceJoinRequested, new SteamFriendsImpl.GameRichPresenceJoinRequestedCallbackDelegate(OnGameRichPresenceJoinRequested));
		SteamMatchmakingImpl.OnLobbyLeave = (SteamMatchmakingImpl.LobbyLeaveDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyLeave, new SteamMatchmakingImpl.LobbyLeaveDelegate(OnLobbyLeave));
		SteamMatchmakingImpl.OnLobbyMatchList = (SteamMatchmakingImpl.LobbyMatchListCallResultDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyMatchList, new SteamMatchmakingImpl.LobbyMatchListCallResultDelegate(OnLobbyMatchList));
	}

	private static void OnLobbyCreatedOK(CSteamID steamLobbyId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (SteamManager.Instance.Lobby != null)
		{
			LeaveLobby();
		}
		SteamManager.Instance.Lobby = new SteamNetworkLobby(steamLobbyId, created: true)
		{
			LobbyType = LobbyTypeQueue.Dequeue()
		};
		Melon<BonkWithFriendsMod>.Logger.Msg($"[{"OnLobbyCreatedOK"}] Created lobby: {steamLobbyId}");
		SetState(SteamNetworkLobbyState.Joined);
	}

	private static void OnLobbyCreatedFail()
	{
		Melon<BonkWithFriendsMod>.Logger.Error("[OnLobbyCreatedFail] Failed to create lobby!");
		SetState(SteamNetworkLobbyState.None);
		LobbyTypeQueue.Dequeue();
	}

	private static void OnLobbyEnterInitiatedSuccess(CSteamID steamLobbyId, bool lobbyLocked)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (SteamManager.Instance.Lobby != null)
		{
			LeaveLobby();
		}
		SteamManager.Instance.Lobby = new SteamNetworkLobby(steamLobbyId, created: false);
		Melon<BonkWithFriendsMod>.Logger.Msg($"[{"OnLobbyEnterInitiatedSuccess"}] Joined lobby: {steamLobbyId}");
		SetState(SteamNetworkLobbyState.Joined);
	}

	private static void OnLobbyEnterInitiatedError(CSteamID steamLobbyId, bool lobbyLocked)
	{
		Melon<BonkWithFriendsMod>.Logger.Error("[OnLobbyEnterInitiatedError] Failed to join lobby!");
		SetState(SteamNetworkLobbyState.None);
	}

	private static void OnLobbyEnterReceivedSuccess(CSteamID steamLobbyId, bool lobbyLocked)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Melon<BonkWithFriendsMod>.Logger.Msg($"[{"OnLobbyEnterReceivedSuccess"}] Joined lobby: {steamLobbyId}");
		SetState(SteamNetworkLobbyState.Joined);
	}

	private static void OnLobbyEnterReceivedError(CSteamID steamLobbyId, bool lobbyLocked)
	{
		Melon<BonkWithFriendsMod>.Logger.Error("[OnLobbyEnterReceivedError] Failed to join (our own) lobby!");
		SetState(SteamNetworkLobbyState.None);
	}

	private static void OnLobbyInvite(CSteamID steamUserIdInviter, CSteamID steamLobbyId, CSteamID steamGameId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!(steamUserIdInviter == CSteamID.Nil) && !(steamLobbyId == CSteamID.Nil))
		{
			Melon<BonkWithFriendsMod>.Logger.Msg(new string('=', 20) ?? "");
			Melon<BonkWithFriendsMod>.Logger.Msg($"[{"OnLobbyInvite"}] Invite received from: {SteamPersonaNameCache.GetOrRequestCachedName(steamUserIdInviter)} ({steamUserIdInviter})");
			Melon<BonkWithFriendsMod>.Logger.Msg(new string('=', 20) ?? "");
			SetState(SteamNetworkLobbyState.InvitePending);
		}
	}

	private static void OnGameLobbyJoinRequested(CSteamID steamLobbyId, CSteamID steamUserId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (!(steamLobbyId == CSteamID.Nil) && !(steamUserId == CSteamID.Nil))
		{
			if (SteamManager.Instance.Lobby != null)
			{
				LeaveLobby();
			}
			Melon<BonkWithFriendsMod>.Logger.Msg($"[{"OnGameLobbyJoinRequested"}] Joining lobby \"{steamLobbyId}\" from {SteamPersonaNameCache.GetOrRequestCachedName(steamUserId)} ({steamUserId})");
			JoinLobby(steamLobbyId);
		}
	}

	private static void OnGameRichPresenceJoinRequested(CSteamID steamUserId, string connect)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		if (steamUserId == CSteamID.Nil || string.IsNullOrWhiteSpace(connect))
		{
			return;
		}
		string[] array = connect.Split(' ');
		if (array == null || array.Length != 2)
		{
			return;
		}
		_ = array[0];
		if (!ulong.TryParse(array[1], out var result))
		{
			return;
		}
		CSteamID val = default(CSteamID);
		val = new CSteamID(result);
		if (!(val == CSteamID.Nil))
		{
			if (SteamManager.Instance.Lobby != null)
			{
				LeaveLobby();
			}
			Melon<BonkWithFriendsMod>.Logger.Msg($"[{"OnGameRichPresenceJoinRequested"}] Joining connect string \"{connect}\" from {SteamPersonaNameCache.GetOrRequestCachedName(steamUserId)} ({steamUserId})");
			JoinLobby(val);
		}
	}

	private static void OnLobbyLeave(CSteamID steamLobbyId)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		SteamManager.Instance.Lobby?.Reset();
		Melon<BonkWithFriendsMod>.Logger.Msg($"[{"OnLobbyLeave"}] Leaving lobby \"{steamLobbyId}\"");
		SteamManager.Instance.Lobby = null;
		SetState(SteamNetworkLobbyState.None);
	}

	internal static void CreateLobby(SteamNetworkLobbyType lobbyType = SteamNetworkLobbyType.Public, int? maxPlayers = null)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		int num = maxPlayers ?? Preferences.MaxPlayers.Value;
		LobbyTypeQueue.Enqueue(lobbyType);
		Melon<BonkWithFriendsMod>.Logger.Msg($"[{"CreateLobby"}] Creating lobby.. (T:{lobbyType}, MM:{num})");
		SteamMatchmakingImpl.SetCallResult<LobbyCreated_t>(SteamMatchmaking.CreateLobby((ELobbyType)lobbyType, num));
		SetState(SteamNetworkLobbyState.Joining);
	}

	internal static void FindLobby()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if (State != SteamNetworkLobbyState.None)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[{"FindLobby"}] Cannot search while busy (State: {State})");
			return;
		}
		Melon<BonkWithFriendsMod>.Logger.Msg("[FindLobby] searching for public lobbies...");
		SetState(SteamNetworkLobbyState.Searching);
		SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1);
		SteamMatchmaking.AddRequestLobbyListDistanceFilter((ELobbyDistanceFilter)3);
		SteamMatchmakingImpl.SetCallResult<LobbyMatchList_t>(SteamMatchmaking.RequestLobbyList());
		if (_searchTimeoutCoroutine != null)
		{
			MelonCoroutines.Stop(_searchTimeoutCoroutine);
		}
		_searchTimeoutCoroutine = MelonCoroutines.Start(LobbySearchTimeout());
	}

	private static void OnLobbyMatchList(int count, bool ioFailure)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (State == SteamNetworkLobbyState.Searching)
		{
			if (_searchTimeoutCoroutine != null)
			{
				MelonCoroutines.Stop(_searchTimeoutCoroutine);
				_searchTimeoutCoroutine = null;
			}
			if (ioFailure)
			{
				Melon<BonkWithFriendsMod>.Logger.Error("[OnLobbyMatchList] IO Failure while searching.");
				SetState(SteamNetworkLobbyState.None);
				return;
			}
			if (count == 0)
			{
				Melon<BonkWithFriendsMod>.Logger.Msg("[OnLobbyMatchList] No lobbies found.");
				SetState(SteamNetworkLobbyState.None);
				CreateLobby();
				return;
			}
			Melon<BonkWithFriendsMod>.Logger.Msg($"[{"OnLobbyMatchList"}] Found {count} lobbies. Joining the best one.");
			JoinLobby(SteamMatchmaking.GetLobbyByIndex(0));
		}
	}

	private static IEnumerator LobbySearchTimeout()
	{
		for (float elapsed = 0f; elapsed < 10f; elapsed += Time.unscaledDeltaTime)
		{
			yield return null;
		}
		if (State == SteamNetworkLobbyState.Searching)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[{"LobbySearchTimeout"}] Lobby search timed out after {10f}s. Creating new lobby instead.");
			SetState(SteamNetworkLobbyState.None);
		}
		_searchTimeoutCoroutine = null;
	}

	internal static void JoinLobby(CSteamID steamLobbyId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (steamLobbyId == CSteamID.Nil)
		{
			throw new ArgumentNullException("steamLobbyId");
		}
		Melon<BonkWithFriendsMod>.Logger.Msg($"[{"CreateLobby"}] Joining lobby \"{steamLobbyId}\"..");
		SteamMatchmakingImpl.SetCallResult<LobbyEnter_t>(SteamMatchmaking.JoinLobby(steamLobbyId));
		SetState(SteamNetworkLobbyState.Joining);
	}

	internal static void LeaveLobby()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		SteamNetworkLobby lobby = SteamManager.Instance.Lobby;
		if (lobby != null)
		{
			CSteamID lobbyId = lobby.LobbyId;
			if (lobbyId == CSteamID.Nil)
			{
				throw new NullReferenceException("currentLobbyId");
			}
			Melon<BonkWithFriendsMod>.Logger.Msg("[CreateLobby] Leaving lobby..");
			SteamMatchmaking.LeaveLobby(lobbyId);
			SetState(SteamNetworkLobbyState.Leaving);
			SteamMatchmakingImpl.OnLobbyLeaveManual(lobbyId);
		}
	}

	internal static void OpenInviteDialog()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		SteamNetworkLobby lobby = SteamManager.Instance.Lobby;
		if (lobby != null)
		{
			CSteamID lobbyId = lobby.LobbyId;
			if (lobbyId == CSteamID.Nil)
			{
				throw new NullReferenceException("currentLobbyId");
			}
			SteamFriends.ActivateGameOverlayInviteDialog(lobbyId);
		}
	}

	private static void SetState(SteamNetworkLobbyState steamNetworkLobbyState)
	{
		State = steamNetworkLobbyState;
	}

	internal static void Reset()
	{
		UnsubscribeFromCallbacksAndCallResults();
	}

	private static void UnsubscribeFromCallbacksAndCallResults()
	{
		SteamMatchmakingImpl.OnLobbyCreatedOK = (SteamMatchmakingImpl.LobbyCreatedOKCallResultDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyCreatedOK, new SteamMatchmakingImpl.LobbyCreatedOKCallResultDelegate(OnLobbyCreatedOK));
		SteamMatchmakingImpl.OnLobbyCreatedFail = (SteamMatchmakingImpl.LobbyCreatedFailCallResultDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyCreatedFail, new SteamMatchmakingImpl.LobbyCreatedFailCallResultDelegate(OnLobbyCreatedFail));
		SteamMatchmakingImpl.OnLobbyCreatedAccessDenied = (SteamMatchmakingImpl.LobbyCreatedAccessDeniedCallResultDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyCreatedAccessDenied, new SteamMatchmakingImpl.LobbyCreatedAccessDeniedCallResultDelegate(OnLobbyCreatedFail));
		SteamMatchmakingImpl.OnLobbyCreatedLimitExceeded = (SteamMatchmakingImpl.LobbyCreatedLimitExceededCallResultDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyCreatedLimitExceeded, new SteamMatchmakingImpl.LobbyCreatedLimitExceededCallResultDelegate(OnLobbyCreatedFail));
		SteamMatchmakingImpl.OnLobbyCreatedNoConnection = (SteamMatchmakingImpl.LobbyCreatedNoConnectionCallResultDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyCreatedNoConnection, new SteamMatchmakingImpl.LobbyCreatedNoConnectionCallResultDelegate(OnLobbyCreatedFail));
		SteamMatchmakingImpl.OnLobbyCreatedTimeout = (SteamMatchmakingImpl.LobbyCreatedTimeoutCallResultDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyCreatedTimeout, new SteamMatchmakingImpl.LobbyCreatedTimeoutCallResultDelegate(OnLobbyCreatedFail));
		SteamMatchmakingImpl.OnLobbyEnterInitiatedSuccess = (SteamMatchmakingImpl.LobbyEnterSuccessCallResultDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyEnterInitiatedSuccess, new SteamMatchmakingImpl.LobbyEnterSuccessCallResultDelegate(OnLobbyEnterInitiatedSuccess));
		SteamMatchmakingImpl.OnLobbyEnterInitiatedError = (SteamMatchmakingImpl.LobbyEnterErrorCallResultDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyEnterInitiatedError, new SteamMatchmakingImpl.LobbyEnterErrorCallResultDelegate(OnLobbyEnterInitiatedError));
		SteamMatchmakingImpl.OnLobbyEnterReceivedSuccess = (SteamMatchmakingImpl.LobbyEnterSuccessCallbackDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyEnterReceivedSuccess, new SteamMatchmakingImpl.LobbyEnterSuccessCallbackDelegate(OnLobbyEnterReceivedSuccess));
		SteamMatchmakingImpl.OnLobbyEnterReceivedError = (SteamMatchmakingImpl.LobbyEnterErrorCallbackDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyEnterReceivedError, new SteamMatchmakingImpl.LobbyEnterErrorCallbackDelegate(OnLobbyEnterReceivedError));
		SteamFriendsImpl.OnGameLobbyJoinRequested = (SteamFriendsImpl.GameLobbyJoinRequestedCallbackDelegate)Delegate.Remove(SteamFriendsImpl.OnGameLobbyJoinRequested, new SteamFriendsImpl.GameLobbyJoinRequestedCallbackDelegate(OnGameLobbyJoinRequested));
		SteamFriendsImpl.OnGameRichPresenceJoinRequested = (SteamFriendsImpl.GameRichPresenceJoinRequestedCallbackDelegate)Delegate.Remove(SteamFriendsImpl.OnGameRichPresenceJoinRequested, new SteamFriendsImpl.GameRichPresenceJoinRequestedCallbackDelegate(OnGameRichPresenceJoinRequested));
		SteamMatchmakingImpl.OnLobbyLeave = (SteamMatchmakingImpl.LobbyLeaveDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyLeave, new SteamMatchmakingImpl.LobbyLeaveDelegate(OnLobbyLeave));
		SteamMatchmakingImpl.OnLobbyMatchList = (SteamMatchmakingImpl.LobbyMatchListCallResultDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyMatchList, new SteamMatchmakingImpl.LobbyMatchListCallResultDelegate(OnLobbyMatchList));
	}
}
