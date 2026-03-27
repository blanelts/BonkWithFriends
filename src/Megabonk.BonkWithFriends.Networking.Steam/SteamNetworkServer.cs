using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Megabonk.BonkWithFriends.Networking.Messages;
using Megabonk.BonkWithFriends.Networking.Messages.Client;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Steam;
using MelonLoader;
using Steamworks;

namespace Megabonk.BonkWithFriends.Networking.Steam;

internal sealed class SteamNetworkServer : IDisposable, IAsyncDisposable
{
	internal const int VirtualPort = 0;

	private const int SteamNetworkMessageBufferSize = 512;

	private const int MaxNetworkReadsPerFrame = 4;

	private const int MaxNetworkWritesPerFrame = 4;

	private const double SendRate = 1.0 / 60.0;

	private const float KeepAliveInterval = 5f;

	internal static SteamNetworkServer Instance;

	private object _syncRoot = new object();

	private bool _disposedValue;

	private bool _netAuthenticationStatusCurrent;

	private bool _relayNetworkAccessStatusCurrent;

	private HSteamListenSocket _steamListenSocketHandle;

	private HSteamNetPollGroup _steamNetPollGroupHandle;

	private Dictionary<CSteamID, HSteamNetConnection> _connections;

	private Dictionary<HSteamNetConnection, HSteamNetPollGroup> _connectionPollGroup;

	private IntPtr[] _steamNetworkMessageReceiveBuffer;

	private IntPtr[] _steamNetworkMessageSendBuffer;

	private long[] _steamNetworkMessageSendResults;

	private ConcurrentQueue<SteamNetworkMessage> _steamNetworkMessageSendQueue;

	private NetworkMessageDispatcher _networkMessageDispatcher;

	private double _lastSendTime;

	private float _keepAliveTimer = 5f;

	internal ServerReadyToListenDelegate OnReadyToListen;

	internal bool IsListening { get; private set; }

	internal bool SafeRW { get; private set; }

	internal SteamNetworkServerState State { get; private set; }

	internal SteamNetworkServer(bool safeReadingAndWriting = true)
	{
		if (Instance != null)
		{
			throw new InvalidOperationException();
		}
		Instance = this;
		_connections = new Dictionary<CSteamID, HSteamNetConnection>(SteamNetworkLobbyManager.MaxPlayers);
		_connectionPollGroup = new Dictionary<HSteamNetConnection, HSteamNetPollGroup>(SteamNetworkLobbyManager.MaxPlayers);
		_steamNetworkMessageReceiveBuffer = new IntPtr[512];
		_steamNetworkMessageSendBuffer = new IntPtr[512];
		_steamNetworkMessageSendResults = new long[512];
		_steamNetworkMessageSendQueue = new ConcurrentQueue<SteamNetworkMessage>();
		_networkMessageDispatcher = new NetworkMessageDispatcher(isServer: true);
		SafeRW = safeReadingAndWriting;
		Setup();
		CheckSteamNetworkStatus();
	}

	private void Setup()
	{
		SubscribeToCallbacksAndCallResults();
	}

	private void SubscribeToCallbacksAndCallResults()
	{
		SteamNetworkingImpl.OnSteamNetAuthenticationStatusCurrent = (SteamNetworkingImpl.SteamNetAuthenticationStatusCurrentCallbackDelegate)Delegate.Combine(SteamNetworkingImpl.OnSteamNetAuthenticationStatusCurrent, new SteamNetworkingImpl.SteamNetAuthenticationStatusCurrentCallbackDelegate(OnSteamNetAuthenticationStatusCurrent));
		SteamNetworkingImpl.OnSteamRelayNetworkStatusCurrent = (SteamNetworkingImpl.SteamRelayNetworkStatusCurrentCallbackDelegate)Delegate.Combine(SteamNetworkingImpl.OnSteamRelayNetworkStatusCurrent, new SteamNetworkingImpl.SteamRelayNetworkStatusCurrentCallbackDelegate(OnSteamRelayNetworkStatusCurrent));
		SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionRequest = (SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionRequestCallbackDelegate)Delegate.Combine(SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionRequest, new SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionRequestCallbackDelegate(OnSteamNetConnectionStatusChangedConnectionRequest));
		SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionClosedOrRejected = (SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionClosedOrRejectedCallbackDelegate)Delegate.Combine(SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionClosedOrRejected, new SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionClosedOrRejectedCallbackDelegate(OnSteamNetConnectionStatusChangedConnectionClosedOrRejected));
		SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionProblem = (SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionProblemCallbackDelegate)Delegate.Combine(SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionProblem, new SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionProblemCallbackDelegate(OnSteamNetConnectionStatusChangedConnectionProblem));
	}

