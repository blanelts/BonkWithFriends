using System.Collections.Generic;
using System.Linq;
using System.Text;
using Megabonk.BonkWithFriends.Net;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Debug.UI;

public static class NetProfiler
{
	private class OpMetrics
	{
		public byte OpCode;

		public float Rate;

		public float BytesPerSec;

		public float AvgSize;

		public long TotalCount;

		public long TotalBytes;

		public string Trend;

		public float PeakBytesPerSec;
	}

	public class ProfilerData
	{
		public int Count;

		public int Bytes;

		public long TotalCount;

		public long TotalBytes;

		public void Update(int byteSize)
		{
			Count++;
			Bytes += byteSize;
			TotalCount++;
			TotalBytes += byteSize;
		}

		public void Reset()
		{
			Count = 0;
			Bytes = 0;
		}
	}

	private static readonly Dictionary<byte, ProfilerData> SentData = new Dictionary<byte, ProfilerData>();

	private static readonly Dictionary<byte, ProfilerData> ReceivedData = new Dictionary<byte, ProfilerData>();

	private static float _lastTickTime;

	private static readonly StringBuilder Sb = new StringBuilder();

	public static float MinRateThreshold = 0.1f;

	public static float BandwidthBudget = 204800f;

	private static readonly Dictionary<byte, float> LastSentBytesPerSec = new Dictionary<byte, float>();

	private static readonly Dictionary<byte, float> LastRecvBytesPerSec = new Dictionary<byte, float>();

	private static readonly Dictionary<byte, float> PeakSentBytesPerSec = new Dictionary<byte, float>();

	private static readonly Dictionary<byte, float> PeakRecvBytesPerSec = new Dictionary<byte, float>();

	private static float _lastMessageTime;

	private static float _noMessageDuration;

	public static string DisplayString { get; private set; } = "NetProfiler Initializing...";

	public static Dictionary<string, string> ColorCodes { get; private set; } = new Dictionary<string, string>();

	public static void TrackMessageSent(byte opCode, int byteSize)
	{
		if (!SentData.ContainsKey(opCode))
		{
			SentData[opCode] = new ProfilerData();
		}
		SentData[opCode].Update(byteSize);
		_lastMessageTime = Time.unscaledTime;
	}

	public static void TrackMessageReceived(byte opCode, int byteSize)
	{
		if (!ReceivedData.ContainsKey(opCode))
		{
			ReceivedData[opCode] = new ProfilerData();
		}
		ReceivedData[opCode].Update(byteSize);
		_lastMessageTime = Time.unscaledTime;
	}

