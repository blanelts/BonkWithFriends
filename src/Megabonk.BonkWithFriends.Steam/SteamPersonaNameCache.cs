using System;
using System.Collections.Generic;
using MelonLoader;
using Steamworks;

namespace Megabonk.BonkWithFriends.Steam;

internal static class SteamPersonaNameCache
{
	private const int DefaultPersonaNameCacheSize = 16;

	private static Dictionary<CSteamID, string> _steamPersonaNames = new Dictionary<CSteamID, string>(16);

	private static CSteamID _currentSteamId;

	internal static void Setup()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		_currentSteamId = SteamUser.GetSteamID();
		SubscribeToCallbacksAndCallResults();
	}

	private static void SubscribeToCallbacksAndCallResults()
	{
		SteamFriendsImpl.OnPersonaStateChangeName = (SteamFriendsImpl.PersonaStateChangeNameCallbackDelegate)Delegate.Combine(SteamFriendsImpl.OnPersonaStateChangeName, new SteamFriendsImpl.PersonaStateChangeNameCallbackDelegate(OnChangeName));
		SteamFriendsImpl.OnPersonaStateChangeNameFirstSet = (SteamFriendsImpl.PersonaStateChangeNameFirstSetCallbackDelegate)Delegate.Combine(SteamFriendsImpl.OnPersonaStateChangeNameFirstSet, new SteamFriendsImpl.PersonaStateChangeNameFirstSetCallbackDelegate(OnChangeName));
		SteamFriendsImpl.OnPersonaStateChangeNickname = (SteamFriendsImpl.PersonaStateChangeNicknameCallbackDelegate)Delegate.Combine(SteamFriendsImpl.OnPersonaStateChangeNickname, new SteamFriendsImpl.PersonaStateChangeNicknameCallbackDelegate(OnChangeName));
	}

	private static void OnChangeName(CSteamID steamUserId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!(steamUserId == CSteamID.Nil))
		{
			string text = null;
			text = ((!(_currentSteamId == steamUserId)) ? SteamFriends.GetFriendPersonaName(steamUserId) : SteamFriends.GetPersonaName());
			if (!string.IsNullOrEmpty(text))
			{
				CachePersonaName(steamUserId, text);
			}
		}
	}

	internal static string GetOrRequestCachedName(CSteamID steamUserId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		string value = null;
		if (!_steamPersonaNames.TryGetValue(steamUserId, out value))
		{
			value = ((!(steamUserId == _currentSteamId)) ? SteamFriends.GetFriendPersonaName(steamUserId) : SteamFriends.GetPersonaName());
			if (!string.IsNullOrEmpty(value))
			{
				CachePersonaName(steamUserId, value);
			}
			else
			{
				RequestPersonaName(steamUserId);
			}
		}
		return value;
	}

	internal static void CachePersonaName(CSteamID steamUserId, string personaName)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		_steamPersonaNames[steamUserId] = personaName;
	}

	internal static string GetCachedName(CSteamID steamUserId)
	{
		return _steamPersonaNames.TryGetValue(steamUserId, out var name) ? name : null;
	}

	private static void RequestPersonaName(CSteamID steamUserId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!(steamUserId == _currentSteamId) && !SteamFriends.RequestUserInformation(steamUserId, true))
		{
			Melon<BonkWithFriendsMod>.Logger.Error("Failed to get to get friend persona name, but user information is already available..");
		}
	}

	internal static void Reset()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		UnsubscribeFromCallbacksAndCallResults();
		_currentSteamId = CSteamID.Nil;
		_steamPersonaNames.Clear();
	}

	private static void UnsubscribeFromCallbacksAndCallResults()
	{
		SteamFriendsImpl.OnPersonaStateChangeName = (SteamFriendsImpl.PersonaStateChangeNameCallbackDelegate)Delegate.Remove(SteamFriendsImpl.OnPersonaStateChangeName, new SteamFriendsImpl.PersonaStateChangeNameCallbackDelegate(OnChangeName));
		SteamFriendsImpl.OnPersonaStateChangeNameFirstSet = (SteamFriendsImpl.PersonaStateChangeNameFirstSetCallbackDelegate)Delegate.Remove(SteamFriendsImpl.OnPersonaStateChangeNameFirstSet, new SteamFriendsImpl.PersonaStateChangeNameFirstSetCallbackDelegate(OnChangeName));
		SteamFriendsImpl.OnPersonaStateChangeNickname = (SteamFriendsImpl.PersonaStateChangeNicknameCallbackDelegate)Delegate.Remove(SteamFriendsImpl.OnPersonaStateChangeNickname, new SteamFriendsImpl.PersonaStateChangeNicknameCallbackDelegate(OnChangeName));
	}
}
