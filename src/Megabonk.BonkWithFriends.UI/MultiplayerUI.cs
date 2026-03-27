using System;
using System.Collections;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppAssets.Scripts.Game.Other;
using Il2CppAssets.Scripts.Managers;
using Il2CppAssets.Scripts._Data.MapsAndStages;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using Il2CppTMPro;
using Megabonk.BonkWithFriends.Debug;
using Megabonk.BonkWithFriends.Managers;
using Megabonk.BonkWithFriends.Networking.Steam;
using Megabonk.BonkWithFriends.Steam;
using MelonLoader;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Megabonk.BonkWithFriends.UI;

public static class MultiplayerUI
{
	private const string MOTD_URL = "https://gist.githubusercontent.com/HHG-r00tz/7e0bcb8576664f7b48a9012563ef56fd/raw";

	private const string MOTD_URL_NO_DLL = "https://gist.githubusercontent.com/HHG-r00tz/a7ff6a57a06a5cfe1cbebcac023014e0/raw";

	public static GameObject MultiplayerMenu;

	public static GameObject PopupWindow;

	public static GameObject LobbyMenu;

	public static GameObject LobbyName;

	public static GameObject LobbyCount;

	private static GameObject PlayerListText;

	private static Button originalMapConfirmBTN;

	private static Button clonedMapConfirmBTN;

	private static Button originalMapBackBTN;

	private static Button clonedMapBackBTN;

	private static readonly Dictionary<CSteamID, GameObject> MemberRows = new Dictionary<CSteamID, GameObject>();

	public static void CreateMultiplayerMenus()
	{
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		if (((Object)((Object)(object)MultiplayerMenu)))
		{
			return;
		}
		GameObject ui = GameObject.Find("UI");
		Transform val = ui.transform.Find("Tabs");
		Transform menu = val.Find("Menu");
		Transform val2 = menu.Find("Content/Main/Buttons");
		Transform val3 = val2.Find("B_Play");
		MultiplayerMenu = Object.Instantiate<GameObject>(((Component)menu).gameObject, val, false);
		((Object)MultiplayerMenu).name = "MultiplayerMenu";
		MultiplayerMenu.SetActive(false);
		Object.Destroy((Object)(object)MultiplayerMenu.GetComponent<MenuAlerts>());
		Helpers.DestroyAllChildren(MultiplayerMenu.transform);
		VerticalLayoutGroup obj = MultiplayerMenu.AddComponent<VerticalLayoutGroup>();
		((HorizontalOrVerticalLayoutGroup)obj).childControlHeight = false;
		((HorizontalOrVerticalLayoutGroup)obj).childControlWidth = false;
		((HorizontalOrVerticalLayoutGroup)obj).childForceExpandHeight = false;
		((HorizontalOrVerticalLayoutGroup)obj).spacing = 20f;
		((LayoutGroup)obj).childAlignment = (TextAnchor)4;
		((LayoutGroup)obj).padding.top = 200;
		Window multiplayerWindow = MultiplayerMenu.GetComponent<Window>();
		multiplayerWindow.isFocused = false;
		multiplayerWindow.allButtons.Clear();
		multiplayerWindow.allButtonsHashed.Clear();
		UnityAction onClick = ((UnityAction)((Action)delegate
		{
			CreateLobbyMenus();
			SteamNetworkLobbyManager.CreateLobby(SteamNetworkLobbyType.FriendsOnly);
			MelonCoroutines.Start(WaitForLobbyAndOpen());
		}));
		GameObject val4 = Helpers.CreateButton(((Component)val3).gameObject, MultiplayerMenu.transform, "B_Host", "Host", onClick);
		multiplayerWindow.allButtons.Add(val4.GetComponent<MyButton>());
		multiplayerWindow.allButtonsHashed.Add(val4);
		multiplayerWindow.startBtn = val4.GetComponent<MyButton>();
		UnityAction onClick2 = ((UnityAction)((Action)delegate
		{
			SteamNetworkLobbyManager.FindLobby();
		}));
		GameObject val5 = Helpers.CreateButton(((Component)val3).gameObject, MultiplayerMenu.transform, "B_Find", "Quick Match", onClick2);
		multiplayerWindow.allButtons.Add(val5.GetComponent<MyButton>());
		multiplayerWindow.allButtonsHashed.Add(val5);
		multiplayerWindow.startBtn = val5.GetComponent<MyButton>();
		UnityAction onClick3 = ((UnityAction)((Action)delegate
		{
			ui.GetComponent<MainMenu>().SetWindow(((Component)menu).gameObject);
			((Component)menu).gameObject.GetComponent<Window>().FocusWindow();
		}));
		GameObject val6 = Helpers.CreateButton(((Component)val3).gameObject, MultiplayerMenu.transform, "B_Back", "Back", onClick3);
		val6.AddComponent<BackEscape>();
		multiplayerWindow.allButtons.Add(val6.GetComponent<MyButton>());
		multiplayerWindow.allButtonsHashed.Add(val6);
		UnityAction onTestClick = ((UnityAction)((Action)delegate
		{
			TestModeManager.ActivateTestMode();
			ui.GetComponent<MainMenu>().SetWindow(((Component)menu).gameObject);
			((Component)menu).gameObject.GetComponent<Window>().FocusWindow();
		}));
		GameObject testBtn = Helpers.CreateButton(((Component)val3).gameObject, MultiplayerMenu.transform, "B_Test", "Test", onTestClick);
		multiplayerWindow.allButtons.Add(testBtn.GetComponent<MyButton>());
		multiplayerWindow.allButtonsHashed.Add(testBtn);
		UnityAction onClick4 = ((UnityAction)((Action)delegate
		{
			ui.GetComponent<MainMenu>().SetWindow(MultiplayerMenu);
			multiplayerWindow.FocusWindow();
		}));
		GameObject val7 = Helpers.CreateButton(((Component)val3).gameObject, val2, "B_Multiplayer", "Online", onClick4);
		RectTransform component = val7.GetComponent<RectTransform>();
		if (((Object)((Object)(object)component)))
		{
			component.sizeDelta = new Vector2(320f, 90f);
		}
		((Component)menu).gameObject.GetComponent<Window>().allButtons.Add(val7.GetComponent<MyButton>());
		((Component)menu).gameObject.GetComponent<Window>().allButtonsHashed.Add(val7);
		Transform val8 = val2.Find("B_Unlocks");
		if (!Helpers.ErrorIfNull<Transform>(val8, "[UI] No B_Unlocks game object found!"))
		{
			int siblingIndex = val8.GetSiblingIndex();
			val7.transform.SetSiblingIndex(siblingIndex);
			Window activeWindow = WindowManager.activeWindow;
			if (activeWindow != null)
			{
				activeWindow.FindAllButtonsInWindow();
			}
		}
	}

