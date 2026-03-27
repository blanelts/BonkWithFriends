using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Il2Cpp;

using Il2CppUtility;
using Megabonk.BonkWithFriends.HarmonyPatches.Items;
using Megabonk.BonkWithFriends.Managers.Items;
using Megabonk.BonkWithFriends.Networking.Messages;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Steam;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Networking.Steam;

internal sealed class SteamNetworkClient : System.IDisposable, System.IAsyncDisposable
{
	private const int SteamNetworkMessageBufferSize = 128;

	private const int MaxNetworkReadsPerFrame = 4;

	private const double SendRate = 1.0 / 60.0;

	private const float KeepAliveInterval = 5f;

	internal static SteamNetworkClient Instance;

	private readonly object _syncRoot = new object();

	private bool _disposedValue;

	private bool _netAuthenticationStatusCurrent;

	private bool _relayNetworkAccessStatusCurrent;

	private HSteamNetConnection _steamNetConnectionHandle;

	private SteamNetworkingIdentity _remoteSteamNetworkingIdentity;

	private CSteamID _remoteSteamUserId;

	private System.IntPtr[] _steamNetworkMessageReceiveBuffer;

	private System.IntPtr[] _steamNetworkMessageSendBuffer;

	private long[] _steamNetworkMessageSendResults;

	private ConcurrentQueue<SteamNetworkMessage> _steamNetworkMessageSendQueue;

	private NetworkMessageDispatcher _networkMessageDispatcher;

	private double _lastSendTime;

	private float _keepAliveTimer = 5f;

	internal ClientConnectedDelegate OnConnected;

	internal bool IsConnected { get; private set; }

	internal bool SafeRW { get; private set; }

	internal SteamNetworkClientState State { get; private set; }

	internal SteamNetworkClient(bool safeReadingAndWriting = true)
	{
		if (Instance != null)
		{
			throw new InvalidOperationException();
		}
		Instance = this;
		_networkMessageDispatcher = new NetworkMessageDispatcher(isServer: false);
		_steamNetworkMessageReceiveBuffer = new IntPtr[128];
		_steamNetworkMessageSendBuffer = new IntPtr[128];
		_steamNetworkMessageSendResults = new long[128];
		_steamNetworkMessageSendQueue = new ConcurrentQueue<SteamNetworkMessage>();
		SafeRW = safeReadingAndWriting;
		Setup();
	}

	private void Setup()
	{
		SubscribeToCallbacksAndCallResults();
	}

	private void SubscribeToCallbacksAndCallResults()
	{
		SteamNetworkingImpl.OnSteamNetAuthenticationStatusCurrent = (SteamNetworkingImpl.SteamNetAuthenticationStatusCurrentCallbackDelegate)Delegate.Combine(SteamNetworkingImpl.OnSteamNetAuthenticationStatusCurrent, new SteamNetworkingImpl.SteamNetAuthenticationStatusCurrentCallbackDelegate(OnSteamNetAuthenticationStatusCurrent));
		SteamNetworkingImpl.OnSteamRelayNetworkStatusCurrent = (SteamNetworkingImpl.SteamRelayNetworkStatusCurrentCallbackDelegate)Delegate.Combine(SteamNetworkingImpl.OnSteamRelayNetworkStatusCurrent, new SteamNetworkingImpl.SteamRelayNetworkStatusCurrentCallbackDelegate(OnSteamRelayNetworkStatusCurrent));
		SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionAccepted = (SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionAcceptedCallbackDelegate)Delegate.Combine(SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionAccepted, new SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionAcceptedCallbackDelegate(OnSteamNetConnectionStatusChangedConnectionAccepted));
		SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionClosedOrRejected = (SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionClosedOrRejectedCallbackDelegate)Delegate.Combine(SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionClosedOrRejected, new SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionClosedOrRejectedCallbackDelegate(OnSteamNetConnectionStatusChangedConnectionClosedOrRejected));
		SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionProblem = (SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionProblemCallbackDelegate)Delegate.Combine(SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionProblem, new SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionProblemCallbackDelegate(OnSteamNetConnectionStatusChangedConnectionProblem));
	}

