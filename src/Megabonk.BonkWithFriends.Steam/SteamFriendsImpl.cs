using System;
using Steamworks;

namespace Megabonk.BonkWithFriends.Steam;

internal static class SteamFriendsImpl
{
	internal delegate void GameLobbyJoinRequestedCallbackDelegate(CSteamID steamLobbyId, CSteamID steamUserId);

	internal delegate void GameRichPresenceJoinRequestedCallbackDelegate(CSteamID steamUserId, string connect);

	internal delegate void AvatarImageLoadedCallbackDelegate(CSteamID steamUserId, int imageHandle, int width, int height);

	internal delegate void PersonaStateChangeCallbackDelegate(CSteamID steamUserId, EPersonaChange flags);

	internal delegate void PersonaStateChangeNameCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeStatusCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeComeOnlineCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeGoneOfflineCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeGamePlayedCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeGameServerCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeAvatarCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeJoinedSourceCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeLeftSourceCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeRelationshipChangedCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeNameFirstSetCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeBroadcastCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeNicknameCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeSteamLevelCallbackDelegate(CSteamID steamUserId);

	internal delegate void PersonaStateChangeRichPresenceCallbackDelegate(CSteamID steamUserId);

	private static Callback<GameLobbyJoinRequested_t> _gameLobbyJoinRequestedCallback;

	private static Callback<GameRichPresenceJoinRequested_t> _gameRichPresenceJoinRequestedCallback;

	private static Callback<PersonaStateChange_t> _personaStateChangeCallback;

	private static Callback<AvatarImageLoaded_t> _avatarImageLoadedCallback;

	internal static GameLobbyJoinRequestedCallbackDelegate OnGameLobbyJoinRequested;

	internal static GameRichPresenceJoinRequestedCallbackDelegate OnGameRichPresenceJoinRequested;

	internal static AvatarImageLoadedCallbackDelegate OnAvatarImageLoaded;

	internal static PersonaStateChangeCallbackDelegate OnPersonaStateChange;

	internal static PersonaStateChangeNameCallbackDelegate OnPersonaStateChangeName;

	internal static PersonaStateChangeStatusCallbackDelegate OnPersonaStateChangeStatus;

	internal static PersonaStateChangeComeOnlineCallbackDelegate OnPersonaStateChangeComeOnline;

	internal static PersonaStateChangeGoneOfflineCallbackDelegate OnPersonaStateChangeGoneOffline;

	internal static PersonaStateChangeGamePlayedCallbackDelegate OnPersonaStateChangeGamePlayed;

	internal static PersonaStateChangeGameServerCallbackDelegate OnPersonaStateChangeGameServer;

	internal static PersonaStateChangeAvatarCallbackDelegate OnPersonaStateChangeAvatar;

	internal static PersonaStateChangeJoinedSourceCallbackDelegate OnPersonaStateChangeJoinedSource;

	internal static PersonaStateChangeLeftSourceCallbackDelegate OnPersonaStateChangeLeftSource;

	internal static PersonaStateChangeRelationshipChangedCallbackDelegate OnPersonaStateChangeRelationshipChanged;

	internal static PersonaStateChangeNameFirstSetCallbackDelegate OnPersonaStateChangeNameFirstSet;

	internal static PersonaStateChangeBroadcastCallbackDelegate OnPersonaStateChangeBroadcast;

	internal static PersonaStateChangeNicknameCallbackDelegate OnPersonaStateChangeNickname;

	internal static PersonaStateChangeSteamLevelCallbackDelegate OnPersonaStateChangeSteamLevel;

	internal static PersonaStateChangeRichPresenceCallbackDelegate OnPersonaStateChangeRichPresence;

	internal static bool IsSetup { get; private set; }

	internal static void Setup()
	{
		SetupCallbacksAndCallResults();
	}