	public static void CreateLobbyMenus()
	{
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		if (((Object)((Object)(object)LobbyMenu)))
		{
			return;
		}
		GameObject ui = GameObject.Find("UI");
		if (Helpers.ErrorIfNull<GameObject>(ui, "[UI] No UI game object found!"))
		{
			return;
		}
		Transform val = ui.transform.Find("Tabs");
		if (Helpers.ErrorIfNull<Transform>(val, "[UI] No Tabs game object found!"))
		{
			return;
		}
		Transform val2 = ui.transform.Find("TestFonts");
		if (Helpers.ErrorIfNull<Transform>(val2, "[UI] No testText game object found!"))
		{
			return;
		}
		Transform val3 = val.Find("Menu");
		if (Helpers.ErrorIfNull<Transform>(val3, "[UI] No Menu game object found!"))
		{
			return;
		}
		Transform obj = val.Find("MultiplayerMenu");
		if (Helpers.ErrorIfNull<Window>((obj != null) ? ((Component)obj).GetComponent<Window>() : null, "[UI] No MultiplayerMenu game object found!"))
		{
			return;
		}
		Transform val4 = val.Find("Character");
		if (Helpers.ErrorIfNull<Transform>(val4, "[UI] No characterWidow game object found!"))
		{
			return;
		}
		Transform mapSelectionWindow = val.Find("MapNew2");
		if (Helpers.ErrorIfNull<Transform>(mapSelectionWindow, "[UI] No mapSelectionWindow game object found!"))
		{
			return;
		}
		Transform val5 = val3.Find("Content/Main/Buttons");
		if (Helpers.ErrorIfNull<Transform>(val5, "[UI] No Buttons game object found!"))
		{
			return;
		}
		Transform val6 = val5.Find("B_Play");
		if (Helpers.ErrorIfNull<Transform>(val6, "[UI] No B_Play game object found!"))
		{
			return;
		}
		Transform obj2 = mapSelectionWindow.Find("Maps And Stats");
		if (Helpers.ErrorIfNull<MapSelectionUi>((obj2 != null) ? ((Component)obj2).GetComponent<MapSelectionUi>() : null, "[UI] No mapSelectionUi game object found!"))
		{
			return;
		}
		LobbyMenu = Object.Instantiate<GameObject>(((Component)val3).gameObject, val, false);
		((Object)LobbyMenu).name = "LobbyMenu";
		LobbyMenu.SetActive(false);
		Object.Destroy((Object)(object)LobbyMenu.GetComponent<MenuAlerts>());
		Helpers.DestroyAllChildren(LobbyMenu.transform);
		VerticalLayoutGroup obj3 = LobbyMenu.AddComponent<VerticalLayoutGroup>();
		((HorizontalOrVerticalLayoutGroup)obj3).childControlHeight = false;
		((HorizontalOrVerticalLayoutGroup)obj3).childControlWidth = false;
		((HorizontalOrVerticalLayoutGroup)obj3).childForceExpandHeight = false;
		((HorizontalOrVerticalLayoutGroup)obj3).spacing = 12f;
		((LayoutGroup)obj3).childAlignment = (TextAnchor)4;
		((LayoutGroup)obj3).padding.top = 100;
		Window lobbyWindow = LobbyMenu.GetComponent<Window>();
		lobbyWindow.isFocused = false;
		lobbyWindow.allButtons.Clear();
		lobbyWindow.allButtonsHashed.Clear();
		GameObject characterWindow = Object.Instantiate<GameObject>(((Component)val4).gameObject, val, false);
		((Object)characterWindow).name = "CharacterSelectWindow";
		LobbyName = Object.Instantiate<GameObject>(((Component)val2).gameObject, LobbyMenu.transform, false);
		((Object)LobbyName).name = "lobbyText";
		LobbyName.SetActive(true);
		RawImage component = LobbyName.GetComponent<RawImage>();
		if (((Object)((Object)(object)component)))
		{
			((Behaviour)component).enabled = false;
		}
		TextMeshProUGUI componentInChildren = LobbyName.GetComponentInChildren<TextMeshProUGUI>(true);
		if (((Object)((Object)(object)componentInChildren)))
		{
			((Component)componentInChildren).gameObject.SetActive(true);
			((Behaviour)componentInChildren).enabled = true;
			LocalizeStringEvent lobbyNameLocalize = ((Component)componentInChildren).GetComponent<LocalizeStringEvent>();
			if (((Object)((Object)(object)lobbyNameLocalize)))
				((Behaviour)lobbyNameLocalize).enabled = false;
		}
		RectTransform component2 = LobbyName.GetComponent<RectTransform>();
		if (((Object)((Object)(object)component2)))
		{
			Vector3 localPosition = ((Transform)component2).localPosition;
			((Transform)component2).localPosition = new Vector3(localPosition.x, localPosition.y + 320f, localPosition.z);
		}
		LobbyCount = Object.Instantiate<GameObject>(((Component)val2).gameObject, LobbyMenu.transform, false);
		((Object)LobbyCount).name = "lobbyText";
		LobbyCount.SetActive(true);
		RawImage component3 = LobbyCount.GetComponent<RawImage>();
		if (((Object)((Object)(object)component3)))
		{
			((Behaviour)component3).enabled = false;
		}
		TextMeshProUGUI componentInChildren2 = LobbyCount.GetComponentInChildren<TextMeshProUGUI>(true);
		if (((Object)((Object)(object)componentInChildren2)))
		{
			((Component)componentInChildren2).gameObject.SetActive(true);
			((Behaviour)componentInChildren2).enabled = true;
			((TMP_Text)componentInChildren2).richText = true;
			LocalizeStringEvent lobbyCountLocalize = ((Component)componentInChildren2).GetComponent<LocalizeStringEvent>();
			if (((Object)((Object)(object)lobbyCountLocalize)))
				((Behaviour)lobbyCountLocalize).enabled = false;
		}
		RectTransform component4 = LobbyCount.GetComponent<RectTransform>();
		if (((Object)((Object)(object)component4)))
		{
			Vector3 localPosition2 = ((Transform)component4).localPosition;
			((Transform)component4).localPosition = new Vector3(localPosition2.x, localPosition2.y + 200f, localPosition2.z);
		}
		PlayerListText = Object.Instantiate<GameObject>(((Component)val2).gameObject, LobbyMenu.transform, false);
		((Object)PlayerListText).name = "playerListText";
		PlayerListText.SetActive(true);
		RawImage component5 = PlayerListText.GetComponent<RawImage>();
		if (((Object)((Object)(object)component5)))
		{
			((Behaviour)component5).enabled = false;
		}
		TextMeshProUGUI playerListTmp = PlayerListText.GetComponentInChildren<TextMeshProUGUI>(true);
		if (((Object)((Object)(object)playerListTmp)))
		{
			((Component)playerListTmp).gameObject.SetActive(true);
			((Behaviour)playerListTmp).enabled = true;
			((TMP_Text)playerListTmp).richText = true;
			((TMP_Text)playerListTmp).fontSize = 28f;
			((TMP_Text)playerListTmp).alignment = (TextAlignmentOptions)514;
			((TMP_Text)playerListTmp).enableWordWrapping = false;
			((TMP_Text)playerListTmp).overflowMode = (TextOverflowModes)0;
			((TMP_Text)playerListTmp).text = "";
			LocalizeStringEvent playerListLocalize = ((Component)playerListTmp).GetComponent<LocalizeStringEvent>();
			if (((Object)((Object)(object)playerListLocalize)))
				((Behaviour)playerListLocalize).enabled = false;
		}
		RectTransform playerListRect = PlayerListText.GetComponent<RectTransform>();
		if (((Object)((Object)(object)playerListRect)))
		{
			playerListRect.sizeDelta = new Vector2(600f, 120f);
		}
		GameObject obj4 = GameObject.Find("UI/Tabs/CharacterSelectWindow/W_Character/W_Stats (1)/Content/Footer/B_Confirm");
		Button bCharConfirmBTN = ((obj4 != null) ? obj4.GetComponent<Button>() : null);
		GameObject obj5 = GameObject.Find("UI/Tabs/MapNew2/Maps And Stats/W_Stats/Content/Footer/B_Confirm");
		originalMapConfirmBTN = ((obj5 != null) ? obj5.GetComponent<Button>() : null);
		GameObject obj6 = GameObject.Find("UI/Tabs/CharacterSelectWindow/W_Character/Header/Header/B_Back");
		Button bCharBackBTN = ((obj6 != null) ? obj6.GetComponent<Button>() : null);
		GameObject obj7 = GameObject.Find("UI/Tabs/MapNew2/Maps And Stats/W_Maps/Header/Header/B_Back");
		originalMapBackBTN = ((obj7 != null) ? obj7.GetComponent<Button>() : null);
		if (!((Object)((Object)(object)clonedMapConfirmBTN)))
		{
			clonedMapConfirmBTN = Object.Instantiate<Button>(originalMapConfirmBTN, ((Component)originalMapConfirmBTN).transform.parent, false);
		}
		if (!((Object)((Object)(object)clonedMapBackBTN)))
		{
			clonedMapBackBTN = Object.Instantiate<Button>(originalMapBackBTN, ((Component)originalMapBackBTN).transform.parent, false);
		}
		UnityAction onClick = ((UnityAction)((Action)delegate
		{
			ui.GetComponent<MainMenu>().SetWindow(characterWindow);
			((UnityEventBase)bCharConfirmBTN.onClick).SetPersistentListenerState(0, (UnityEventCallState)0);
			((UnityEventBase)bCharBackBTN.onClick).SetPersistentListenerState(0, (UnityEventCallState)0);
			MelonCoroutines.Start(RefreshLobby());
		}));
		GameObject val7 = Helpers.CreateButton(((Component)val6).gameObject, LobbyMenu.transform, "B_ChooseChar", "Choose Character", onClick);
		lobbyWindow.allButtons.Add(val7.GetComponent<MyButton>());
		lobbyWindow.allButtonsHashed.Add(val7);
		lobbyWindow.startBtn = val7.GetComponent<MyButton>();
		lobbyWindow.alwaysUseStartBtn = true;
		UnityAction val8 = ((UnityAction)((Action)delegate
		{
			ui.GetComponent<MainMenu>().SetWindow(LobbyMenu);
			lobbyWindow.FocusWindow();
			LobbyMenu.SetActive(true);
			DisableLobbyButtons(LobbyMenu);
			MelonCoroutines.Start(RefreshLobby());
		}));
		((UnityEvent)bCharConfirmBTN.onClick).AddListener(val8);
		((UnityEvent)bCharBackBTN.onClick).AddListener(val8);
		UnityAction onClick2 = ((UnityAction)((Action)delegate
		{
			ui.GetComponent<MainMenu>().SetWindow(((Component)mapSelectionWindow).gameObject);
			((Component)mapSelectionWindow).gameObject.SetActive(true);
			if ((bool)(Object)(object)clonedMapConfirmBTN)
			{
				((UnityEventBase)clonedMapConfirmBTN.onClick).SetPersistentListenerState(0, (UnityEventCallState)0);
			}
		}));
		GameObject val9 = Helpers.CreateButton(((Component)val6).gameObject, LobbyMenu.transform, "B_ChooseMap", "Choose Map", onClick2);
		lobbyWindow.allButtons.Add(val9.GetComponent<MyButton>());
		lobbyWindow.allButtonsHashed.Add(val9);
		((UnityEventBase)clonedMapConfirmBTN.onClick).RemoveAllListeners();
		((UnityEvent)clonedMapConfirmBTN.onClick).AddListener(val8);
		((UnityEventBase)clonedMapBackBTN.onClick).RemoveAllListeners();
		((UnityEvent)clonedMapBackBTN.onClick).AddListener(val8);
		((Component)clonedMapConfirmBTN).gameObject.SetActive(true);
		((Component)clonedMapBackBTN).gameObject.SetActive(true);
		UnityAction onClick3 = ((UnityAction)((Action)delegate
		{
			SteamNetworkLobbyManager.OpenInviteDialog();
		}));
		GameObject val10 = Helpers.CreateButton(((Component)val6).gameObject, LobbyMenu.transform, "B_Start", "Invite", onClick3);
		lobbyWindow.allButtons.Add(val10.GetComponent<MyButton>());
		lobbyWindow.allButtonsHashed.Add(val10);
		lobbyWindow.startBtn = val10.GetComponent<MyButton>();
		GameObject botRow = new GameObject("BotButtonRow");
		botRow.transform.SetParent(LobbyMenu.transform, false);
		RectTransform botRowRect = botRow.AddComponent<RectTransform>();
		botRowRect.sizeDelta = new Vector2(500f, 70f);
		HorizontalLayoutGroup botHlg = botRow.AddComponent<HorizontalLayoutGroup>();
		((HorizontalOrVerticalLayoutGroup)botHlg).childControlHeight = false;
		((HorizontalOrVerticalLayoutGroup)botHlg).childControlWidth = false;
		((HorizontalOrVerticalLayoutGroup)botHlg).childForceExpandWidth = false;
		((HorizontalOrVerticalLayoutGroup)botHlg).spacing = 10f;
		((LayoutGroup)botHlg).childAlignment = (TextAnchor)4;
		UnityAction onAddBot = ((UnityAction)((Action)delegate
		{
			BotManager.AddBot();
		}));
		GameObject addBotBtn = Helpers.CreateButton(((Component)val6).gameObject, botRow.transform, "B_AddBot", "+ Bot", onAddBot);
		RectTransform addBotRect = addBotBtn.GetComponent<RectTransform>();
		if (((Object)((Object)(object)addBotRect)))
			addBotRect.sizeDelta = new Vector2(230f, 70f);
		lobbyWindow.allButtons.Add(addBotBtn.GetComponent<MyButton>());
		lobbyWindow.allButtonsHashed.Add(addBotBtn);
		UnityAction onRemoveBot = ((UnityAction)((Action)delegate
		{
			BotManager.RemoveBot();
		}));
		GameObject removeBotBtn = Helpers.CreateButton(((Component)val6).gameObject, botRow.transform, "B_RemoveBot", "- Bot", onRemoveBot);
		RectTransform removeBotRect = removeBotBtn.GetComponent<RectTransform>();
		if (((Object)((Object)(object)removeBotRect)))
			removeBotRect.sizeDelta = new Vector2(230f, 70f);
		lobbyWindow.allButtons.Add(removeBotBtn.GetComponent<MyButton>());
		lobbyWindow.allButtonsHashed.Add(removeBotBtn);
		bool isReady = false;
		TextMeshProUGUI buttonText = null;
		UnityAction onClick4 = ((UnityAction)((Action)delegate
		{
			isReady = !isReady;
			SteamNetworkLobby.Instance.MemberSetReady(isReady);
			if ((bool)(Object)(object)buttonText)
			{
				((TMP_Text)buttonText).text = (isReady ? "Unready" : "Ready");
			}
			MelonCoroutines.Start(StartMatch());
		}));
		GameObject val11 = Helpers.CreateButton(((Component)val6).gameObject, LobbyMenu.transform, "B_Ready", "Ready", onClick4);
		Transform val12 = val11.transform.Find("T_Text");
		if (((Object)((Object)(object)val12)))
		{
			buttonText = ((Component)val12).GetComponent<TextMeshProUGUI>();
		}
		else
		{
			buttonText = val11.GetComponentInChildren<TextMeshProUGUI>(true);
		}
		lobbyWindow.allButtons.Add(val11.GetComponent<MyButton>());
		lobbyWindow.allButtonsHashed.Add(val11);
		UnityAction onClick5 = ((UnityAction)((Action)delegate
		{
			CloseLobbyMenu();
		}));
		GameObject val13 = Helpers.CreateButton(((Component)val6).gameObject, LobbyMenu.transform, "B_Leave", "Leave", onClick5);
		val13.AddComponent<BackEscape>();
		lobbyWindow.allButtons.Add(val13.GetComponent<MyButton>());
		lobbyWindow.allButtonsHashed.Add(val13);
		Window activeWindow = WindowManager.activeWindow;
		if (activeWindow != null)
		{
			activeWindow.FindAllButtonsInWindow();
		}
	}

