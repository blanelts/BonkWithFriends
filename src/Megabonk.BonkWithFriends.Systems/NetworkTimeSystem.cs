using Megabonk.BonkWithFriends.Managers.Server;
using Megabonk.BonkWithFriends.Networking.Messages;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Networking.Steam;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Systems;

public static class NetworkTimeSystem
{
	[NetworkMessageHandler(MessageType.TimeSyncRequest)]
	private static void HandleTimeSyncRequest(SteamNetworkMessage message)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (SteamNetworkManager.IsServer)
		{
			TimeSyncRequestMessage timeSyncRequestMessage = message.Deserialize<TimeSyncRequestMessage>();
			TimeSyncResponseMessage tMsg = new TimeSyncResponseMessage
			{
				ClientSendTime = timeSyncRequestMessage.ClientSendTime,
				ServerReceiveTime = Time.unscaledTime
			};
			SteamNetworkServer.Instance.SendMessage(tMsg, message.SteamUserId);
		}
	}

	[NetworkMessageHandler(MessageType.TimeSyncResponse)]
	private static void HandleTimeSyncResponse(SteamNetworkMessage message)
	{
		if (SteamNetworkManager.IsClient && !SteamNetworkManager.IsServer)
		{
			TimeSyncResponseMessage timeSyncResponseMessage = message.Deserialize<TimeSyncResponseMessage>();
			NetworkTimeSync.ProcessTimeSyncResponse(timeSyncResponseMessage.ServerReceiveTime, timeSyncResponseMessage.ClientSendTime);
		}
	}
}