	private static void SetupCallbacksAndCallResults()
	{
		_gameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create((Callback<GameLobbyJoinRequested_t>.DispatchDelegate)OnGameLobbyJoinRequestedCallback);
		_gameRichPresenceJoinRequestedCallback = Callback<GameRichPresenceJoinRequested_t>.Create((Callback<GameRichPresenceJoinRequested_t>.DispatchDelegate)OnGameRichPresenceJoinRequestedCallback);
		_personaStateChangeCallback = Callback<PersonaStateChange_t>.Create((Callback<PersonaStateChange_t>.DispatchDelegate)OnPersonaStateChangeCallback);
		_avatarImageLoadedCallback = Callback<AvatarImageLoaded_t>.Create((Callback<AvatarImageLoaded_t>.DispatchDelegate)OnAvatarImageLoadedCallback);
		IsSetup = true;
	}

	internal static void Reset()
	{
		DisposeCallbacksAndCallResults();
	}

	private static void DisposeCallbacksAndCallResults()
	{
		_gameLobbyJoinRequestedCallback?.Dispose();
		_gameRichPresenceJoinRequestedCallback?.Dispose();
		_personaStateChangeCallback?.Dispose();
		_avatarImageLoadedCallback?.Dispose();
		IsSetup = false;
	}

	private static void OnGameLobbyJoinRequestedCallback(GameLobbyJoinRequested_t gameLobbyJoinRequested_t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		CSteamID steamIDLobby = gameLobbyJoinRequested_t.m_steamIDLobby;
		CSteamID steamIDFriend = gameLobbyJoinRequested_t.m_steamIDFriend;
		OnGameLobbyJoinRequested?.Invoke(steamIDLobby, steamIDFriend);
	}

	private static void OnGameRichPresenceJoinRequestedCallback(GameRichPresenceJoinRequested_t gameRichPresenceJoinRequested_t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		CSteamID steamIDFriend = gameRichPresenceJoinRequested_t.m_steamIDFriend;
		string rgchConnect = gameRichPresenceJoinRequested_t.m_rgchConnect;
		OnGameRichPresenceJoinRequested?.Invoke(steamIDFriend, rgchConnect);
	}

	private static void OnAvatarImageLoadedCallback(AvatarImageLoaded_t avatarImageLoaded_t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		CSteamID steamID = avatarImageLoaded_t.m_steamID;
		int iImage = avatarImageLoaded_t.m_iImage;
		int iWide = avatarImageLoaded_t.m_iWide;
		int iTall = avatarImageLoaded_t.m_iTall;
		if (steamID == CSteamID.Nil || iWide <= 0 || iTall <= 0)
		{
			throw new ArgumentException("avatarImageLoaded_t");
		}
		OnAvatarImageLoaded?.Invoke(steamID, iImage, iWide, iTall);
	}

	private static void OnPersonaStateChangeCallback(PersonaStateChange_t personaStateChange_t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		ulong ulSteamID = personaStateChange_t.m_ulSteamID;
		CSteamID steamUserId = default(CSteamID);
		steamUserId = new CSteamID(ulSteamID);
		EPersonaChange nChangeFlags = personaStateChange_t.m_nChangeFlags;
		OnPersonaStateChange?.Invoke(steamUserId, nChangeFlags);
		if (((int)nChangeFlags & 1) != 0)
		{
			OnPersonaStateChangeName?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 2) != 0)
		{
			OnPersonaStateChangeStatus?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 4) != 0)
		{
			OnPersonaStateChangeComeOnline?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 8) != 0)
		{
			OnPersonaStateChangeGoneOffline?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 0x10) != 0)
		{
			OnPersonaStateChangeGamePlayed?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 0x20) != 0)
		{
			OnPersonaStateChangeGameServer?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 0x40) != 0)
		{
			OnPersonaStateChangeAvatar?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 0x80) != 0)
		{
			OnPersonaStateChangeJoinedSource?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 0x100) != 0)
		{
			OnPersonaStateChangeLeftSource?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 0x200) != 0)
		{
			OnPersonaStateChangeRelationshipChanged?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 0x400) != 0)
		{
			OnPersonaStateChangeNameFirstSet?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 0x800) != 0)
		{
			OnPersonaStateChangeBroadcast?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 0x1000) != 0)
		{
			OnPersonaStateChangeNickname?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 0x2000) != 0)
		{
			OnPersonaStateChangeSteamLevel?.Invoke(steamUserId);
		}
		if (((int)nChangeFlags & 0x4000) != 0)
		{
			OnPersonaStateChangeRichPresence?.Invoke(steamUserId);
		}
	}
}
