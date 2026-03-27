using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using Megabonk.BonkWithFriends.Networking.Messages;
using UnityEngine;

namespace Megabonk.BonkWithFriends.IO;

internal sealed class NetworkWriter : BinaryWriter
{
	internal NetworkWriter(Stream output)
		: base(output)
	{
	}

	internal NetworkWriter(Stream output, Encoding encoding)
		: base(output, encoding)
	{
	}

	internal NetworkWriter(Stream output, Encoding encoding, bool leaveOpen)
		: base(output, encoding, leaveOpen)
	{
	}

	internal void WriteVector2(Vector2 vector2)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Write(vector2.x);
		Write(vector2.y);
	}

	internal void WriteVector3(Vector3 vector3)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Write(vector3.x);
		Write(vector3.y);
		Write(vector3.z);
	}

	internal void WriteVector3Fast(Vector3 vector3)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Span<byte> span = stackalloc byte[12];
		BinaryPrimitives.WriteSingleLittleEndian(span, vector3.x);
		BinaryPrimitives.WriteSingleLittleEndian(span, vector3.y);
		BinaryPrimitives.WriteSingleLittleEndian(span, vector3.z);
		OutStream.Write(span);
	}

	internal void WriteVector4(Vector4 vector4)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Write(vector4.x);
		Write(vector4.y);
		Write(vector4.z);
		Write(vector4.w);
	}

	internal void WriteQuaternion(Quaternion quaternion)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Write(quaternion.x);
		Write(quaternion.y);
		Write(quaternion.z);
		Write(quaternion.w);
	}

	internal void WriteQuaternionFast(Quaternion quaternion)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Span<byte> span = stackalloc byte[16];
		BinaryPrimitives.WriteSingleLittleEndian(span, quaternion.x);
		BinaryPrimitives.WriteSingleLittleEndian(span, quaternion.y);
		BinaryPrimitives.WriteSingleLittleEndian(span, quaternion.z);
		BinaryPrimitives.WriteSingleLittleEndian(span, quaternion.w);
		OutStream.Write(span);
	}

	internal void WriteTransform(Transform transform)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		WriteQuaternion(transform.rotation);
		WriteVector3(transform.position);
	}

	internal void WriteRay(Ray ray)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		WriteVector3(ray.origin);
		WriteVector3(ray.direction);
	}

	internal void WriteRect(Rect rect)
	{
		Write(rect.x);
		Write(rect.y);
		Write(rect.width);
		Write(rect.height);
	}

	internal void WriteMatrix4x4(Matrix4x4 matrix)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		Write(matrix.m00);
		Write(matrix.m01);
		Write(matrix.m02);
		Write(matrix.m03);
		Write(matrix.m10);
		Write(matrix.m11);
		Write(matrix.m12);
		Write(matrix.m13);
		Write(matrix.m20);
		Write(matrix.m21);
		Write(matrix.m22);
		Write(matrix.m23);
		Write(matrix.m30);
		Write(matrix.m31);
		Write(matrix.m32);
		Write(matrix.m33);
	}

	internal void WriteColor(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Write(color.r);
		Write(color.g);
		Write(color.b);
		Write(color.a);
	}

	internal void WriteColor32(Color32 color32)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Write(color32.r);
		Write(color32.g);
		Write(color32.b);
		Write(color32.a);
	}

	internal void WriteMessageType(MessageType messageType)
	{
		Write((ushort)messageType);
	}
}
