using UnityEngine;

namespace Megabonk.BonkWithFriends.Net;

public static class Quant
{
	public const float POS_UNIT = 0.05f;

	public const float VEL_UNIT = 0.16f;

	public const float AVEL_UNIT = 3f;

	public static short QPos(float v)
	{
		return (short)Mathf.Clamp(Mathf.RoundToInt(v / 0.05f), -32768, 32767);
	}

	public static float DPos(short q)
	{
		return (float)q * 0.05f;
	}

	public static sbyte QVel(float v)
	{
		return (sbyte)Mathf.Clamp(Mathf.RoundToInt(v / 0.16f), -128, 127);
	}

	public static float DVel(sbyte q)
	{
		return (float)q * 0.16f;
	}

	public static byte QYaw(float deg)
	{
		return (byte)Mathf.Clamp(Mathf.RoundToInt((deg % 360f + 360f) % 360f * (17f / 24f)), 0, 255);
	}

	public static float DYaw(byte q)
	{
		return (float)(int)q * 1.4117647f;
	}

	public static sbyte QAngVel(float degPerSec)
	{
		return (sbyte)Mathf.Clamp(Mathf.RoundToInt(degPerSec / 3f), -128, 127);
	}

	public static float DAngVel(sbyte q)
	{
		return (float)q * 3f;
	}
}