	public static IEnumerator WaitForLobbyAndOpen()
	{
		while (SteamNetworkLobbyManager.State != SteamNetworkLobbyState.Joined)
		{
			yield return null;
		}
		yield return null;
		Transform obj = LobbyName.transform.Find("Text (TMP)");
		TextMeshProUGUI component = ((Component)obj).GetComponent<TextMeshProUGUI>();
		((TMP_Text)component).text = SteamNetworkLobby.Instance.Name;
		((TMP_Text)component).fontSize = 60f;
		((Component)obj).transform.localPosition = new Vector3(0f, 100f, 0f);
		TextMeshProUGUI componentInChildren = LobbyCount.GetComponentInChildren<TextMeshProUGUI>(true);
		if (((Object)((Object)(object)componentInChildren)))
		{
			CSteamID lobbyId = SteamNetworkLobby.Instance.LobbyId;
			int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
			int lobbyMemberLimit = SteamMatchmaking.GetLobbyMemberLimit(lobbyId);
			((TMP_Text)componentInChildren).text = $"Players: {numLobbyMembers} / {lobbyMemberLimit}";
			((TMP_Text)componentInChildren).fontSize = 40f;
			((TMP_Text)componentInChildren).alignment = (TextAlignmentOptions)514;
			((TMP_Text)componentInChildren).richText = true;
		}
		if (SteamNetworkManager.IsServer)
		{
			DateTime utcNow = DateTime.UtcNow;
			int seed = (int)(((ulong)utcNow.Ticks ^ SteamUser.GetSteamID().m_SteamID) & 0x7FFFFFFF);
			SteamNetworkLobby.Instance.SetSeed(seed);
		}
		OpenLobbyMenu();
		MelonCoroutines.Start(RefreshLobby());
	}