	private void CheckSteamNetworkStatus()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		SteamRelayNetworkStatus_t val = default(SteamRelayNetworkStatus_t);
		if ((int)SteamNetworkingUtils.GetRelayNetworkStatus(out val) == 100)
		{
			OnSteamRelayNetworkStatusCurrent();
		}
		SteamNetAuthenticationStatus_t val2 = default(SteamNetAuthenticationStatus_t);
		if ((int)SteamNetworkingSockets.GetAuthenticationStatus(out val2) == 100)
		{
			OnSteamNetAuthenticationStatusCurrent();
		}
		if (!_relayNetworkAccessStatusCurrent || !_netAuthenticationStatusCurrent)
		{
			Task.Run((Func<ValueTask>)CheckSteamNetworkStatusAsync);
		}
	}

	private async ValueTask CheckSteamNetworkStatusAsync()
	{
		SteamRelayNetworkStatus_t val = default(SteamRelayNetworkStatus_t);
		SteamNetAuthenticationStatus_t val2 = default(SteamNetAuthenticationStatus_t);
		while ((int)SteamNetworkingUtils.GetRelayNetworkStatus(out val) != 100 || (int)SteamNetworkingSockets.GetAuthenticationStatus(out val2) != 100)
		{
			await Task.Delay(TimeSpan.FromMilliseconds(100.0)).ConfigureAwait(continueOnCapturedContext: false);
		}
		SynchronizationContext.SetSynchronizationContext(BonkWithFriendsMod.Instance.MainThreadSyncContext);
		OnSteamRelayNetworkStatusCurrent();
		OnSteamNetAuthenticationStatusCurrent();
		OnReadyToListen?.Invoke();
	}

	private bool IsReadyToListen()
	{
		if (_netAuthenticationStatusCurrent)
		{
			return _relayNetworkAccessStatusCurrent;
		}
		return false;
	}

	internal void StartListening()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (!IsReadyToListen())
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[StartListening] Not ready to listen!");
			return;
		}
		lock (_syncRoot)
		{
			SteamNetworkingConfigValue_t[] array = Array.Empty<SteamNetworkingConfigValue_t>();
			_steamListenSocketHandle = SteamNetworkingSockets.CreateListenSocketP2P(0, array.Length, array);
			if (_steamListenSocketHandle == HSteamListenSocket.Invalid)
			{
				Melon<BonkWithFriendsMod>.Logger.Error("[StartListening] Steam listen socket handle invalid!");
				return;
			}
			_steamNetPollGroupHandle = SteamNetworkingSockets.CreatePollGroup();
			if (_steamNetPollGroupHandle == HSteamNetPollGroup.Invalid)
			{
				Melon<BonkWithFriendsMod>.Logger.Error("[StartListening] Steam poll group handle invalid!");
				return;
			}
		}
		IsListening = true;
		Melon<BonkWithFriendsMod>.Logger.Msg("Server listening..");
	}

	private void OnSteamNetAuthenticationStatusCurrent()
	{
		if (!_netAuthenticationStatusCurrent)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("OnSteamNetAuthenticationStatusCurrent");
			_netAuthenticationStatusCurrent = true;
		}
	}

	private void OnSteamRelayNetworkStatusCurrent()
	{
		if (!_relayNetworkAccessStatusCurrent)
		{
			Melon<BonkWithFriendsMod>.Logger.Msg("OnSteamRelayNetworkStatusCurrent");
			_relayNetworkAccessStatusCurrent = true;
		}
	}

	private void OnSteamNetConnectionStatusChangedConnectionRequest(HSteamNetConnection connection, SteamNetConnectionInfo_t connectionInfo, ESteamNetworkingConnectionState oldState)
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
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Invalid comparison between Unknown and I4
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Invalid comparison between Unknown and I4
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		if (!IsListening || connection == HSteamNetConnection.Invalid)
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
				Melon<BonkWithFriendsMod>.Logger.Error("[OnSteamNetConnectionStatusChangedConnectionRequest] Steam user id null!");
				return;
			}
			if (!SteamManager.Instance.Lobby.HasMember(nil))
			{
				SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
				Melon<BonkWithFriendsMod>.Logger.Error("[OnSteamNetConnectionStatusChangedConnectionRequest] Attempted connection is not from a member in our lobby!");
				return;
			}
			EResult val = SteamNetworkingSockets.AcceptConnection(connection);
			if ((int)val == 8 || (int)val == 11)
			{
				SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
				Melon<BonkWithFriendsMod>.Logger.Error("[OnSteamNetConnectionStatusChangedConnectionRequest] Steam connection handle invalid!");
				return;
			}
			lock (_syncRoot)
			{
				_connections[nil] = connection;
				if (!SteamNetworkingSockets.SetConnectionPollGroup(connection, _steamNetPollGroupHandle))
				{
					Melon<BonkWithFriendsMod>.Logger.Error("[OnSteamNetConnectionStatusChangedConnectionRequest] Steam connection or poll group handle invalid!");
					return;
				}
				_connectionPollGroup[connection] = _steamNetPollGroupHandle;
			}
			Melon<BonkWithFriendsMod>.Logger.Msg($"[{"OnSteamNetConnectionStatusChangedConnectionRequest"}] Established p2p connection with {nil}");
			return;
		}
		SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
		throw new ArgumentException("steamNetworkingIdentity");
	}

	private void OnSteamNetConnectionStatusChangedConnectionClosedOrRejected(HSteamNetConnection connection, SteamNetConnectionInfo_t connectionInfo, ESteamNetworkingConnectionState oldState)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		if (!IsListening || connection == HSteamNetConnection.Invalid)
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
				return;
			}
			lock (_syncRoot)
			{
				if (_connectionPollGroup.TryGetValue(connection, out var _) && SteamNetworkingSockets.SetConnectionPollGroup(connection, HSteamNetPollGroup.Invalid) && !_connectionPollGroup.Remove(connection))
				{
					Melon<BonkWithFriendsMod>.Logger.Error("[OnSteamNetConnectionStatusChangedConnectionClosedOrRejected] Unable to unassign connection from poll group!");
				}
				if (_connections.ContainsValue(connection))
				{
					KeyValuePair<CSteamID, HSteamNetConnection> keyValuePair = _connections.FirstOrDefault((KeyValuePair<CSteamID, HSteamNetConnection> c) => c.Value == connection);
					if (keyValuePair.Key == CSteamID.Nil || keyValuePair.Value == HSteamNetConnection.Invalid)
					{
						throw new NullReferenceException("entry");
					}
					if (!_connections.Remove(keyValuePair.Key))
					{
						Melon<BonkWithFriendsMod>.Logger.Error("[OnSteamNetConnectionStatusChangedConnectionClosedOrRejected] Unable to remove connection!");
					}
					SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
					Melon<BonkWithFriendsMod>.Logger.Msg($"[{"OnSteamNetConnectionStatusChangedConnectionClosedOrRejected"}] Closed p2p connection with {nil}");
				}
				return;
			}
		}
		SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
		throw new ArgumentException("steamNetworkingIdentity");
	}

	private void OnSteamNetConnectionStatusChangedConnectionProblem(HSteamNetConnection connection, SteamNetConnectionInfo_t connectionInfo, ESteamNetworkingConnectionState oldState)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		if (!IsListening || connection == HSteamNetConnection.Invalid)
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
				return;
			}
			lock (_syncRoot)
			{
				if (_connectionPollGroup.TryGetValue(connection, out var _) && SteamNetworkingSockets.SetConnectionPollGroup(connection, HSteamNetPollGroup.Invalid) && !_connectionPollGroup.Remove(connection))
				{
					Melon<BonkWithFriendsMod>.Logger.Error("[OnSteamNetConnectionStatusChangedConnectionProblem] Unable to unassign connection from poll group!");
				}
				if (_connections.ContainsValue(connection))
				{
					KeyValuePair<CSteamID, HSteamNetConnection> keyValuePair = _connections.FirstOrDefault((KeyValuePair<CSteamID, HSteamNetConnection> c) => c.Value == connection);
					if (keyValuePair.Key == CSteamID.Nil || keyValuePair.Value == HSteamNetConnection.Invalid)
					{
						throw new NullReferenceException("entry");
					}
					if (!_connections.Remove(keyValuePair.Key))
					{
						Melon<BonkWithFriendsMod>.Logger.Error("[OnSteamNetConnectionStatusChangedConnectionProblem] Unable to remove connection!");
					}
					SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, true);
					Melon<BonkWithFriendsMod>.Logger.Msg($"[{"OnSteamNetConnectionStatusChangedConnectionProblem"}] Closed p2p connection with {nil}");
				}
				return;
			}
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
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (_steamListenSocketHandle == HSteamListenSocket.Invalid || _steamNetPollGroupHandle == HSteamNetPollGroup.Invalid || !IsListening)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		do
		{
			num2 = SteamNetworkingSockets.ReceiveMessagesOnPollGroup(_steamNetPollGroupHandle, _steamNetworkMessageReceiveBuffer, _steamNetworkMessageReceiveBuffer.Length);
			if (num2 <= 0)
			{
				break;
			}
			if (SafeRW)
			{
				SafeUpdate(num2);
			}
			else
			{
				UnsafeUpdate(num2);
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
			BroadcastToRemoteClients(new KeepAliveMessage());
			_keepAliveTimer = 5f;
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

	internal void SendMessage<TMsg>(TMsg tMsg, CSteamID steamUserId, HSteamNetConnection steamNetConnectionHandle = default(HSteamNetConnection)) where TMsg : MessageBase
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!IsListening || _steamListenSocketHandle == HSteamListenSocket.Invalid || _steamNetPollGroupHandle == HSteamNetPollGroup.Invalid || steamUserId == CSteamID.Nil)
		{
			return;
		}
		if (steamNetConnectionHandle == HSteamNetConnection.Invalid)
		{
			lock (_syncRoot)
			{
				if (!_connections.TryGetValue(steamUserId, out steamNetConnectionHandle))
				{
					return;
				}
			}
		}
		var (messageType, messageSendFlags) = _networkMessageDispatcher.GetMessageTypeAndSendFlags(tMsg.GetType());
		try
		{
			SteamNetworkMessage steamNetworkMessage = new SteamNetworkMessage(steamUserId, steamNetConnectionHandle, messageType, messageSendFlags);
			steamNetworkMessage.Serialize(tMsg);
			_steamNetworkMessageSendQueue.Enqueue(steamNetworkMessage);
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[SendMessage] Failed to send " + typeof(TMsg).Name + ": " + ex.Message);
		}
	}

	internal void BroadcastMessage<TMsg>(TMsg tMsg) where TMsg : MessageBase
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (!IsListening || _steamListenSocketHandle == HSteamListenSocket.Invalid || _steamNetPollGroupHandle == HSteamNetPollGroup.Invalid)
		{
			return;
		}
		var (messageType, messageSendFlags) = _networkMessageDispatcher.GetMessageTypeAndSendFlags(tMsg.GetType());
		lock (_syncRoot)
		{
			foreach (KeyValuePair<CSteamID, HSteamNetConnection> connection in _connections)
			{
				try
				{
					CSteamID key = connection.Key;
					HSteamNetConnection value = connection.Value;
					SteamNetworkMessage steamNetworkMessage = new SteamNetworkMessage(key, value, messageType, messageSendFlags);
					steamNetworkMessage.Serialize(tMsg);
					_steamNetworkMessageSendQueue.Enqueue(steamNetworkMessage);
				}
				catch
				{
				}
			}
		}
	}

	internal void BroadcastToRemoteClients<TMsg>(TMsg tMsg) where TMsg : MessageBase
	{
		// Send to all clients except the host (host already has data from native game code)
		CSteamID hostId = SteamUser.GetSteamID();
		BroadcastMessageExcept(tMsg, hostId);
	}

	internal void BroadcastMessageExcept<TMsg>(TMsg tMsg, CSteamID excludedSteamUserId, HSteamNetConnection excludedSteamNetConnectionHandle = default(HSteamNetConnection)) where TMsg : MessageBase
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!IsListening || _steamListenSocketHandle == HSteamListenSocket.Invalid || _steamNetPollGroupHandle == HSteamNetPollGroup.Invalid || excludedSteamUserId == CSteamID.Nil)
		{
			return;
		}
		if (excludedSteamNetConnectionHandle == HSteamNetConnection.Invalid)
		{
			lock (_syncRoot)
			{
				if (!_connections.TryGetValue(excludedSteamUserId, out excludedSteamNetConnectionHandle))
				{
					return;
				}
			}
		}
		var (messageType, messageSendFlags) = _networkMessageDispatcher.GetMessageTypeAndSendFlags(tMsg.GetType());
		lock (_syncRoot)
		{
			foreach (KeyValuePair<CSteamID, HSteamNetConnection> connection in _connections)
			{
				try
				{
					CSteamID key = connection.Key;
					HSteamNetConnection value = connection.Value;
					if (!(key == excludedSteamUserId) && !(value == excludedSteamNetConnectionHandle))
					{
						SteamNetworkMessage steamNetworkMessage = new SteamNetworkMessage(key, value, messageType, messageSendFlags);
						steamNetworkMessage.Serialize(tMsg);
						_steamNetworkMessageSendQueue.Enqueue(steamNetworkMessage);
					}
				}
				catch
				{
				}
			}
		}
	}

	internal void LateUpdate(float deltaTime, double timeAsDouble)
	{
	}

	private void SafeUpdate(int receivedMessages)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
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

	private unsafe void UnsafeUpdate(int receivedMessages)
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

	[NetworkMessageHandler(MessageType.KeepAlive)]
	private void HandleKeepAlive(SteamNetworkMessage steamNetworkMessage)
	{
		Melon<BonkWithFriendsMod>.Logger.Msg("SteamNetworkServer.HandleKeepAlive");
	}

	[NetworkMessageHandler(MessageType.ClientIntroduce)]
	private void HandleClientIntroduce(SteamNetworkMessage steamNetworkMessage)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected I4, but got Unknown
		ClientIntroduceMessage clientIntroduceMessage = steamNetworkMessage.Deserialize<ClientIntroduceMessage>();
		CSteamID steamUserId = steamNetworkMessage.SteamUserId;
		Melon<BonkWithFriendsMod>.Logger.Msg($"[Server] Client {steamUserId} introduced with character {clientIntroduceMessage.Character}.");
		PlayerJoinedMessage tMsg = new PlayerJoinedMessage
		{
			Character = clientIntroduceMessage.Character
		};
		BroadcastMessageExcept(tMsg, steamUserId);
		HostWelcomeMessage hostWelcomeMessage = new HostWelcomeMessage();
		SteamNetworkLobby lobby = SteamManager.Instance.Lobby;
		if (lobby != null)
		{
			foreach (SteamNetworkLobbyMember member in lobby.Members)
			{
				if (!(member.UserId == steamUserId))
				{
					hostWelcomeMessage.ExistingPlayers.Add(new HostWelcomeMessage.PlayerInfo
					{
						SteamUserId = member.UserId.m_SteamID,
						Character = (int)member.Character
					});
				}
			}
		}
		SendMessage(hostWelcomeMessage, steamUserId);
	}

	[NetworkMessageHandler(MessageType.InteractableUsed)]
	private void HandleInteractableUsed(SteamNetworkMessage steamNetworkMessage)
	{
		InteractableUsedMessage tMsg = steamNetworkMessage.Deserialize<InteractableUsedMessage>();
		BroadcastMessageExcept(tMsg, steamNetworkMessage.SteamUserId);
	}

	private void SetState(SteamNetworkServerState steamNetworkServerState)
	{
		State = steamNetworkServerState;
	}

	internal void Reset()
	{
		CloseAllConnectionsAndDestroy();
		UnsubscribeFromCallbacksAndCallResults();
		ResetNetworkMessageDispatcher();
		Array.Clear(_steamNetworkMessageReceiveBuffer);
		Array.Clear(_steamNetworkMessageSendBuffer);
		Array.Clear(_steamNetworkMessageSendResults);
		_steamNetworkMessageSendQueue.Clear();
		Instance = null;
	}

	private void CloseAllConnectionsAndDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (_steamListenSocketHandle == HSteamListenSocket.Invalid)
		{
			return;
		}
		if (_steamNetPollGroupHandle != HSteamNetPollGroup.Invalid)
		{
			Dictionary<HSteamNetConnection, HSteamNetPollGroup> connectionPollGroup = _connectionPollGroup;
			if (connectionPollGroup != null && connectionPollGroup.Count > 0)
			{
				foreach (KeyValuePair<HSteamNetConnection, HSteamNetPollGroup> item in _connectionPollGroup)
				{
					if (!(item.Key == HSteamNetConnection.Invalid))
					{
						SteamNetworkingSockets.SetConnectionPollGroup(item.Key, HSteamNetPollGroup.Invalid);
					}
				}
				_connectionPollGroup.Clear();
			}
			SteamNetworkingSockets.DestroyPollGroup(_steamNetPollGroupHandle);
		}
		Dictionary<CSteamID, HSteamNetConnection> connections = _connections;
		if (connections != null && connections.Count > 0)
		{
			foreach (KeyValuePair<CSteamID, HSteamNetConnection> connection in _connections)
			{
				if (!(connection.Value == HSteamNetConnection.Invalid))
				{
					SteamNetworkingSockets.CloseConnection(connection.Value, 0, string.Empty, true);
				}
			}
			_connections.Clear();
		}
		SteamNetworkingSockets.CloseListenSocket(_steamListenSocketHandle);
	}

	private void UnsubscribeFromCallbacksAndCallResults()
	{
		SteamNetworkingImpl.OnSteamNetAuthenticationStatusCurrent = (SteamNetworkingImpl.SteamNetAuthenticationStatusCurrentCallbackDelegate)Delegate.Remove(SteamNetworkingImpl.OnSteamNetAuthenticationStatusCurrent, new SteamNetworkingImpl.SteamNetAuthenticationStatusCurrentCallbackDelegate(OnSteamNetAuthenticationStatusCurrent));
		SteamNetworkingImpl.OnSteamRelayNetworkStatusCurrent = (SteamNetworkingImpl.SteamRelayNetworkStatusCurrentCallbackDelegate)Delegate.Remove(SteamNetworkingImpl.OnSteamRelayNetworkStatusCurrent, new SteamNetworkingImpl.SteamRelayNetworkStatusCurrentCallbackDelegate(OnSteamRelayNetworkStatusCurrent));
		SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionRequest = (SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionRequestCallbackDelegate)Delegate.Remove(SteamNetworkingImpl.OnSteamNetConnectionStatusChangedConnectionRequest, new SteamNetworkingImpl.SteamNetConnectionStatusChangedConnectionRequestCallbackDelegate(OnSteamNetConnectionStatusChangedConnectionRequest));
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
