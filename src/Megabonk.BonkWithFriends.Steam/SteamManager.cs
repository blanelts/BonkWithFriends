using System;
using System.Text;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppRewired;
using Il2CppSystem.Collections.Generic;
using Megabonk.BonkWithFriends.HarmonyPatches.Items;
using Megabonk.BonkWithFriends.Managers;
using Megabonk.BonkWithFriends.Managers.Enemies;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.Managers.Server;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Megabonk.BonkWithFriends.Steam;

[RegisterTypeInIl2Cpp]
internal sealed class SteamManager : MonoBehaviour
{
	private const uint CMegabonkSteamAppId = 3405340u;

	private static readonly AppId_t MegabonkSteamAppId = new AppId_t(3405340u);

	internal static SteamManager Instance { get; set; }

	internal bool Initialized { get; set; }

	internal CSteamID CurrentUserId { get; set; }

	internal SteamNetworkLobby Lobby { get; set; }

	internal SteamNetworkServer Server { get; set; }

	internal SteamNetworkClient Client { get; set; }

	public SteamManager(IntPtr intPtr)
		: base(intPtr)
	{
	}

	public SteamManager()
		: base(ClassInjector.DerivedConstructorPointer<SteamManager>())
	{
		ClassInjector.DerivedConstructorBody((Il2CppObjectBase)(object)this);
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Object.DestroyImmediate((Object)(object)Instance);
		}
		else
		{
			Instance = this;
		}
	}

	private void Start()
	{
		SetupSteamApi();
		if (!Initialized)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[SteamManager.Start] Aborting setting up further steam integrations..");
			return;
		}
		SetupSteamComponents();
		SetupSteamImplementations();
	}

	private void SetupSteamApi()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (!Packsize.Test())
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[SteamManager.SetupSteamApi] Packsize failed!");
			return;
		}
		if (!DllCheck.Test())
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[SteamManager.SetupSteamApi] DllCheck failed!");
			return;
		}
		if (SteamAPI.RestartAppIfNecessary(MegabonkSteamAppId))
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[SteamManager.SetupSteamApi] Steam not available or game was started through the executable!");
			Application.Quit();
			return;
		}
		if (!(Initialized = SteamAPI.Init()))
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[SteamManager.SetupSteamApi] Couldn't initialize SteamAPI!");
			return;
		}
		CurrentUserId = SteamUser.GetSteamID();
		Melon<BonkWithFriendsMod>.Logger.Msg($"[{"SteamManager"}.{"SetupSteamApi"}] SteamAPI initialized! (Name: {SteamFriends.GetPersonaName()}, ID: {CurrentUserId})");
	}

	private static void SteamNetworkingSocketsDebugOutput(ESteamNetworkingSocketsDebugOutputType nType, StringBuilder pszMsg)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected I4, but got Unknown
		if (pszMsg != null && pszMsg.Length > 0)
		{
			switch ((int)nType)
			{
			case 1:
			case 2:
				Melon<BonkWithFriendsMod>.Logger.Error(pszMsg.ToString() ?? "");
				break;
			case 3:
			case 4:
				Melon<BonkWithFriendsMod>.Logger.Warning(pszMsg.ToString() ?? "");
				break;
			case 0:
			case 5:
			case 6:
			case 7:
			case 8:
				Melon<BonkWithFriendsMod>.Logger.Msg(pszMsg.ToString() ?? "");
				break;
			}
		}
	}

	private void SetupSteamComponents()
	{
	}

	private void SetupSteamImplementations()
	{
		SteamNetworkingImpl.Setup();
		SteamMatchmakingImpl.Setup();
		SteamFriendsImpl.Setup();
		SteamPersonaNameCache.Setup();
	}

	private void FixedUpdate()
	{
		if (Initialized)
		{
			float fixedDeltaTime = Time.fixedDeltaTime;
			double fixedTimeAsDouble = Time.fixedTimeAsDouble;
			Server?.FixedUpdate(fixedDeltaTime, fixedTimeAsDouble);
			Client?.FixedUpdate(fixedDeltaTime, fixedTimeAsDouble);
		}
	}

	private void Update()
	{
		if (Initialized)
		{
			CheckInputs();
			SteamAPI.RunCallbacks();
			float deltaTime = Time.deltaTime;
			double timeAsDouble = Time.timeAsDouble;
			Server?.Update(deltaTime, timeAsDouble);
			Client?.Update(deltaTime, timeAsDouble);
			NetUpdate();
		}
	}

	private void NetUpdate()
	{
		if (SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer && PlayerSceneManager.HasPendingSceneLoad(out var sceneName))
		{
			PlayerSceneManager.ClearPendingSceneLoad();
			Melon<BonkWithFriendsMod>.Logger.Msg("[MP] Executing queued scene load for '" + sceneName + "'...");
			SceneManager.LoadScene(sceneName);
			return;
		}
		if (SteamNetworkManager.IsServer)
		{
			HostEnemyManager.HostNetworkTick();
		}
		if (SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer)
		{
			NetworkTimeSync.Update();
			RemoteEnemyManager.Update();
			PickupPatches.ProcessPendingSpawns();
			PickupPatches.ProcessPendingDespawns();
		}
		LocalPlayerManager.BroadcastPlayerStateChange();
	}

	private void CheckInputs()
	{
		ReInput.PlayerHelper players = ReInput.players;
		if (players == null)
		{
			return;
		}
		IList<Il2CppRewired.Player> allPlayers = players.AllPlayers;
		if (allPlayers == null)
		{
			return;
		}
		Il2CppRewired.Player val = allPlayers[1];
		if (val == null)
		{
			return;
		}
		Il2CppRewired.Player.ControllerHelper controllers = val.controllers;
		if (controllers == null)
		{
			return;
		}
		Keyboard keyboard = controllers.Keyboard;
		if (keyboard != null && keyboard.GetModifierKey((ModifierKey)1))
		{
			if (keyboard.GetKeyDown((KeyCode)49))
			{
				SteamNetworkLobbyManager.CreateLobby();
			}
			if (keyboard.GetKeyDown((KeyCode)50))
			{
				SteamNetworkLobbyManager.OpenInviteDialog();
			}
			keyboard.GetKeyDown((KeyCode)51);
			if (keyboard.GetKeyDown((KeyCode)48))
			{
				SteamNetworkLobbyManager.LeaveLobby();
			}
		}
	}

	private void LateUpdate()
	{
		float deltaTime = Time.deltaTime;
		double timeAsDouble = Time.timeAsDouble;
		Server?.LateUpdate(deltaTime, timeAsDouble);
		Client?.LateUpdate(deltaTime, timeAsDouble);
	}

	private void OnDestroy()
	{
		SteamAPI.Shutdown();
		RemoveSteamImplementations();
		RemoveSteamComponents();
		Initialized = false;
	}

	private void RemoveSteamImplementations()
	{
		SteamPersonaNameCache.Reset();
		SteamFriendsImpl.Reset();
		SteamMatchmakingImpl.Reset();
		SteamNetworkingImpl.Reset();
	}

	private void RemoveSteamComponents()
	{
	}
}
