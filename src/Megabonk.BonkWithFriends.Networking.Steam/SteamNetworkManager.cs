using System;
using Il2Cpp;
using Il2CppUtility;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Steam;
using MelonLoader;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using SteamManager = Megabonk.BonkWithFriends.Steam.SteamManager;

namespace Megabonk.BonkWithFriends.Networking.Steam;

internal static class SteamNetworkManager
{
	internal static int NetworkSeed { get; set; }

	internal static SteamNetworkMode Mode { get; private set; }

	internal static bool IsServer => Mode == SteamNetworkMode.Server || Mode == SteamNetworkMode.Host;
	internal static bool IsClient => Mode == SteamNetworkMode.Client || Mode == SteamNetworkMode.Host;
	internal static bool IsMultiplayer => Mode != SteamNetworkMode.None;

	internal static void CreateHostSession()
	{
		SteamManager instance = SteamManager.Instance;
		if ((Object)(object)instance == (Object)null) return;

		if (instance.Server != null) DestroyServer();
		if (instance.Client != null) DestroyClient();

		// 1. Create server and start listening
		SteamNetworkServer server = new SteamNetworkServer();
		instance.Server = server;
		server.StartListening();

		if (!server.IsListening)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[Net] Server failed to start listening, aborting host session");
			instance.Server.Dispose();
			instance.Server = null;
			return;
		}

		// 2. Create client and connect to self (loopback P2P)
		CSteamID myId = SteamUser.GetSteamID();
		SteamNetworkClient client = new SteamNetworkClient();
		instance.Client = client;
		client.Connect(myId);

		// 3. Set Mode=Host (server + client)
		SetMode(SteamNetworkMode.Host);
		Melon<BonkWithFriendsMod>.Logger.Msg("[Net] Host session created (server + loopback client)");
	}

	internal static void DestroyHostSession()
	{
		SteamManager instance = SteamManager.Instance;
		if ((Object)(object)instance == (Object)null) return;

		SetMode(SteamNetworkMode.None);

		if (instance.Client != null)
		{
			instance.Client.Dispose();
			instance.Client = null;
		}
		if (instance.Server != null)
		{
			instance.Server.Dispose();
			instance.Server = null;
		}
		Melon<BonkWithFriendsMod>.Logger.Msg("[Net] Host session destroyed");
	}

	internal static void CreateAndStartServer()
	{
		SteamManager instance = SteamManager.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			if (instance.Server != null)
			{
				DestroyServer();
			}
			SteamNetworkServer steamNetworkServer = (instance.Server = new SteamNetworkServer());
			steamNetworkServer.StartListening();
			SetMode(SteamNetworkMode.Server);
		}
	}

	internal static void CreateServer()
	{
		SteamManager instance = SteamManager.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			if (instance.Server != null)
			{
				DestroyServer();
			}
			SteamNetworkServer server = new SteamNetworkServer();
			instance.Server = server;
			SetMode(SteamNetworkMode.Server);
		}
	}

	internal static void DestroyServer()
	{
		SteamManager instance = SteamManager.Instance;
		if (!((Object)(object)instance == (Object)null) && instance.Server != null)
		{
			SetMode(SteamNetworkMode.None);
			instance.Server.Dispose();
			instance.Server = null;
		}
	}

	internal static void CreateClientAndConnect(CSteamID steamUserId)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		SteamManager instance = SteamManager.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			if (instance.Client != null)
			{
				DestroyClient();
			}
			SteamNetworkClient steamNetworkClient = (instance.Client = new SteamNetworkClient());
			steamNetworkClient.Connect(steamUserId);
			SetMode(SteamNetworkMode.Client);
		}
	}

	internal static void CreateClientAndConnect(CSteamID steamUserId, ClientConnectedDelegate clientConnectedDelegate)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		SteamManager instance = SteamManager.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			if (instance.Client != null)
			{
				DestroyClient();
			}
			SteamNetworkClient steamNetworkClient = new SteamNetworkClient();
			steamNetworkClient.OnConnected = (ClientConnectedDelegate)Delegate.Combine(steamNetworkClient.OnConnected, clientConnectedDelegate);
			instance.Client = steamNetworkClient;
			steamNetworkClient.Connect(steamUserId);
			SetMode(SteamNetworkMode.Client);
		}
	}

	internal static void CreateClient()
	{
		SteamManager instance = SteamManager.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			if (instance.Client != null)
			{
				DestroyClient();
			}
			SteamNetworkClient client = new SteamNetworkClient();
			instance.Client = client;
			SetMode(SteamNetworkMode.Client);
		}
	}

	internal static void DestroyClient()
	{
		SteamManager instance = SteamManager.Instance;
		if (!((Object)(object)instance == (Object)null) && instance.Client != null)
		{
			SetMode(SteamNetworkMode.None);
			instance.Client.Dispose();
			instance.Client = null;
		}
	}

	internal static void StartMultiplayerGame(string sceneName)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		if (string.IsNullOrWhiteSpace(sceneName))
		{
			throw new ArgumentNullException("sceneName");
		}
		if (IsServer)
		{
			NetworkSeed = System.Random.Shared.Next(100000000, int.MaxValue);
			UnityEngine.Random.InitState(NetworkSeed);
			MapGenerator.seed = NetworkSeed;
			MyRandom.random = (Il2CppSystem.Random)new ConsistentRandom(NetworkSeed);
			SteamNetworkServer.Instance?.BroadcastToRemoteClients(new SeedSyncMessage
			{
				Seed = NetworkSeed
			});
			SceneManager.LoadScene(sceneName);
		}
	}

	private static void SetMode(SteamNetworkMode steamNetworkMode)
	{
		Mode = steamNetworkMode;
	}

}
