using System;
using System.Collections.Generic;
using System.Linq;
using Il2Cpp;
using Il2CppAssets.Scripts._Data;
using Il2CppAssets.Scripts._Data.MapsAndStages;
using Megabonk.BonkWithFriends.Managers;
using Megabonk.BonkWithFriends.Steam;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Networking.Steam;

internal sealed class SteamNetworkLobby
{
	private const string LobbyDataNameKey = "name";

	private const string LobbyDataMapKey = "map";

	private const string LobbyDataTierKey = "tier";

	private const string LobbyDataSeedKey = "seed";

	private const string LobbyDataChallengeKey = "challenge";

	private const string LobbyDataServerReadyKey = "serverReady";

	private const string LobbyDataBotsKey = "bots";

	private const string LobbyMemberDataReadyKey = "ready";

	private const string LobbyMemberDataCharacterKey = "character";

	private const string LobbyMemberDataSkinTypeKey = "skinType";

	internal static SteamNetworkLobby Instance;

	private readonly object _syncRoot = new object();

	private readonly bool _created;

	private bool _firstLobbyUpdate;

	private List<SteamNetworkLobbyMember> _members;

	internal ServerReadyChangedDelegate OnServerReadyChanged;

	internal CSteamID LobbyId { get; private set; }

	internal CSteamID LobbyOwnerUserId { get; private set; }

	internal SteamNetworkLobbyType LobbyType { get; set; } = SteamNetworkLobbyType.Unknown;

	internal IReadOnlyList<SteamNetworkLobbyMember> Members => _members;

	internal int MemberCount => _members.Count;

	internal int MaxMembers { get; private set; }

	internal string Name { get; private set; }

	internal EMap Map { get; private set; }

	internal int Tier { get; private set; }

	internal int Seed { get; private set; }

	internal ChallengeData Challenge { get; private set; }

	internal bool ServerReady { get; private set; }

