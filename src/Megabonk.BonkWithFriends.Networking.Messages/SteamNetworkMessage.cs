using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using Megabonk.BonkWithFriends.IO;
using Steamworks;

namespace Megabonk.BonkWithFriends.Networking.Messages;

internal sealed class SteamNetworkMessage : IDisposable
{
	private const int DefaultMemoryStreamSize = 600;

	private bool _disposedValue;

	private MemoryStream _stream;

	private UnmanagedMemoryStream _unmanagedStream;

	private NetworkWriter _writer;

	private NetworkReader _reader;

	private bool _safeRW;

	private byte[] _pooledBuffer;

	internal MessageType Type;

	internal MessageSendFlags SendFlags;

	internal CSteamID SteamUserId;

	internal HSteamNetConnection SteamNetConnectionHandle;

	internal SteamNetworkMessage(CSteamID steamUserId, HSteamNetConnection steamNetConnectionHandle, MessageType messageType, MessageSendFlags messageSendFlags)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		SteamUserId = steamUserId;
		if (SteamUserId == CSteamID.Nil)
		{
			throw new ArgumentNullException("steamUserId");
		}
		SteamNetConnectionHandle = steamNetConnectionHandle;
		if (SteamNetConnectionHandle == HSteamNetConnection.Invalid)
		{
			throw new ArgumentNullException("steamNetConnectionHandle");
		}
		Type = messageType;
		if (Type == MessageType.None)
		{
			throw new ArgumentNullException("messageType");
		}
		SendFlags = messageSendFlags;
		_stream = new MemoryStream(600);
		_writer = new NetworkWriter(_stream);
	}

	internal SteamNetworkMessage(SteamNetworkingMessage_t steamNetworkingMessage_t, bool safeReadingAndWriting)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		IntPtr pData = steamNetworkingMessage_t.m_pData;
		if (pData == IntPtr.Zero)
		{
			throw new ArgumentNullException("dataPointer");
		}
		int cbSize = steamNetworkingMessage_t.m_cbSize;
		if (cbSize < 2)
		{
			throw new ArgumentOutOfRangeException("dataSize");
		}
		_safeRW = safeReadingAndWriting;
		if (_safeRW)
		{
			SetupSafeReading(pData, cbSize);
		}
		else
		{
			SetupUnsafeReading(pData, cbSize);
		}
		if (!ParseHeader(ref Type))
		{
			throw new ArgumentOutOfRangeException("Type");
		}
		HSteamNetConnection conn = steamNetworkingMessage_t.m_conn;
		if (conn == HSteamNetConnection.Invalid)
		{
			throw new ArgumentNullException("steamNetConnectionHandle");
		}
		SteamNetworkingIdentity identityPeer = steamNetworkingMessage_t.m_identityPeer;
		CSteamID steamID = identityPeer.GetSteamID();
		if (steamID == CSteamID.Nil)
		{
			throw new ArgumentNullException("steamNetworkingIdentity");
		}
		SetSenderInformation(conn, steamID);
	}

	private void SetupSafeReading(IntPtr dataPointer, int dataSize)
	{
		_pooledBuffer = ArrayPool<byte>.Shared.Rent(dataSize);
		Marshal.Copy(dataPointer, _pooledBuffer, 0, dataSize);
		_stream = new MemoryStream(_pooledBuffer);
		_reader = new NetworkReader(_stream);
	}

	private unsafe void SetupUnsafeReading(IntPtr dataPointer, int dataSize)
	{
		_unmanagedStream = new UnmanagedMemoryStream((byte*)dataPointer.ToPointer(), dataSize);
		_reader = new NetworkReader(_unmanagedStream);
	}

	private bool ParseHeader(ref MessageType messageType)
	{
		messageType = _reader.ReadMessageType();
		if (messageType == MessageType.None)
		{
			return false;
		}
		return true;
	}

	private void SetSenderInformation(HSteamNetConnection steamNetConnectionHandle, CSteamID steamUserId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		SteamNetConnectionHandle = steamNetConnectionHandle;
		SteamUserId = steamUserId;
	}

	internal TMsg Deserialize<TMsg>() where TMsg : MessageBase, new()
	{
		if (_reader == null)
		{
			throw new NullReferenceException("_reader");
		}
		TMsg val = new TMsg();
		val.Deserialize(_reader);
		return val;
	}

	internal void Deserialize<TMsg>(TMsg tMsg) where TMsg : MessageBase
	{
		if (_reader == null)
		{
			throw new NullReferenceException("_reader");
		}
		tMsg.Deserialize(_reader);
	}

	internal void Serialize<TMsg>(TMsg tMsg) where TMsg : MessageBase
	{
		if (tMsg == null || _writer == null)
		{
			throw new NullReferenceException("tMsg, _writer");
		}
		_writer.WriteMessageType(Type);
		tMsg.Serialize(_writer);
	}

	internal byte[] ToArray()
	{
		if (_stream == null)
		{
			return null;
		}
		return _stream.ToArray();
	}

	internal byte[] GetBuffer()
	{
		if (_stream == null)
		{
			return null;
		}
		return _stream.GetBuffer();
	}

	internal bool TryGetBuffer(out ArraySegment<byte> buffer)
	{
		if (_stream != null)
		{
			return _stream.TryGetBuffer(out buffer);
		}
		buffer = default(ArraySegment<byte>);
		return false;
	}

	internal long GetLength()
	{
		if (_stream == null)
		{
			return 0L;
		}
		return _stream.Length;
	}

	private void Dispose(bool disposing)
	{
		if (_disposedValue)
		{
			return;
		}
		if (disposing)
		{
			_reader?.Dispose();
			_writer?.Dispose();
			_stream?.Dispose();
			_unmanagedStream?.Dispose();
			byte[] pooledBuffer = _pooledBuffer;
			if (pooledBuffer != null && pooledBuffer.Length > 0)
			{
				ArrayPool<byte>.Shared.Return(_pooledBuffer);
				_pooledBuffer = null;
			}
		}
		_disposedValue = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