	private static IEnumerator RefreshLobby()
	{
		TextMeshProUGUI countTextInstance = LobbyCount.GetComponentInChildren<TextMeshProUGUI>(true);
		TextMeshProUGUI playerListInstance = PlayerListText != null
			? PlayerListText.GetComponentInChildren<TextMeshProUGUI>(true)
			: null;
		while (SteamNetworkLobby.Instance != null && SteamNetworkLobbyManager.State == SteamNetworkLobbyState.Joined)
		{
			CSteamID lobbyId = SteamNetworkLobby.Instance.LobbyId;
			int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
			int lobbyMemberLimit = SteamMatchmaking.GetLobbyMemberLimit(lobbyId);
			int botCount = BotManager.BotCount;

			// Count line
			if (((Object)((Object)(object)countTextInstance)))
			{
				string countLine = botCount > 0
					? $"Players: {numLobbyMembers}/{lobbyMemberLimit} + {botCount} Bot{(botCount > 1 ? "s" : "")}"
					: $"Players: {numLobbyMembers} / {lobbyMemberLimit}";
				((TMP_Text)countTextInstance).richText = true;
				((TMP_Text)countTextInstance).text = countLine;
			}

			// Player list
			if (((Object)((Object)(object)playerListInstance)))
			{
				string playerList = "";
				IReadOnlyList<SteamNetworkLobbyMember> members = SteamNetworkLobby.Instance.Members;
				foreach (SteamNetworkLobbyMember member in members)
				{
					string name = SteamPersonaNameCache.GetOrRequestCachedName(member.UserId) ?? "???";
					bool isHost = member.UserId == SteamNetworkLobby.Instance.LobbyOwnerUserId;
					string readyTag = member.IsReady ? "READY" : "...";
					string hostTag = isHost ? "[HOST] " : "";
					playerList += $"{hostTag}{name} [{member.Character}] {readyTag}\n";
				}
				foreach (BotManager.BotSlot bot in BotManager.LobbyBots)
				{
					playerList += $"[BOT] {bot.Name} [{bot.Character}]\n";
				}
				((TMP_Text)playerListInstance).richText = true;
				((TMP_Text)playerListInstance).text = playerList;
			}
			yield return (object)new WaitForSeconds(0.25f);
		}
	}