	internal SteamNetworkLobby(CSteamID steamLobbyId, bool created)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (Instance != null)
		{
			throw new InvalidOperationException("Instance");
		}
		if (steamLobbyId == CSteamID.Nil)
		{
			throw new ArgumentNullException("steamLobbyId");
		}
		Instance = this;
		_created = created;
		LobbyId = steamLobbyId;
		Setup();
	}

	private void Setup()
	{
		_firstLobbyUpdate = false;
		lock (_syncRoot)
		{
			_members = new List<SteamNetworkLobbyMember>(MaxMembers);
		}
		SubscribeToCallbacksAndCallResults();
	}

	private void SubscribeToCallbacksAndCallResults()
	{
		SteamMatchmakingImpl.OnLobbyChatUpdateMemberEntered = (SteamMatchmakingImpl.LobbyChatUpdateMemberEnteredCallbackDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyChatUpdateMemberEntered, new SteamMatchmakingImpl.LobbyChatUpdateMemberEnteredCallbackDelegate(OnLobbyChatUpdateMemberEntered));
		SteamMatchmakingImpl.OnLobbyChatUpdateMemberLeft = (SteamMatchmakingImpl.LobbyChatUpdateMemberLeftCallbackDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyChatUpdateMemberLeft, new SteamMatchmakingImpl.LobbyChatUpdateMemberLeftCallbackDelegate(OnLobbyChatUpdateMemberLeft));
		SteamMatchmakingImpl.OnLobbyChatUpdateMemberDisconnected = (SteamMatchmakingImpl.LobbyChatUpdateMemberDisconnectedCallbackDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyChatUpdateMemberDisconnected, new SteamMatchmakingImpl.LobbyChatUpdateMemberDisconnectedCallbackDelegate(OnLobbyChatUpdateMemberDisconnected));
		SteamMatchmakingImpl.OnLobbyChatUpdateMemberKicked = (SteamMatchmakingImpl.LobbyChatUpdateMemberKickedCallbackDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyChatUpdateMemberKicked, new SteamMatchmakingImpl.LobbyChatUpdateMemberKickedCallbackDelegate(OnLobbyChatUpdateMemberKicked));
		SteamMatchmakingImpl.OnLobbyChatUpdateMemberBanned = (SteamMatchmakingImpl.LobbyChatUpdateMemberBannedCallbackDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyChatUpdateMemberBanned, new SteamMatchmakingImpl.LobbyChatUpdateMemberBannedCallbackDelegate(OnLobbyChatUpdateMemberBanned));
		SteamMatchmakingImpl.OnLobbyDataUpdateLobby = (SteamMatchmakingImpl.LobbyDataUpdateLobbyCallbackDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyDataUpdateLobby, new SteamMatchmakingImpl.LobbyDataUpdateLobbyCallbackDelegate(OnLobbyDataUpdateLobby));
		SteamMatchmakingImpl.OnLobbyDataUpdateMember = (SteamMatchmakingImpl.LobbyDataUpdateMemberCallbackDelegate)Delegate.Combine(SteamMatchmakingImpl.OnLobbyDataUpdateMember, new SteamMatchmakingImpl.LobbyDataUpdateMemberCallbackDelegate(OnLobbyDataUpdateMember));
	}

	internal bool OwnedByUs()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return LobbyOwnerUserId == SteamUser.GetSteamID();
	}

	private bool IsThisLobby(CSteamID steamLobbyId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (LobbyId != CSteamID.Nil && steamLobbyId != CSteamID.Nil)
		{
			return LobbyId == steamLobbyId;
		}
		return false;
	}

	internal bool HasMember(CSteamID steamUserId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetMember(steamUserId) != null;
	}

	internal SteamNetworkLobbyMember GetMember(CSteamID steamUserId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (steamUserId == CSteamID.Nil)
		{
			throw new ArgumentNullException("steamUserId");
		}
		lock (_syncRoot)
		{
			return _members.FirstOrDefault((SteamNetworkLobbyMember snlm) => snlm.UserId == steamUserId);
		}
	}

	private void AddMember(CSteamID steamUserId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (steamUserId == CSteamID.Nil)
		{
			throw new ArgumentNullException("steamUserId");
		}
		if (HasMember(steamUserId))
		{
			throw new ArgumentOutOfRangeException("steamUserId");
		}
		SteamNetworkLobbyMember item = new SteamNetworkLobbyMember(LobbyId, steamUserId);
		lock (_syncRoot)
		{
			_members.Add(item);
		}
	}

	private void RemoveMember(CSteamID steamUserId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (steamUserId == CSteamID.Nil)
		{
			throw new ArgumentNullException("steamUserId");
		}
		lock (_syncRoot)
		{
			SteamNetworkLobbyMember steamNetworkLobbyMember = _members.FirstOrDefault((SteamNetworkLobbyMember snlm) => snlm.UserId == steamUserId);
			if (steamNetworkLobbyMember != null)
			{
				_members.Remove(steamNetworkLobbyMember);
			}
		}
	}

	private void OnLobbyChatUpdateMemberEntered(CSteamID steamLobbyId, CSteamID steamUserIdRecipient, CSteamID steamUserIdInitiator)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (IsThisLobby(steamLobbyId) && !(steamUserIdRecipient == CSteamID.Nil))
		{
			AddMember(steamUserIdRecipient);
		}
	}

	private void OnLobbyChatUpdateMemberLeft(CSteamID steamLobbyId, CSteamID steamUserIdRecipient, CSteamID steamUserIdInitiator)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (IsThisLobby(steamLobbyId) && !(steamUserIdRecipient == CSteamID.Nil))
		{
			RemoveMember(steamUserIdRecipient);
		}
	}

	private void OnLobbyChatUpdateMemberDisconnected(CSteamID steamLobbyId, CSteamID steamUserIdRecipient, CSteamID steamUserIdInitiator)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (IsThisLobby(steamLobbyId) && !(steamUserIdRecipient == CSteamID.Nil))
		{
			RemoveMember(steamUserIdRecipient);
		}
	}

	private void OnLobbyChatUpdateMemberKicked(CSteamID steamLobbyId, CSteamID steamUserIdRecipient, CSteamID steamUserIdInitiator)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (IsThisLobby(steamLobbyId) && !(steamUserIdRecipient == CSteamID.Nil))
		{
			RemoveMember(steamUserIdRecipient);
		}
	}

	private void OnLobbyChatUpdateMemberBanned(CSteamID steamLobbyId, CSteamID steamUserIdRecipient, CSteamID steamUserIdInitiator)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (IsThisLobby(steamLobbyId) && !(steamUserIdRecipient == CSteamID.Nil))
		{
			RemoveMember(steamUserIdRecipient);
		}
	}

	private void OnLobbyDataUpdateLobby(CSteamID steamLobbyId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (!IsThisLobby(steamLobbyId))
		{
			return;
		}
		if (!_firstLobbyUpdate)
		{
			MaxMembers = SteamMatchmaking.GetLobbyMemberLimit(LobbyId);
			LobbyOwnerUserId = SteamMatchmaking.GetLobbyOwner(LobbyId);
			if (LobbyOwnerUserId == CSteamID.Nil)
			{
				throw new NullReferenceException("LobbyOwnerUserId");
			}
			int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(LobbyId);
			for (int i = 0; i < numLobbyMembers; i++)
			{
				CSteamID lobbyMemberByIndex = SteamMatchmaking.GetLobbyMemberByIndex(LobbyId, i);
				if (lobbyMemberByIndex == CSteamID.Nil)
				{
					throw new NullReferenceException("lobbyMemberUserId");
				}
				AddMember(lobbyMemberByIndex);
				UpdateMemberData(lobbyMemberByIndex);
			}
			if (_created)
			{
				SetLobbyName();
				SetMap((EMap)1);
				SetTier(ETier.Tier3);
				SteamNetworkManager.CreateHostSession();
				if (SteamNetworkServer.Instance != null && SteamNetworkServer.Instance.IsListening)
				{
					Melon<BonkWithFriendsMod>.Logger.Msg("Set Server Ready!");
					SetServerReady(serverReady: true);
				}
			}
			_firstLobbyUpdate = true;
		}
		UpdateLobbyData();
	}

	internal void SetLobbyName(string name = "")
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!(LobbyOwnerUserId != SteamUser.GetSteamID()))
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("Requesting lobby name change to: " + name);
			SteamMatchmaking.SetLobbyData(LobbyId, "name", string.IsNullOrWhiteSpace(name) ? (SteamFriends.GetPersonaName() + "'s Lobby") : name);
		}
	}

	private void UpdateLobbyData()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		string lobbyData = SteamMatchmaking.GetLobbyData(LobbyId, "name");
		if (!string.IsNullOrWhiteSpace(lobbyData) && !lobbyData.Equals(Name))
		{
			Name = lobbyData;
			Melon<BonkWithFriendsMod>.Logger.Msg("Lobby name changed to: " + Name);
		}
		string lobbyData2 = SteamMatchmaking.GetLobbyData(LobbyId, "map");
		if (!string.IsNullOrWhiteSpace(lobbyData2) && Enum.TryParse<EMap>(lobbyData2, ignoreCase: false, out EMap result) && Map != result)
		{
			Map = result;
			Melon<BonkWithFriendsMod>.Logger.Msg($"Map changed to: {Map}");
		}
		string lobbyData3 = SteamMatchmaking.GetLobbyData(LobbyId, "tier");
		if (!string.IsNullOrWhiteSpace(lobbyData3) && int.TryParse(lobbyData3, out var result2) && Tier != result2)
		{
			Tier = result2;
			Melon<BonkWithFriendsMod>.Logger.Msg($"Tier changed to: {Tier}");
		}
		string lobbyData4 = SteamMatchmaking.GetLobbyData(LobbyId, "seed");
		if (!string.IsNullOrWhiteSpace(lobbyData4) && int.TryParse(lobbyData4, out var result3) && Seed != result3)
		{
			Seed = result3;
			Melon<BonkWithFriendsMod>.Logger.Msg($"Seed changed to: {Seed}");
		}
		string lobbyData5 = SteamMatchmaking.GetLobbyData(LobbyId, "challenge");
		if (!string.IsNullOrWhiteSpace(lobbyData5))
		{
			if (!((Object)((Object)(object)Challenge)) || ((Object)Challenge).name != lobbyData5)
			{
				foreach (ChallengeData item in Resources.FindObjectsOfTypeAll<ChallengeData>())
				{
					if (((Object)item).name == lobbyData5)
					{
						Challenge = item;
						Melon<BonkWithFriendsMod>.Logger.Msg("Challenge changed to: " + ((Object)Challenge).name);
						break;
					}
				}
			}
		}
		else if ((Object)(object)Challenge != (Object)null)
		{
			Challenge = null;
			Melon<BonkWithFriendsMod>.Logger.Msg("Challenge cleared");
		}
		CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(LobbyId);
		if (lobbyOwner != CSteamID.Nil && lobbyOwner != LobbyOwnerUserId)
		{
			CSteamID lobbyOwnerUserId = LobbyOwnerUserId;
			LobbyOwnerUserId = lobbyOwner;
			Melon<BonkWithFriendsMod>.Logger.Msg($"Lobby owner changed from {SteamPersonaNameCache.GetOrRequestCachedName(lobbyOwnerUserId)} ({lobbyOwnerUserId}) to {SteamPersonaNameCache.GetOrRequestCachedName(LobbyOwnerUserId)} ({LobbyOwnerUserId})");
		}
		string lobbyData6 = SteamMatchmaking.GetLobbyData(LobbyId, "serverReady");
		if (!string.IsNullOrWhiteSpace(lobbyData6) && bool.TryParse(lobbyData6, out var result4) && ServerReady != result4)
		{
			ServerReady = result4;
			Melon<BonkWithFriendsMod>.Logger.Msg($"Server ready status changed to: {ServerReady}");
			if (ServerReady && !_created)
			{
				SteamNetworkManager.CreateClientAndConnect(LobbyOwnerUserId, OnClientConnected);
			}
			OnServerReadyChanged?.Invoke(ServerReady);
		}
		string botsData = SteamMatchmaking.GetLobbyData(LobbyId, "bots");
		BotManager.ParseBotsFromLobbyData(botsData);
	}

	private void OnClientConnected()
	{
		_ = SteamNetworkClient.Instance.IsConnected;
	}

	private void OnLobbyDataUpdateMember(CSteamID steamLobbyId, CSteamID steamMemberId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (IsThisLobby(steamLobbyId) && !(steamMemberId == CSteamID.Nil))
		{
			UpdateMemberData(steamMemberId);
		}
	}

	private void UpdateMemberData(CSteamID steamMemberId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		if (!HasMember(steamMemberId))
		{
			return;
		}
		SteamNetworkLobbyMember member = GetMember(steamMemberId);
		if (member != null)
		{
			string lobbyMemberData = SteamMatchmaking.GetLobbyMemberData(LobbyId, steamMemberId, "ready");
			if (!string.IsNullOrWhiteSpace(lobbyMemberData) && bool.TryParse(lobbyMemberData, out var result) && member.IsReady != result)
			{
				member.IsReady = result;
				Melon<BonkWithFriendsMod>.Logger.Msg($"[{SteamPersonaNameCache.GetOrRequestCachedName(steamMemberId)}] member ready status changed to: {member.IsReady}");
			}
			string lobbyMemberData2 = SteamMatchmaking.GetLobbyMemberData(LobbyId, steamMemberId, "character");
			if (!string.IsNullOrWhiteSpace(lobbyMemberData2) && Enum.TryParse<ECharacter>(lobbyMemberData2, out ECharacter result2) && member.Character != result2)
			{
				member.Character = result2;
				Melon<BonkWithFriendsMod>.Logger.Msg($"[{SteamPersonaNameCache.GetOrRequestCachedName(steamMemberId)}] character changed to: {member.Character}");
			}
			string lobbyMemberData3 = SteamMatchmaking.GetLobbyMemberData(LobbyId, steamMemberId, "skinType");
			if (!string.IsNullOrWhiteSpace(lobbyMemberData3) && Enum.TryParse<ESkinType>(lobbyMemberData3, out ESkinType result3) && member.SkinType != result3)
			{
				member.SkinType = result3;
				Melon<BonkWithFriendsMod>.Logger.Msg($"[{SteamPersonaNameCache.GetOrRequestCachedName(steamMemberId)}] skin type changed to: {member.SkinType}");
			}
		}
	}

	internal void SetSeed(int seed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!(LobbyOwnerUserId != SteamUser.GetSteamID()))
		{
			Melon<BonkWithFriendsMod>.Logger.Msg($"Requesting seed change to: {seed}");
			if (!SteamMatchmaking.SetLobbyData(LobbyId, "seed", seed.ToString()))
			{
				Melon<BonkWithFriendsMod>.Logger.Msg($"Failed to set seed change to: {seed}");
			}
		}
	}

	internal unsafe void SetMap(EMap eMap = (EMap)0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!(LobbyOwnerUserId != SteamUser.GetSteamID()))
		{
			Melon<BonkWithFriendsMod>.Logger.Msg($"Requesting map change to: {eMap}");
			SteamMatchmaking.SetLobbyData(LobbyId, "map", ((object)(*(EMap*)(&eMap))/*cast due to .constrained prefix*/).ToString());
		}
	}

	internal void SetTier(int tier = 0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!(LobbyOwnerUserId != SteamUser.GetSteamID()) && tier >= 0 && tier <= 2)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg($"Requesting tier change to: {tier}");
			SteamMatchmaking.SetLobbyData(LobbyId, "tier", tier.ToString());
		}
	}

	internal void SetTier(ETier eTier = ETier.Tier1)
	{
		SetTier((int)eTier);
	}

	internal void SetChallenge(ChallengeData challengeData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!(LobbyOwnerUserId != SteamUser.GetSteamID()))
		{
			string text = (((Object)(object)challengeData != (Object)null) ? ((Object)challengeData).name : "");
			Melon<BonkWithFriendsMod>.Logger.Msg("Requesting challengeData change to: " + text);
			SteamMatchmaking.SetLobbyData(LobbyId, "challenge", text);
		}
	}

	internal void SetServerReady(bool serverReady)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!(LobbyOwnerUserId != SteamUser.GetSteamID()))
		{
			Melon<BonkWithFriendsMod>.Logger.Msg($"Requesting server ready status change to: {serverReady}");
			SteamMatchmaking.SetLobbyData(LobbyId, "serverReady", serverReady.ToString());
		}
	}

	internal void MemberSetReady(bool isReady)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Melon<BonkWithFriendsMod>.Logger.Msg($"Requesting member ready status change to: {isReady}");
		SteamMatchmaking.SetLobbyMemberData(LobbyId, "ready", isReady.ToString());
	}

	internal unsafe void MemberSetCharacter(ECharacter eCharacter = (ECharacter)0)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Melon<BonkWithFriendsMod>.Logger.Msg($"Requesting character change to: {eCharacter}");
		SteamMatchmaking.SetLobbyMemberData(LobbyId, "character", ((object)(*(ECharacter*)(&eCharacter))/*cast due to .constrained prefix*/).ToString());
	}

	internal unsafe void MemberSetSkinType(ESkinType eSkinType = (ESkinType)0)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Melon<BonkWithFriendsMod>.Logger.Msg($"Requesting skin type change to: {eSkinType}");
		SteamMatchmaking.SetLobbyMemberData(LobbyId, "skinType", ((object)(*(ESkinType*)(&eSkinType))/*cast due to .constrained prefix*/).ToString());
	}

	internal bool AreAllMembersReady()
	{
		lock (_syncRoot)
		{
			return _members.All((SteamNetworkLobbyMember m) => m.IsReady);
		}
	}

	internal void Reset()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (_created)
		{
			SteamNetworkManager.DestroyHostSession();
		}
		else
		{
			SteamNetworkManager.DestroyClient();
		}
		BotManager.ClearAll();
		UnsubscribeFromCallbacksAndCallResults();
		lock (_syncRoot)
		{
			_members?.Clear();
		}
		LobbyId = CSteamID.Nil;
		LobbyOwnerUserId = CSteamID.Nil;
		Instance = null;
	}

	private void UnsubscribeFromCallbacksAndCallResults()
	{
		SteamMatchmakingImpl.OnLobbyChatUpdateMemberEntered = (SteamMatchmakingImpl.LobbyChatUpdateMemberEnteredCallbackDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyChatUpdateMemberEntered, new SteamMatchmakingImpl.LobbyChatUpdateMemberEnteredCallbackDelegate(OnLobbyChatUpdateMemberEntered));
		SteamMatchmakingImpl.OnLobbyChatUpdateMemberLeft = (SteamMatchmakingImpl.LobbyChatUpdateMemberLeftCallbackDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyChatUpdateMemberLeft, new SteamMatchmakingImpl.LobbyChatUpdateMemberLeftCallbackDelegate(OnLobbyChatUpdateMemberLeft));
		SteamMatchmakingImpl.OnLobbyChatUpdateMemberDisconnected = (SteamMatchmakingImpl.LobbyChatUpdateMemberDisconnectedCallbackDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyChatUpdateMemberDisconnected, new SteamMatchmakingImpl.LobbyChatUpdateMemberDisconnectedCallbackDelegate(OnLobbyChatUpdateMemberDisconnected));
		SteamMatchmakingImpl.OnLobbyChatUpdateMemberKicked = (SteamMatchmakingImpl.LobbyChatUpdateMemberKickedCallbackDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyChatUpdateMemberKicked, new SteamMatchmakingImpl.LobbyChatUpdateMemberKickedCallbackDelegate(OnLobbyChatUpdateMemberKicked));
		SteamMatchmakingImpl.OnLobbyChatUpdateMemberBanned = (SteamMatchmakingImpl.LobbyChatUpdateMemberBannedCallbackDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyChatUpdateMemberBanned, new SteamMatchmakingImpl.LobbyChatUpdateMemberBannedCallbackDelegate(OnLobbyChatUpdateMemberBanned));
		SteamMatchmakingImpl.OnLobbyDataUpdateLobby = (SteamMatchmakingImpl.LobbyDataUpdateLobbyCallbackDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyDataUpdateLobby, new SteamMatchmakingImpl.LobbyDataUpdateLobbyCallbackDelegate(OnLobbyDataUpdateLobby));
		SteamMatchmakingImpl.OnLobbyDataUpdateMember = (SteamMatchmakingImpl.LobbyDataUpdateMemberCallbackDelegate)Delegate.Remove(SteamMatchmakingImpl.OnLobbyDataUpdateMember, new SteamMatchmakingImpl.LobbyDataUpdateMemberCallbackDelegate(OnLobbyDataUpdateMember));
	}
}
