using System;
using System.Buffers;
using System.IO;
using System.Text;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Net;

public static class MsgIO
{
	public const ushort Protocol = 1;

	public static void WriteHeader(BinaryWriter w, Op op)
	{
		w.Write((ushort)1);
		w.Write((byte)op);
	}

	public static bool ReadHeader(BinaryReader r, out Op op)
	{
		op = (Op)0;
		if (r.ReadUInt16() != 1)
		{
			return false;
		}
		op = (Op)r.ReadByte();
		return true;
	}

	public static void WriteVec3(BinaryWriter w, Vector3 v)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		w.Write(v.x);
		w.Write(v.y);
		w.Write(v.z);
	}

	public static Vector3 ReadVec3(BinaryReader r)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
	}

	public static void WriteQuat(BinaryWriter w, Quaternion q)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		w.Write(q.x);
		w.Write(q.y);
		w.Write(q.z);
		w.Write(q.w);
	}

	public static Quaternion ReadQuat(BinaryReader r)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
	}

	public static void WriteU64(BinaryWriter w, ulong v)
	{
		w.Write(v);
	}

	public static ulong ReadU64(BinaryReader r)
	{
		return r.ReadUInt64();
	}

	public static void WriteU32(BinaryWriter w, uint v)
	{
		w.Write(v);
	}

	public static uint ReadU32(BinaryReader r)
	{
		return r.ReadUInt32();
	}

	public static void WriteString(BinaryWriter w, string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			w.Write(0);
			return;
		}
		Encoding uTF = Encoding.UTF8;
		int byteCount = uTF.GetByteCount(s);
		byte[] array = ArrayPool<byte>.Shared.Rent(byteCount);
		try
		{
			int bytes = uTF.GetBytes(s.AsSpan(), array);
			w.Write(bytes);
			w.Write(array, 0, bytes);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(array, clearArray: true);
		}
	}

	public static string ReadString(BinaryReader r)
	{
		int num = r.ReadInt32();
		if (num <= 0)
		{
			return string.Empty;
		}
		byte[] array = ArrayPool<byte>.Shared.Rent(num);
		try
		{
			if (r.Read(array, 0, num) != num)
			{
				throw new EndOfStreamException();
			}
			return Encoding.UTF8.GetString(array, 0, num);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(array, clearArray: true);
		}
	}
}