	private static IEnumerator StartMatch()
	{
		if (SteamNetworkLobby.Instance == null || SteamNetworkLobbyManager.State != SteamNetworkLobbyState.Joined)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("[MP] StartMatch coroutine stopped - lobby closed");
			yield break;
		}
		while (!SteamNetworkLobby.Instance.AreAllMembersReady() || SteamNetworkLobby.Instance.MemberCount + BotManager.BotCount < 2)
		{
			yield return (object)new WaitForSeconds(0.1f);
		}
		yield return null;
		EMap map = SteamNetworkLobby.Instance.Map;
		int tier = SteamNetworkLobby.Instance.Tier;
		MapData map2 = DataManager.Instance.GetMap(map);
		StageData stageData = ((Il2CppArrayBase<StageData>)(object)map2.stages)[tier];
		RunConfig val = new RunConfig
		{
			mapData = map2,
			stageData = stageData,
			mapTierIndex = tier,
			challenge = null,
			musicTrackIndex = -1
		};
		if (SteamNetworkManager.IsServer)
		{
			SteamNetworkLobby.Instance.LobbyType = SteamNetworkLobbyType.Private;
		}
		MapController.StartNewMap(val);
	}

	public static void EnableButton(Button button, GameObject buttonOverlay)
	{
		if (((Object)((Object)(object)button)) && ((Object)((Object)(object)buttonOverlay)))
		{
			((Selectable)button).interactable = true;
			buttonOverlay.SetActive(false);
		}
	}

	public static void DisableButton(Button button, GameObject buttonOverlay)
	{
		((Selectable)button).interactable = false;
		buttonOverlay.SetActive(true);
	}

	public static void OpenLobbyMenu()
	{
		Melon<BonkWithFriendsMod>.Logger.Msg("[MP] OpenLobbyMenu");
		GameObject val = GameObject.Find("UI");
		if (!Helpers.ErrorIfNull<GameObject>(val, "[UI] No UI game object found!") && !Helpers.ErrorIfNull<GameObject>(LobbyMenu, "[UI] No LobbyMenu game object found!"))
		{
			val.GetComponent<MainMenu>().SetWindow(LobbyMenu.gameObject);
			LobbyMenu.GetComponent<Window>().FocusWindow();
			LobbyMenu.SetActive(true);
			ResetLobbyMenu(LobbyMenu.transform);
			DisableLobbyButtons(LobbyMenu.gameObject);
			if (((Object)((Object)(object)originalMapConfirmBTN)))
			{
				((Component)originalMapConfirmBTN).gameObject.SetActive(false);
			}
			if (((Object)((Object)(object)originalMapBackBTN)))
			{
				((Component)originalMapBackBTN).gameObject.SetActive(false);
			}
			if (((Object)((Object)(object)clonedMapConfirmBTN)))
			{
				((Component)clonedMapConfirmBTN).gameObject.SetActive(true);
			}
			if (((Object)((Object)(object)clonedMapBackBTN)))
			{
				((Component)clonedMapBackBTN).gameObject.SetActive(true);
			}
		}
	}

	public static void CloseLobbyMenu()
	{
		SteamNetworkLobbyManager.LeaveLobby();
		if (((Object)((Object)(object)originalMapConfirmBTN)))
		{
			((Component)originalMapConfirmBTN).gameObject.SetActive(true);
		}
		if (((Object)((Object)(object)originalMapBackBTN)))
		{
			((Component)originalMapBackBTN).gameObject.SetActive(true);
		}
		if (((Object)((Object)(object)clonedMapConfirmBTN)))
		{
			((Component)clonedMapConfirmBTN).gameObject.SetActive(false);
		}
		if (((Object)((Object)(object)clonedMapBackBTN)))
		{
			((Component)clonedMapBackBTN).gameObject.SetActive(false);
		}
		GameObject obj = GameObject.Find("UI/Tabs/MapNew2");
		Window val = ((obj != null) ? obj.GetComponent<Window>() : null);
		if (((Object)((Object)(object)val)))
		{
			val.FindAllButtonsInWindow();
		}
		GameObject val2 = GameObject.Find("UI");
		if (!Helpers.ErrorIfNull<GameObject>(val2, "[UI] No UI game object found!") && !Helpers.ErrorIfNull<GameObject>(MultiplayerMenu, "[UI] No MultiplayerMenu game object found!"))
		{
			val2.GetComponent<MainMenu>().SetWindow(MultiplayerMenu.gameObject);
			MultiplayerMenu.GetComponent<Window>().FocusWindow();
		}
	}

	public static void ResetLobbyMenu(Transform lobbyMenu)
	{
		Button component = ((Component)((Component)lobbyMenu).transform.Find("B_Start")).GetComponent<Button>();
		GameObject gameObject = ((Component)((Component)component).transform.Find("DisabledOverlay")).gameObject;
		Button component2 = ((Component)((Component)lobbyMenu).transform.Find("B_Ready")).GetComponent<Button>();
		GameObject gameObject2 = ((Component)((Component)component2).transform.Find("DisabledOverlay")).gameObject;
		Button component3 = ((Component)((Component)lobbyMenu).transform.Find("B_ChooseMap")).GetComponent<Button>();
		GameObject gameObject3 = ((Component)((Component)component3).transform.Find("DisabledOverlay")).gameObject;
		EnableButton(component, gameObject);
		EnableButton(component2, gameObject2);
		EnableButton(component3, gameObject3);
	}

	public static void DisableLobbyButtons(GameObject lobbyMenu)
	{
		Transform val = lobbyMenu.transform.Find("B_Start");
		if (Helpers.ErrorIfNull<Transform>(val, "[UI] No B_Start game object found!") || Helpers.ErrorIfNull<Transform>(lobbyMenu.transform.Find("B_Ready"), "[UI] No B_Ready game object found!"))
		{
			return;
		}
		Transform val2 = lobbyMenu.transform.Find("B_ChooseMap");
		if (Helpers.ErrorIfNull<Transform>(val2, "[UI] No B_ChooseMap game object found!"))
		{
			return;
		}
		Melon<BonkWithFriendsMod>.Logger.Msg("SteamNetworkManager.Mode: " + SteamNetworkManager.Mode);
		if (!SteamNetworkManager.IsClient || SteamNetworkManager.IsServer)
		{
			return;
		}
		Transform val3 = val2.Find("DisabledOverlay");
		if (!Helpers.ErrorIfNull<Transform>(val3, "[UI] No DisabledOverlay game object found!"))
		{
			DisableButton(((Component)val2).GetComponent<Button>(), ((Component)val3).gameObject);
			Transform val4 = val.Find("DisabledOverlay");
			if (!Helpers.ErrorIfNull<Transform>(val4, "[UI] No DisabledOverlay game object found!"))
			{
				DisableButton(((Component)val).GetComponent<Button>(), ((Component)val4).gameObject);
			}
		}
		Transform addBotTransform = lobbyMenu.transform.Find("BotButtonRow/B_AddBot");
		if (addBotTransform != null)
		{
			Transform addBotOverlay = addBotTransform.Find("DisabledOverlay");
			if (addBotOverlay != null)
				DisableButton(((Component)addBotTransform).GetComponent<Button>(), ((Component)addBotOverlay).gameObject);
		}
		Transform removeBotTransform = lobbyMenu.transform.Find("BotButtonRow/B_RemoveBot");
		if (removeBotTransform != null)
		{
			Transform removeBotOverlay = removeBotTransform.Find("DisabledOverlay");
			if (removeBotOverlay != null)
				DisableButton(((Component)removeBotTransform).GetComponent<Button>(), ((Component)removeBotOverlay).gameObject);
		}
	}

	public static void ShowPopup()
	{
		if (((Object)((Object)(object)PopupWindow)))
		{
			return;
		}
		Transform val = GameObject.Find("UI").transform.Find("Tabs");
		Transform obj = val.Find("W_Credits");
		val.Find("Menu");
		PopupWindow = Object.Instantiate<GameObject>(((Component)obj).gameObject, val, false);
		((Object)PopupWindow).name = "BWFPopupWindow";
		Transform val2 = PopupWindow.transform.Find("Header/Header/T_Title");
		if (((Object)((Object)(object)val2)))
		{
			TextMeshProUGUI component = ((Component)val2).GetComponent<TextMeshProUGUI>();
			if (((Object)((Object)(object)component)))
			{
				LocalizeStringEvent component2 = ((Component)component).GetComponent<LocalizeStringEvent>();
				if (((Object)((Object)(object)component2)))
				{
					((Behaviour)component2).enabled = false;
				}
				((TMP_Text)component).text = "BonkWithFriends Notice";
			}
		}
		Transform val3 = PopupWindow.transform.Find("WindowLayers/Content/ScrollRect/ContentEntries/T_Title");
		if (((Object)((Object)(object)val3)))
		{
			TextMeshProUGUI component3 = ((Component)val3).GetComponent<TextMeshProUGUI>();
			if (((Object)((Object)(object)component3)))
			{
				((TMP_Text)component3).text = "Loading...";
				MelonCoroutines.Start(FetchMOTD(component3));
			}
		}
		Button component4 = ((Component)PopupWindow.transform.Find("Header/Header/B_Back")).GetComponent<Button>();
		((UnityEventBase)component4.onClick).SetPersistentListenerState(0, (UnityEventCallState)0);
		((Behaviour)component4).enabled = false;
		((Component)val3).gameObject.AddComponent<TMPLinkHandler>();
		PopupWindow.SetActive(true);
		Window activeWindow = WindowManager.activeWindow;
		if (activeWindow != null)
		{
			activeWindow.FindAllButtonsInWindow();
		}
	}

	private static IEnumerator FetchMOTD(TextMeshProUGUI textComponent)
	{
		UnityWebRequest request = ((!BonkWithFriendsMod.IsSteamApiDllMissing) ? UnityWebRequest.Get("https://gist.githubusercontent.com/HHG-r00tz/7e0bcb8576664f7b48a9012563ef56fd/raw") : UnityWebRequest.Get("https://gist.githubusercontent.com/HHG-r00tz/a7ff6a57a06a5cfe1cbebcac023014e0/raw"));
		yield return request.SendWebRequest();
		if ((int)request.result == 1)
		{
			((TMP_Text)textComponent).text = request.downloadHandler.text;
		}
		else
		{
			((TMP_Text)textComponent).text = "This mod is in pre-alpha development. Please bear this in mind.\n\n<color=red>Failed to load latest updates.</color>\n\nJoin our <link=\"https://discord.gg/Mxc8uFA8Nv\"><color=#5865F2><u>Discord</u></color></link>!";
			Melon<BonkWithFriendsMod>.Logger.Warning("Failed to fetch MOTD: " + request.error);
		}
		MelonCoroutines.Start(HidePopupAfterDelay());
	}

	private static IEnumerator HidePopupAfterDelay()
	{
		if (BonkWithFriendsMod.IsSteamApiDllMissing)
		{
			yield return (object)new WaitForSeconds(20f);
			Application.Quit();
		}
		else
		{
			yield return (object)new WaitForSeconds(8f);
		}
		if (((Object)((Object)(object)PopupWindow)))
		{
			PopupWindow.SetActive(false);
			Object.Destroy((Object)(object)PopupWindow);
			PopupWindow = null;
		}
	}
}