	public static void UpdateDisplay()
	{
		float num = Time.unscaledTime - _lastTickTime;
		if (num == 0f)
		{
			num = 1f;
		}
		_lastTickTime = Time.unscaledTime;
		_noMessageDuration = Time.unscaledTime - _lastMessageTime;
		string value = ((_noMessageDuration > 2f) ? $" ⚠ No messages for {_noMessageDuration:F1}s!" : "");
		ColorCodes.Clear();
		Sb.Clear();
		Sb.AppendLine("=== Network Profiler ===");
		StringBuilder sb = Sb;
		StringBuilder stringBuilder = sb;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(43, 3, sb);
		handler.AppendLiteral("Update: ");
		handler.AppendFormatted(num, "F2");
		handler.AppendLiteral("s | Threshold: >");
		handler.AppendFormatted(MinRateThreshold, "F1");
		handler.AppendLiteral(" msg/s | F3: Toggle");
		handler.AppendFormatted(value);
		stringBuilder.AppendLine(ref handler);
		Sb.AppendLine();
		float num2 = CalculateTotalBandwidth(SentData, num);
		float num3 = CalculateTotalBandwidth(ReceivedData, num);
		float num4 = num2 + num3;
		string color;
		string budgetStatus = GetBudgetStatus(num4, out color);
		sb = Sb;
		StringBuilder stringBuilder2 = sb;
		handler = new StringBuilder.AppendInterpolatedStringHandler(27, 3, sb);
		handler.AppendLiteral("Bandwidth Budget: ");
		handler.AppendFormatted(FormatBytes(num4));
		handler.AppendLiteral("/s of ");
		handler.AppendFormatted(FormatBytes(BandwidthBudget));
		handler.AppendLiteral("/s ");
		handler.AppendFormatted(budgetStatus);
		stringBuilder2.AppendLine(ref handler);
		sb = Sb;
		StringBuilder stringBuilder3 = sb;
		handler = new StringBuilder.AppendInterpolatedStringHandler(21, 2, sb);
		handler.AppendLiteral("  Sent: ");
		handler.AppendFormatted(FormatBytes(num2));
		handler.AppendLiteral("/s | Recv: ");
		handler.AppendFormatted(FormatBytes(num3));
		handler.AppendLiteral("/s");
		stringBuilder3.AppendLine(ref handler);
		Sb.AppendLine();
		Sb.AppendLine("--- SENT (sorted by bandwidth) ---");
		AppendData(Sb, SentData, num, "SENT", LastSentBytesPerSec, PeakSentBytesPerSec);
		Sb.AppendLine();
		Sb.AppendLine("--- RECEIVED (sorted by bandwidth) ---");
		AppendData(Sb, ReceivedData, num, "RECV", LastRecvBytesPerSec, PeakRecvBytesPerSec);
		DisplayString = Sb.ToString();
	}

	private static float CalculateTotalBandwidth(Dictionary<byte, ProfilerData> data, float deltaTime)
	{
		float num = 0f;
		foreach (KeyValuePair<byte, ProfilerData> datum in data)
		{
			float num2 = (float)datum.Value.Bytes / deltaTime;
			num += num2;
		}
		return num;
	}

	private static string GetBudgetStatus(float totalBandwidth, out string color)
	{
		float num = totalBandwidth / BandwidthBudget;
		if (num > 0.8f)
		{
			color = "#FF4444";
			return "[CRITICAL]";
		}
		if (num > 0.5f)
		{
			color = "#FFAA44";
			return "[WARNING]";
		}
		color = "#44FF44";
		return "[GOOD]";
	}

