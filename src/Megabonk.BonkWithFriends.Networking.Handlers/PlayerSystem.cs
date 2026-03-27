using System;
using Megabonk.BonkWithFriends.HarmonyPatches.Player;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.MonoBehaviours.Player;
using Megabonk.BonkWithFriends.Networking.Messages;
using Megabonk.BonkWithFriends.Networking.Messages.Client;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Networking.Steam;
using Megabonk.BonkWithFriends.Player;
using Megabonk.BonkWithFriends.Managers;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Networking.Handlers;

public static class PlayerSystem
{
	[NetworkMessageHandler(MessageType.PlayerJoined)]
	private static void HandlePlayerJoined(SteamNetworkMessage message)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		PlayerJoinedMessage playerJoinedMessage = message.Deserialize<PlayerJoinedMessage>();
		Melon<BonkWithFriendsMod>.Logger.Msg($"[Client] Player {message.SteamUserId} joined as {playerJoinedMessage.Character}.");
	}

	[NetworkMessageHandler(MessageType.PlayerLeft)]
	private static void HandlePlayerLeft(SteamNetworkMessage message)
	{
		PlayerLeftMessage playerLeftMessage = message.Deserialize<PlayerLeftMessage>();
		Melon<BonkWithFriendsMod>.Logger.Msg($"[Client] Player {playerLeftMessage.SteamUserId} left.");
	}

	[NetworkMessageHandler(MessageType.PlayerMovementRelay)]
	private static void RouteMovement(SteamNetworkMessage message)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		PlayerMovementRelayMessage playerMovementRelayMessage = message.Deserialize<PlayerMovementRelayMessage>();
		if (SteamNetworkManager.IsServer)
		{
			SteamNetworkServer.Instance?.BroadcastMessageExcept(playerMovementRelayMessage, message.SteamUserId);
		}
		NetworkedPlayer player = RemotePlayerManager.GetPlayer(new CSteamID(playerMovementRelayMessage.SteamUserId));
		if (((Object)((Object)(object)player)))
		{
			((Component)player).GetComponentInChildren<RemotePlayerInterpolation>()?.OnRemoteMovementUpdate(playerMovementRelayMessage.Position, playerMovementRelayMessage.Rotation, playerMovementRelayMessage.Velocity, playerMovementRelayMessage.ServerTime);
		}
	}

	[NetworkMessageHandler(MessageType.PlayerMovement)]
	private static void HandleMovement(SteamNetworkMessage message)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		PlayerMovementMessage playerMovementMessage = message.Deserialize<PlayerMovementMessage>();
		NetworkedPlayer player = RemotePlayerManager.GetPlayer(message.SteamUserId);
		if (((Object)((Object)(object)player)))
		{
			((Component)player).GetComponentInChildren<RemotePlayerInterpolation>()?.OnRemoteMovementUpdate(playerMovementMessage.Position, playerMovementMessage.Rotation, playerMovementMessage.Velocity, playerMovementMessage.ServerTime);
		}
	}

	[NetworkMessageHandler(MessageType.AnimationStateRelay)]
	private static void RelayAnimation(SteamNetworkMessage message)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		AnimationStateRelayMessage animationStateRelayMessage = message.Deserialize<AnimationStateRelayMessage>();
		if (SteamNetworkManager.IsServer)
		{
			SteamNetworkServer.Instance?.BroadcastMessageExcept(animationStateRelayMessage, message.SteamUserId);
		}
		NetworkedPlayer player = RemotePlayerManager.GetPlayer(new CSteamID(animationStateRelayMessage.SteamUserId));
		if (((Object)((Object)(object)player)))
		{
			((Component)player).GetComponentInChildren<RemoteAnimationController>()?.OnAnimationStateUpdate(animationStateRelayMessage.StateFlags);
		}
	}

	[NetworkMessageHandler(MessageType.AnimationState)]
	private static void RouteAnimation(SteamNetworkMessage message)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		AnimationStateMessage animationStateMessage = message.Deserialize<AnimationStateMessage>();
		NetworkedPlayer player = RemotePlayerManager.GetPlayer(message.SteamUserId);
		if (((Object)((Object)(object)player)))
		{
			((Component)player).GetComponentInChildren<RemoteAnimationController>()?.OnAnimationStateUpdate(animationStateMessage.StateFlags);
		}
	}

	[NetworkMessageHandler(MessageType.PlayerStateRelay)]
	private static void RelayPlayerState(SteamNetworkMessage message)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		PlayerStateRelayMessage playerStateRelayMessage = message.Deserialize<PlayerStateRelayMessage>();
		if (SteamNetworkManager.IsServer)
		{
			SteamNetworkServer.Instance?.BroadcastMessageExcept(playerStateRelayMessage, message.SteamUserId);
		}
		PlayerState state = new PlayerState
		{
			CurrentHp = playerStateRelayMessage.Hp,
			MaxHp = playerStateRelayMessage.MaxHp,
			Shield = playerStateRelayMessage.Shield,
			MaxShield = playerStateRelayMessage.MaxShield,
			Level = playerStateRelayMessage.Level,
			Xp = playerStateRelayMessage.Xp,
			IsDead = playerStateRelayMessage.IsDead
		};
		RemotePlayerManager.UpdatePlayerState(new CSteamID(playerStateRelayMessage.SteamUserId), state);
		if (state.IsDead)
		{
			StopPlayerAttacks(new CSteamID(playerStateRelayMessage.SteamUserId));
		}
		Melon<BonkWithFriendsMod>.Logger.Msg("Updated Player State");
	}

	[NetworkMessageHandler(MessageType.PlayerState)]
	private static void HandlePlayerState(SteamNetworkMessage message)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		PlayerStateMessage playerStateMessage = message.Deserialize<PlayerStateMessage>();
		PlayerState state = new PlayerState
		{
			CurrentHp = playerStateMessage.Hp,
			MaxHp = playerStateMessage.MaxHp,
			Shield = playerStateMessage.Shield,
			MaxShield = playerStateMessage.MaxShield,
			Level = playerStateMessage.Level,
			Xp = playerStateMessage.Xp,
			IsDead = playerStateMessage.IsDead
		};
		RemotePlayerManager.UpdatePlayerState(message.SteamUserId, state);
		if (state.IsDead)
		{
			StopPlayerAttacks(message.SteamUserId);
		}
		Melon<BonkWithFriendsMod>.Logger.Msg("Updated Player State");
	}

	private static void StopPlayerAttacks(CSteamID steamId)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		NetworkedPlayer player = RemotePlayerManager.GetPlayer(steamId);
		if (((Object)((Object)(object)player)))
		{
			RemoteAttackController attackController = ((Component)player).GetComponentInChildren<RemoteAttackController>();
			if (((Object)((Object)(object)attackController)))
			{
				attackController.ClearAllAttacks();
				Melon<BonkWithFriendsMod>.Logger.Msg($"[PlayerSystem] Cleared attacks for dead player {steamId.m_SteamID}");
			}
		}
	}

	[NetworkMessageHandler(MessageType.PlayerRevive)]
	private static void HandlePlayerRevive(SteamNetworkMessage message)
	{
		PlayerReviveMessage msg = message.Deserialize<PlayerReviveMessage>();

		// Server: relay to all other clients
		if (SteamNetworkManager.IsServer)
		{
			SteamNetworkServer.Instance?.BroadcastMessageExcept(msg, message.SteamUserId);
		}

		ResurrectionManager.HandleRevive(msg.TargetSteamId, msg.ReviverSteamId);
		Melon<BonkWithFriendsMod>.Logger.Msg($"[PlayerSystem] Revive: {msg.ReviverSteamId} revived {msg.TargetSteamId}");
	}

	[NetworkMessageHandler(MessageType.XpGained)]
	private static void HandleXpGained(SteamNetworkMessage message)
	{
		XpGainedMessage xpGainedMessage = message.Deserialize<XpGainedMessage>();
		Melon<BonkWithFriendsMod>.Logger.Msg($"[XpSync] HandleXpGained called - Mode: {SteamNetworkManager.Mode}, From: {message.SteamUserId}, Amount: {xpGainedMessage.XpAmount}");

		if (SteamNetworkManager.IsServer)
		{
			// Server relay: forward client XP to other clients (exclude sender)
			SteamNetworkServer.Instance?.BroadcastMessageExcept(xpGainedMessage, message.SteamUserId);
			Melon<BonkWithFriendsMod>.Logger.Msg($"[XpSync] Server relaying {xpGainedMessage.XpAmount} XP (excluding sender {message.SteamUserId})");
			// Host never applies XP from network — host gets XP natively and broadcasts directly
			return;
		}

		// Client: apply received XP locally
		if ((Object)(object)LocalPlayerManager._myPlayer == (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[XpSync] Cannot add XP - _myPlayer is null!");
			return;
		}
		if (LocalPlayerManager._myPlayer.inventory == null)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[XpSync] Cannot add XP - inventory is null!");
			return;
		}
		if (LocalPlayerManager._myPlayer.inventory.playerXp == null)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[XpSync] Cannot add XP - playerXp is null!");
			return;
		}
		PlayerPatches.SetAddingXpFromNetwork(value: true);
		try
		{
			int xp = LocalPlayerManager._myPlayer.inventory.playerXp.xp;
			LocalPlayerManager._myPlayer.inventory.playerXp.AddXp(xpGainedMessage.XpAmount);
			int xp2 = LocalPlayerManager._myPlayer.inventory.playerXp.xp;
			Melon<BonkWithFriendsMod>.Logger.Msg($"[XpSync] Client applied {xpGainedMessage.XpAmount} XP (Before: {xp}, After: {xp2})");
		}
		catch (Exception value)
		{
			Melon<BonkWithFriendsMod>.Logger.Error($"[XpSync] Exception adding XP: {value}");
		}
		finally
		{
			PlayerPatches.SetAddingXpFromNetwork(value: false);
		}
	}
}
