using System.Collections;
using Megabonk.BonkWithFriends.Managers;
using Megabonk.BonkWithFriends.Networking.Steam;
using Megabonk.BonkWithFriends.UI;
using MelonLoader;

namespace Megabonk.BonkWithFriends.Debug;

public static class TestModeManager
{
	public static bool IsTestMode { get; private set; }

	public static void ActivateTestMode()
	{
		if (IsTestMode)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning("[TestMode] Already active");
			return;
		}

		IsTestMode = true;

		// Create a real private lobby, add a bot, then open it
		MultiplayerUI.CreateLobbyMenus();
		SteamNetworkLobbyManager.CreateLobby(SteamNetworkLobbyType.Private);
		MelonCoroutines.Start(WaitAndSetupTestLobby());

		Melon<BonkWithFriendsMod>.Logger.Msg("[TestMode] Activated — creating private lobby with bot");
	}

	private static IEnumerator WaitAndSetupTestLobby()
	{
		// Wait for lobby to be created and joined
		while (SteamNetworkLobbyManager.State != SteamNetworkLobbyState.Joined)
			yield return null;

		yield return null;

		// Add a bot
		BotManager.AddBot();

		// Open the lobby menu (WaitForLobbyAndOpen handles the timing)
		MelonCoroutines.Start(MultiplayerUI.WaitForLobbyAndOpen());
	}

	public static void DeactivateTestMode()
	{
		IsTestMode = false;
	}

	public static void Update()
	{
		// No longer needed — BotManager.Update() handles bots
	}

	public static void DrawGui()
	{
		// Simplified — no more IMGUI overlay needed for test mode
		if (!IsTestMode) return;

		if (SteamNetworkManager.IsMultiplayer)
		{
			UnityEngine.GUI.Label(new UnityEngine.Rect(10f, 10f, 300f, 25f),
				"<color=yellow>[TEST MODE]</color>");
		}
	}
}
