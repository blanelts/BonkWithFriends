using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using Megabonk.BonkWithFriends.Networking.Messages;
using UnityEngine;

namespace Megabonk.BonkWithFriends.IO;

internal sealed class NetworkReader : BinaryReader
{
	internal NetworkReader(Stream input)
		: base(input)
	{
	}

	internal NetworkReader(Stream input, Encoding encoding)
		: base(input, encoding)
	{
	}

	internal NetworkReader(Stream input, Encoding encoding, bool leaveOpen)
		: base(input, encoding, leaveOpen)
	{
	}

	internal Vector2 ReadVector2()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(ReadSingle(), ReadSingle());
	}

	internal Vector3 ReadVector3()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
	}

	internal Vector3 ReadVector3Fast()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Span<byte> buffer = stackalloc byte[12];
		Read(buffer);
		return new Vector3(BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(0, 4)), BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(4, 8)), BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(8, 12)));
	}

	internal Vector4 ReadVector4()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
	}

	internal Quaternion ReadQuaternion()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new Quaternion(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
	}

	internal Quaternion ReadQuaternionFast()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Span<byte> buffer = stackalloc byte[16];
		Read(buffer);
		return new Quaternion(BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(0, 4)), BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(4, 8)), BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(8, 12)), BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(12, 16)));
	}

	internal Tuple<Quaternion, Vector3> ReadTransform()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new Tuple<Quaternion, Vector3>(ReadQuaternion(), ReadVector3());
	}

	internal Ray ReadRay()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Ray(ReadVector3(), ReadVector3());
	}

	internal Rect ReadRect()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new Rect(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
	}

	internal Matrix4x4 ReadMatrix4x4()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new Matrix4x4(ReadVector4(), ReadVector4(), ReadVector4(), ReadVector4());
	}

	internal Color ReadColor()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new Color(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
	}

	internal Color32 ReadColor32()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new Color32(ReadByte(), ReadByte(), ReadByte(), ReadByte());
	}

	internal MessageType ReadMessageType()
	{
		return (MessageType)ReadUInt16();
	}
}