	private bool IsReadyToConnect()
	{
		if (_netAuthenticationStatusCurrent)
		{
			return _relayNetworkAccessStatusCurrent;
		}
		return false;
	}

	internal void Connect(CSteamID steamUserId)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		SteamNetworkingIdentity val = default(SteamNetworkingIdentity);
		val.SetSteamID(steamUserId);
		SteamNetworkingConfigValue_t[] array = Array.Empty<SteamNetworkingConfigValue_t>();
		_steamNetConnectionHandle = SteamNetworkingSockets.ConnectP2P(ref val, 0, array.Length, array);
	}

	private void OnSteamNetAuthenticationStatusCurrent()
	{
		_netAuthenticationStatusCurrent = true;
	}

	private void OnSteamRelayNetworkStatusCurrent()
	{
		_relayNetworkAccessStatusCurrent = true;
	}

	private void OnSteamNetConnectionStatusChangedConnectionAccepted(HSteamNetConnection connection, SteamNetConnectionInfo_t connectionInfo, ESteamNetworkingConnectionState oldState)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (IsConnected || connection == HSteamNetConnection.Invalid)
		{
			SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
			return;
		}
		CSteamID nil = CSteamID.Nil;
		SteamNetworkingIdentity identityRemote = connectionInfo.m_identityRemote;
		if ((int)identityRemote.m_eType == 16)
		{
			nil = identityRemote.GetSteamID();
			if (nil == CSteamID.Nil)
			{
				SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
				Melon<BonkWithFriendsMod>.Logger.Error("[OnSteamNetConnectionStatusChangedConnectionAccepted] Steam user id null!");
				return;
			}
			if (_steamNetConnectionHandle != connection)
			{
				SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
				throw new InvalidOperationException("connection");
			}
			_remoteSteamNetworkingIdentity = identityRemote;
			_remoteSteamUserId = nil;
			IsConnected = true;
			Melon<BonkWithFriendsMod>.Logger.Msg("Client connected..");
			OnConnected?.Invoke();
			return;
		}
		SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
		throw new ArgumentException("steamNetworkingIdentity");
	}

	private void OnSteamNetConnectionStatusChangedConnectionClosedOrRejected(HSteamNetConnection connection, SteamNetConnectionInfo_t connectionInfo, ESteamNetworkingConnectionState oldState)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (!IsConnected || connection == HSteamNetConnection.Invalid)
		{
			SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
			return;
		}
		CSteamID nil = CSteamID.Nil;
		SteamNetworkingIdentity identityRemote = connectionInfo.m_identityRemote;
		if ((int)identityRemote.m_eType == 16)
		{
			nil = identityRemote.GetSteamID();
			if (nil == CSteamID.Nil)
			{
				SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
				Melon<BonkWithFriendsMod>.Logger.Error("[OnSteamNetConnectionStatusChangedConnectionClosedOrRejected] Steam user id null!");
			}
			else
			{
				SteamNetworkLobbyManager.LeaveLobby();
				if (SteamNetworkManager.Mode == SteamNetworkMode.Host)
					SteamNetworkManager.DestroyHostSession();
				else
					SteamNetworkManager.DestroyClient();
				IsConnected = false;
			}
			return;
		}
		SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
		throw new ArgumentException("steamNetworkingIdentity");
	}

	private void OnSteamNetConnectionStatusChangedConnectionProblem(HSteamNetConnection connection, SteamNetConnectionInfo_t connectionInfo, ESteamNetworkingConnectionState oldState)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (!IsConnected || connection == HSteamNetConnection.Invalid)
		{
			SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
			return;
		}
		CSteamID nil = CSteamID.Nil;
		SteamNetworkingIdentity identityRemote = connectionInfo.m_identityRemote;
		if ((int)identityRemote.m_eType == 16)
		{
			nil = identityRemote.GetSteamID();
			if (nil == CSteamID.Nil)
			{
				SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
				Melon<BonkWithFriendsMod>.Logger.Error("[OnSteamNetConnectionStatusChangedConnectionProblem] Steam user id null!");
			}
			else
			{
				SteamNetworkLobbyManager.LeaveLobby();
				if (SteamNetworkManager.Mode == SteamNetworkMode.Host)
					SteamNetworkManager.DestroyHostSession();
				else
					SteamNetworkManager.DestroyClient();
				IsConnected = false;
			}
			return;
		}
		SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
		throw new ArgumentException("steamNetworkingIdentity");
	}

	internal void FixedUpdate(float fixedDeltaTime, double fixedTimeAsDouble)
	{
	}

	internal void Update(float deltaTime, double timeAsDouble)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (_steamNetConnectionHandle == HSteamNetConnection.Invalid || !IsConnected)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		do
		{
			num2 = SteamNetworkingSockets.ReceiveMessagesOnConnection(_steamNetConnectionHandle, _steamNetworkMessageReceiveBuffer, _steamNetworkMessageReceiveBuffer.Length);
			if (num2 <= 0)
			{
				break;
			}
			if (SafeRW)
			{
				SafeReceiveAndDispatch(num2);
			}
			else
			{
				UnsafeReceiveAndDispatch(num2);
			}
			num++;
			if (num >= 4)
			{
				num = 0;
				break;
			}
		}
		while (num2 == _steamNetworkMessageReceiveBuffer.Length);
		if (timeAsDouble - _lastSendTime > 1.0 / 60.0)
		{
			int num3 = 0;
			SteamNetworkMessage result;
			while (_steamNetworkMessageSendQueue.TryDequeue(out result))
			{
				if (result == null)
				{
					continue;
				}
				try
				{
					if (SafeRW)
					{
						SafeSend(result, num3);
					}
					else
					{
						UnsafeSend(result, num3);
					}
				}
				catch (Exception ex)
				{
					Melon<BonkWithFriendsMod>.Logger.Error($"[Update] Failed to send message - Size: {result.GetLength()}, Flags: {result.SendFlags}, Error: {ex.Message}");
				}
				finally
				{
					result.Dispose();
				}
				num3++;
				if (num3 >= _steamNetworkMessageSendBuffer.Length)
				{
					break;
				}
			}
			_lastSendTime = timeAsDouble;
		}
		_keepAliveTimer -= deltaTime;
		if (_keepAliveTimer <= 0f)
		{
			SendMessage(new KeepAliveMessage());
			_keepAliveTimer = 5f;
		}
	}

	internal void LateUpdate(float deltaTime, double timeAsDouble)
	{
	}

	private void SafeReceiveAndDispatch(int receivedMessages)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Melon<BonkWithFriendsMod>.Logger.Msg($"{"SafeReceiveAndDispatch"} - {receivedMessages}");
		for (int i = 0; i < receivedMessages; i++)
		{
			IntPtr intPtr = _steamNetworkMessageReceiveBuffer[i];
			if (intPtr == IntPtr.Zero)
			{
				continue;
			}
			SteamNetworkingMessage_t steamNetworkingMessage_t = SteamNetworkingMessage_t.FromIntPtr(intPtr);
			try
			{
				using SteamNetworkMessage steamNetworkMessage = new SteamNetworkMessage(steamNetworkingMessage_t, SafeRW);
				_networkMessageDispatcher.Dispatch(steamNetworkMessage);
			}
			finally
			{
				SteamNetworkingMessage_t.Release(intPtr);
				_steamNetworkMessageReceiveBuffer[i] = IntPtr.Zero;
			}
		}
	}

	private unsafe void UnsafeReceiveAndDispatch(int receivedMessages)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < receivedMessages; i++)
		{
			IntPtr intPtr = _steamNetworkMessageReceiveBuffer[i];
			if (intPtr == IntPtr.Zero)
			{
				continue;
			}
			SteamNetworkingMessage_t steamNetworkingMessage_t = Unsafe.ReadUnaligned<SteamNetworkingMessage_t>(intPtr.ToPointer());
			try
			{
				using SteamNetworkMessage steamNetworkMessage = new SteamNetworkMessage(steamNetworkingMessage_t, SafeRW);
				_networkMessageDispatcher.Dispatch(steamNetworkMessage);
			}
			finally
			{
				SteamNetworkingMessage_t.Release(intPtr);
				_steamNetworkMessageReceiveBuffer[i] = IntPtr.Zero;
			}
		}
	}

	private void SafeSend(SteamNetworkMessage steamNetworkMessage, int sentMessages)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		byte[] buffer = steamNetworkMessage.GetBuffer();
		long length = steamNetworkMessage.GetLength();
		if (length >= int.MaxValue)
		{
			throw new InvalidOperationException();
		}
		HSteamNetConnection steamNetConnectionHandle = steamNetworkMessage.SteamNetConnectionHandle;
		int sendFlags = (int)steamNetworkMessage.SendFlags;
		GCHandle gCHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
		try
		{
			long num = default(long);
			EResult value = SteamNetworkingSockets.SendMessageToConnection(steamNetConnectionHandle, gCHandle.AddrOfPinnedObject(), (uint)length, sendFlags, out num);
			Melon<BonkWithFriendsMod>.Logger.Msg($"nameof{new Action<SteamNetworkMessage, int>(SafeSend)} - {value}");
		}
		finally
		{
			gCHandle.Free();
		}
	}

	private void UnsafeSend(SteamNetworkMessage steamNetworkMessage, int sentMessages)
	{
	}

	internal void SendMessage<TMsg>(TMsg tMsg) where TMsg : MessageBase
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (!IsConnected || _steamNetConnectionHandle == HSteamNetConnection.Invalid)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning("[SendMessage] Dropped " + typeof(TMsg).Name + " - client not connected (IsConnected=" + IsConnected + ")");
			return;
		}
		try
		{
			(MessageType messageType, MessageSendFlags messageSendFlags) messageTypeAndSendFlags = _networkMessageDispatcher.GetMessageTypeAndSendFlags(tMsg.GetType());
			MessageType item = messageTypeAndSendFlags.messageType;
			MessageSendFlags item2 = messageTypeAndSendFlags.messageSendFlags;
			SteamNetworkMessage steamNetworkMessage = new SteamNetworkMessage(_remoteSteamUserId, _steamNetConnectionHandle, item, item2);
			steamNetworkMessage.Serialize(tMsg);
			_steamNetworkMessageSendQueue.Enqueue(steamNetworkMessage);
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[SendMessage] Failed to send " + typeof(TMsg).Name + ": " + ex.Message);
		}
	}

	private void SetState(SteamNetworkClientState steamNetworkClientState)
	{
		State = steamNetworkClientState;
	}

	[NetworkMessageHandler(MessageType.KeepAlive)]
	private void HandleKeepAlive(SteamNetworkMessage steamNetworkMessage)
	{
		Melon<BonkWithFriendsMod>.Logger.Msg("SteamNetworkClient.HandleKeepAlive");
	}

	[NetworkMessageHandler(MessageType.HostWelcome)]
	private void HandleHostWelcome(SteamNetworkMessage steamNetworkMessage)
	{
		HostWelcomeMessage hostWelcomeMessage = steamNetworkMessage.Deserialize<HostWelcomeMessage>();
		Melon<BonkWithFriendsMod>.Logger.Msg($"[Client] Welcome received. Spawning {hostWelcomeMessage.ExistingPlayers.Count} existing players.");
		foreach (HostWelcomeMessage.PlayerInfo existingPlayer in hostWelcomeMessage.ExistingPlayers)
		{
			_ = existingPlayer;
		}
	}

	[NetworkMessageHandler(MessageType.SeedSync)]
	private void HandleSeedSync(SteamNetworkMessage steamNetworkMessage)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		SeedSyncMessage seedSyncMessage = steamNetworkMessage.Deserialize<SeedSyncMessage>();
		SteamNetworkManager.NetworkSeed = seedSyncMessage.Seed;
		UnityEngine.Random.InitState(seedSyncMessage.Seed);
		MapGenerator.seed = seedSyncMessage.Seed;
		MyRandom.random = (Il2CppSystem.Random)new ConsistentRandom(seedSyncMessage.Seed);
		Melon<BonkWithFriendsMod>.Logger.Msg($"[Client] Seed synced: {seedSyncMessage.Seed}");
	}

	[NetworkMessageHandler(MessageType.InteractableSpawnBatch)]
	private void HandleInteractableSpawnBatch(SteamNetworkMessage steamNetworkMessage)
	{
		InteractableSpawnBatchMessage interactableSpawnBatchMessage = steamNetworkMessage.Deserialize<InteractableSpawnBatchMessage>();
		Melon<BonkWithFriendsMod>.Logger.Msg($"[Client] Receiving {interactableSpawnBatchMessage.Spawns.Count} interactable spawns from host.");
		SpawnInteractablesPatches.HandleInteractableSpawnBatch(interactableSpawnBatchMessage);
	}

	[NetworkMessageHandler(MessageType.InteractableUsed)]
	private void HandleInteractableUsed(SteamNetworkMessage steamNetworkMessage)
	{
		InteractableUsedMessage interactableUsedMessage = steamNetworkMessage.Deserialize<InteractableUsedMessage>();
		Melon<BonkWithFriendsMod>.Logger.Msg($"[Client] Player {interactableUsedMessage.PlayerSteamId} used interactable: ID={interactableUsedMessage.InteractableId}");
		SpawnInteractablesPatches.HandleInteractableUsed(interactableUsedMessage);
	}

	[NetworkMessageHandler(MessageType.PickupSpawned)]
	private void HandlePickupSpawn(SteamNetworkMessage steamNetworkMessage)
	{
		PickupSpawnManager.HandlePickupSpawn(steamNetworkMessage.Deserialize<PickupSpawnedMessage>());
	}

	private void Reset()
	{
		CloseConnection();
		UnsubscribeFromCallbacksAndCallResults();
		ResetNetworkMessageDispatcher();
		Array.Clear(_steamNetworkMessageReceiveBuffer);
		Array.Clear(_steamNetworkMessageSendBuffer);
		Array.Clear(_steamNetworkMessageSendResults);
		_steamNetworkMessageSendQueue.Clear();
		Instance = null;
	}

	private void CloseConnection()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!(_steamNetConnectionHandle == HSteamNetConnection.Invalid))
		{
			SteamNetworkingSockets.CloseConnection(_steamNetConnectionHandle, 0, string.Empty, true);
		}
	}

	private void UnsubscribeFromCallbacksAndCallResults()
	{
		SteamNetworkingImpl.OnSteamNetAuthenticationStatusCurrent = (SteamNetworkingImpl.SteamNetAuthenticationStatusCurrentCallbackDelegate)Delegate.Remove(SteamNetworkingImpl.OnSteamNetAuthenticationStatusCurrent, new SteamNetworkingImpl.SteamNetAuthenticationStatusCurrentCallbackDelegate(OnSteamNetAuthenticationStatusCurrent));
		SteamNetworkingImpl.OnSteamRelayNetworkStatusCurrent = (SteamNetworkingImpl.SteamRelayNetworkStatusCurrentCallbackDelegate)Delegate.Remove(SteamNetworkingImpl.OnSteamRelayNetworkStatusCurrent, new SteamNetworkingImpl.SteamRelayNetworkStatusCurrentCallbackDelegate(OnSteamRelayNetworkStatusCurrent));
		SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionAccepted = (SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionAcceptedCallbackDelegate)Delegate.Remove(SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionAccepted, new SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionAcceptedCallbackDelegate(OnSteamNetConnectionStatusChangedConnectionAccepted));
		SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionClosedOrRejected = (SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionClosedOrRejectedCallbackDelegate)Delegate.Remove(SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionClosedOrRejected, new SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionClosedOrRejectedCallbackDelegate(OnSteamNetConnectionStatusChangedConnectionClosedOrRejected));
		SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionProblem = (SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionProblemCallbackDelegate)Delegate.Remove(SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionProblem, new SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionProblemCallbackDelegate(OnSteamNetConnectionStatusChangedConnectionProblem));
	}

	private void ResetNetworkMessageDispatcher()
	{
		_networkMessageDispatcher?.Reset();
	}

	private void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Reset();
			}
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public async ValueTask DisposeAsync()
	{
		Dispose();
	}
}
