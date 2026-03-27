using Il2Cpp;
using Megabonk.BonkWithFriends.Managers.Items;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.Networking.Messages;
using Megabonk.BonkWithFriends.Networking.Messages.Client;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Systems;

public static class PickupSystem
{
	[NetworkMessageHandler(MessageType.PickupSpawned)]
	private static void HandlePickupSpawned(SteamNetworkMessage message)
	{
		PickupSpawnManager.HandlePickupSpawn(message.Deserialize<PickupSpawnedMessage>());
	}

	[NetworkMessageHandler(MessageType.PickupCollected)]
	private static void HandlePickupCollected(SteamNetworkMessage message)
	{
		PickupCollectedMessage msg = message.Deserialize<PickupCollectedMessage>();

		if (SteamNetworkManager.IsServer)
		{
			// Get pickup value BEFORE despawning so we can relay it
			Pickup pickup = PickupSpawnManager.GetPickup(msg.PickupId);
			if ((Object)(object)pickup != (Object)null)
			{
				int pickupValue = pickup.GetValue();
				if (pickupValue > 0)
				{
					// Apply XP to host (shared XP model) — this triggers the
					// AddXp postfix which broadcasts XpGainedMessage to all clients
					var playerXp = LocalPlayerManager._myPlayer?.inventory?.playerXp;
					if (playerXp != null)
					{
						playerXp.AddXp(pickupValue);
						Melon<BonkWithFriendsMod>.Logger.Msg($"[PickupSystem] Client collected pickup {msg.PickupId}, applied {pickupValue} XP via host");
					}
				}
			}

			// Relay despawn to other clients (for 3+ player support)
			SteamNetworkServer.Instance?.BroadcastMessageExcept(msg, message.SteamUserId);
		}

		PickupSpawnManager.ProcessPickupCollection(msg.PickupId);
	}

	[NetworkMessageHandler(MessageType.PickupDespawned)]
	private static void HandlePickupDespawned(SteamNetworkMessage message)
	{
		PickupSpawnManager.QueueRemotePickupDespawn(message.Deserialize<PickupDespawnedMessage>().PickupId);
	}
}