	private static void AppendData(StringBuilder sb, Dictionary<byte, ProfilerData> data, float deltaTime, string prefix, Dictionary<byte, float> lastBytesPerSec, Dictionary<byte, float> peakBytesPerSec)
	{
		if (data.Count == 0)
		{
			sb.AppendLine("  (No activity)");
			return;
		}
		List<OpMetrics> list = new List<OpMetrics>();
		float num = 0f;
		foreach (KeyValuePair<byte, ProfilerData> datum in data)
		{
			ProfilerData value = datum.Value;
			float num2 = (float)value.Count / deltaTime;
			float num3 = (float)value.Bytes / deltaTime;
			if (!(num2 < MinRateThreshold))
			{
				num += num3;
				float last = (lastBytesPerSec.ContainsKey(datum.Key) ? lastBytesPerSec[datum.Key] : num3);
				string trendIndicator = GetTrendIndicator(num3, last);
				if (!peakBytesPerSec.ContainsKey(datum.Key) || num3 > peakBytesPerSec[datum.Key])
				{
					peakBytesPerSec[datum.Key] = num3;
				}
				float peakBytesPerSec2 = peakBytesPerSec[datum.Key];
				list.Add(new OpMetrics
				{
					OpCode = datum.Key,
					Rate = num2,
					BytesPerSec = num3,
					AvgSize = ((value.Count > 0) ? ((float)value.Bytes / (float)value.Count) : 0f),
					TotalCount = value.TotalCount,
					TotalBytes = value.TotalBytes,
					Trend = trendIndicator,
					PeakBytesPerSec = peakBytesPerSec2
				});
				lastBytesPerSec[datum.Key] = num3;
			}
		}
		list = list.OrderByDescending((OpMetrics m) => m.BytesPerSec).ToList();
		if (list.Count == 0)
		{
			sb.AppendLine("  (All below threshold)");
		}
		else
		{
			StringBuilder stringBuilder = sb;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(6, 7, stringBuilder);
			handler.AppendFormatted<string>("Op", -20);
			handler.AppendLiteral(" ");
			handler.AppendFormatted<string>("Rate", -8);
			handler.AppendLiteral(" ");
			handler.AppendFormatted<string>("BW", -10);
			handler.AppendLiteral(" ");
			handler.AppendFormatted<string>("Peak", -10);
			handler.AppendLiteral(" ");
			handler.AppendFormatted<string>("Avg", -8);
			handler.AppendLiteral(" ");
			handler.AppendFormatted<string>("%", -5);
			handler.AppendLiteral(" ");
			handler.AppendFormatted<string>("Trend", -5);
			stringBuilder2.AppendLine(ref handler);
			sb.AppendLine(new string('-', 72));
			foreach (OpMetrics item in list)
			{
				Op opCode = (Op)item.OpCode;
				float value2 = ((num > 0f) ? (item.BytesPerSec / num * 100f) : 0f);
				GetColorLabel(item.BytesPerSec, out var colorHex);
				ColorCodes[$"{prefix}_{item.OpCode}"] = colorHex;
				stringBuilder = sb;
				StringBuilder stringBuilder3 = stringBuilder;
				handler = new StringBuilder.AppendInterpolatedStringHandler(9, 7, stringBuilder);
				handler.AppendFormatted<string>(opCode.ToString(), -20);
				handler.AppendLiteral(" ");
				handler.AppendFormatted(item.Rate, 5, "F1");
				handler.AppendLiteral("/s ");
				handler.AppendFormatted<string>(FormatBytes(item.BytesPerSec), -10);
				handler.AppendLiteral(" ");
				handler.AppendFormatted<string>(FormatBytes(item.PeakBytesPerSec), -10);
				handler.AppendLiteral(" ");
				handler.AppendFormatted<string>(FormatBytes(item.AvgSize), -8);
				handler.AppendLiteral(" ");
				handler.AppendFormatted(value2, 4, "F1");
				handler.AppendLiteral("% ");
				handler.AppendFormatted<string>(item.Trend, -5);
				stringBuilder3.AppendLine(ref handler);
			}
			sb.AppendLine(new string('-', 72));
			stringBuilder = sb;
			StringBuilder stringBuilder4 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(2, 3, stringBuilder);
			handler.AppendFormatted<string>("TOTAL", -20);
			handler.AppendLiteral(" ");
			handler.AppendFormatted<string>("", -8);
			handler.AppendLiteral(" ");
			handler.AppendFormatted<string>(FormatBytes(num), -10);
			stringBuilder4.AppendLine(ref handler);
		}
		foreach (KeyValuePair<byte, ProfilerData> datum2 in data)
		{
			datum2.Value.Reset();
		}
	}

	private static string GetTrendIndicator(float current, float last)
	{
		if (last == 0f)
		{
			return "--";
		}
		float num = (current - last) / last;
		if (num > 0.1f)
		{
			return "▲";
		}
		if (num < -0.1f)
		{
			return "▼";
		}
		return "─";
	}

	private static string GetColorLabel(float bytesPerSec, out string colorHex)
	{
		if (bytesPerSec > 10240f)
		{
			colorHex = "#FF4444";
			return "[HIGH]";
		}
		if (bytesPerSec > 1024f)
		{
			colorHex = "#FFAA44";
			return "[MED]";
		}
		colorHex = "#44FF44";
		return "[LOW]";
	}

	private static string FormatBytes(float bytes)
	{
		if (!(bytes > 1048576f))
		{
			if (!(bytes > 1024f))
			{
				return $"{bytes:F0} B";
			}
			return $"{bytes / 1024f:F2} KB";
		}
		return $"{bytes / 1048576f:F2} MB";
	}
}
