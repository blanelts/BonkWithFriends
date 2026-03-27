using System;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppAssets.Scripts._Data;
using Il2CppTMPro;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.MonoBehaviours.Player;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Networking.Steam;
using Megabonk.BonkWithFriends.Player;
using Megabonk.BonkWithFriends.Steam;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Managers;

internal static class BotManager
{
	internal sealed class BotSlot
	{
		internal ulong FakeSteamId { get; set; }
		internal ECharacter Character { get; set; }
		internal string Name { get; set; }
	}

	private sealed class ActiveBot
	{
		internal BotSlot Slot { get; set; }
		internal CSteamID SteamId { get; set; }
		internal NetworkedPlayer Player { get; set; }
		internal RemotePlayerInterpolation Interp { get; set; }
		internal Vector3 Position { get; set; }
	}

	private const ulong BOT_STEAM_ID_BASE = 1000000000UL;
	private const int MAX_BOTS = 3;
	private const float BOT_FOLLOW_DISTANCE = 5f;
	private const float BOT_FOLLOW_SPEED = 4f;
	private const float BOT_STATE_UPDATE_INTERVAL = 0.5f;
	private const float BOT_MOVE_UPDATE_INTERVAL = 1f / 30f;
	private const string LOBBY_DATA_BOTS_KEY = "bots";

	private static readonly List<BotSlot> _lobbyBots = new();
	private static readonly List<ActiveBot> _activeBots = new();
	private static float _lastStateUpdate;
	private static float _lastMoveUpdate;

	internal static int BotCount => _lobbyBots.Count;
	internal static IReadOnlyList<BotSlot> LobbyBots => _lobbyBots;

	internal static bool IsBotSteamId(ulong steamId)
	{
		return steamId >= BOT_STEAM_ID_BASE && steamId < BOT_STEAM_ID_BASE + 100;
	}

	// ── Lobby phase ──

	internal static void AddBot()
	{
		if (!SteamNetworkManager.IsServer)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning("[BotManager] Only the host can add bots");
			return;
		}
		if (_lobbyBots.Count >= MAX_BOTS)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[BotManager] Maximum {MAX_BOTS} bots reached");
			return;
		}

		int index = _lobbyBots.Count;
		ulong fakeSteamId = BOT_STEAM_ID_BASE + (ulong)index;
		ECharacter character = (ECharacter)UnityEngine.Random.Range(0, 6);
		string name = $"Bot {index + 1}";

		BotSlot slot = new BotSlot
		{
			FakeSteamId = fakeSteamId,
			Character = character,
			Name = name
		};
		_lobbyBots.Add(slot);

		SteamPersonaNameCache.CachePersonaName(new CSteamID(fakeSteamId), name);
		WriteBotDataToLobby();

		Melon<BonkWithFriendsMod>.Logger.Msg($"[BotManager] Added bot '{name}' as {character}");
	}

	internal static void RemoveBot()
	{
		if (!SteamNetworkManager.IsServer)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning("[BotManager] Only the host can remove bots");
			return;
		}
		if (_lobbyBots.Count == 0) return;

		BotSlot removed = _lobbyBots[^1];
		_lobbyBots.RemoveAt(_lobbyBots.Count - 1);
		WriteBotDataToLobby();

		Melon<BonkWithFriendsMod>.Logger.Msg($"[BotManager] Removed bot '{removed.Name}'");
	}

	private static void WriteBotDataToLobby()
	{
		SteamNetworkLobby lobby = SteamNetworkLobby.Instance;
		if (lobby == null) return;

		// Format: "fakeSteamId,character,name;fakeSteamId,character,name;..."
		string data = "";
		for (int i = 0; i < _lobbyBots.Count; i++)
		{
			BotSlot bot = _lobbyBots[i];
			if (i > 0) data += ";";
			data += $"{bot.FakeSteamId},{(int)bot.Character},{bot.Name}";
		}

		SteamMatchmaking.SetLobbyData(lobby.LobbyId, LOBBY_DATA_BOTS_KEY, data);
	}

	internal static void ParseBotsFromLobbyData(string data)
	{
		_lobbyBots.Clear();

		if (string.IsNullOrWhiteSpace(data)) return;

		string[] entries = data.Split(';', StringSplitOptions.RemoveEmptyEntries);
		foreach (string entry in entries)
		{
			string[] parts = entry.Split(',');
			if (parts.Length < 3) continue;

			if (!ulong.TryParse(parts[0], out ulong fakeSteamId)) continue;
			if (!int.TryParse(parts[1], out int charIndex)) continue;

			BotSlot slot = new BotSlot
			{
				FakeSteamId = fakeSteamId,
				Character = (ECharacter)charIndex,
				Name = parts[2]
			};
			_lobbyBots.Add(slot);

			SteamPersonaNameCache.CachePersonaName(new CSteamID(fakeSteamId), slot.Name);
		}
	}

	// ── Game phase ──

	internal static void SpawnBots()
	{
		_activeBots.Clear();

		foreach (BotSlot slot in _lobbyBots)
		{
			try
			{
				CSteamID botSteamId = new CSteamID(slot.FakeSteamId);

				RemotePlayerManager.OnGameStarted(botSteamId, slot.Character, ESkinType.Default);
				NetworkedPlayer player = RemotePlayerManager.GetPlayer(botSteamId);

				if ((Object)(object)player == (Object)null)
				{
					Melon<BonkWithFriendsMod>.Logger.Error($"[BotManager] Failed to spawn bot '{slot.Name}'");
					continue;
				}

				RemotePlayerInterpolation interp = ((Component)player).GetComponentInChildren<RemotePlayerInterpolation>();

				// Fix nameplate
				try
				{
					TextMeshPro tmp = ((Component)player).GetComponentInChildren<TextMeshPro>();
					if (tmp != null)
						((TMP_Text)tmp).text = slot.Name;
				}
				catch (Exception)
				{
					// Non-critical
				}

				Vector3 startPos = Vector3.zero;
				if (LocalPlayerManager._myPlayer != null)
				{
					startPos = ((Component)LocalPlayerManager._myPlayer).transform.position
						+ Vector3.right * (BOT_FOLLOW_DISTANCE + _activeBots.Count * 2f);
				}

				if (interp != null)
					interp.Teleport(startPos, Quaternion.identity);

				ActiveBot activeBot = new ActiveBot
				{
					Slot = slot,
					SteamId = botSteamId,
					Player = player,
					Interp = interp,
					Position = startPos
				};
				_activeBots.Add(activeBot);

				// Set initial state — immortal
				UpdateBotState(activeBot);

				Melon<BonkWithFriendsMod>.Logger.Msg($"[BotManager] Spawned bot '{slot.Name}' as {slot.Character}");
			}
			catch (Exception ex)
			{
				Melon<BonkWithFriendsMod>.Logger.Error($"[BotManager] Error spawning bot '{slot.Name}': {ex}");
			}
		}
	}

	internal static void Update()
	{
		if (_activeBots.Count == 0) return;
		if (!SteamNetworkManager.IsServer) return;

		float time = Time.unscaledTime;

		if (time - _lastMoveUpdate >= BOT_MOVE_UPDATE_INTERVAL)
		{
			_lastMoveUpdate = time;
			foreach (ActiveBot bot in _activeBots)
			{
				if ((Object)(object)bot.Player != (Object)null)
					UpdateBotMovement(bot, time);
			}
		}

		if (time - _lastStateUpdate >= BOT_STATE_UPDATE_INTERVAL)
		{
			_lastStateUpdate = time;
			foreach (ActiveBot bot in _activeBots)
			{
				if ((Object)(object)bot.Player != (Object)null)
					UpdateBotState(bot);
			}
		}
	}

	private static void UpdateBotMovement(ActiveBot bot, float time)
	{
		if (bot.Interp == null) return;

		// Find closest alive player to follow
		Vector3 targetPos = FindClosestAlivePlayerPosition(bot.Position);
		Vector3 toTarget = targetPos - bot.Position;
		float distance = toTarget.magnitude;

		Vector3 velocity = Vector3.zero;

		if (distance > BOT_FOLLOW_DISTANCE)
		{
			Vector3 direction = toTarget.normalized;
			velocity = direction * BOT_FOLLOW_SPEED;
			bot.Position += velocity * BOT_MOVE_UPDATE_INTERVAL;
		}

		Quaternion rotation = Quaternion.identity;
		if (toTarget.sqrMagnitude > 0.01f)
		{
			rotation = Quaternion.LookRotation(new Vector3(toTarget.x, 0f, toTarget.z).normalized);
		}

		// Update locally (for host)
		bot.Interp.OnRemoteMovementUpdate(bot.Position, rotation, velocity, time);

		// Broadcast to remote clients
		SteamNetworkServer.Instance?.BroadcastToRemoteClients(new PlayerMovementRelayMessage
		{
			SteamUserId = bot.Slot.FakeSteamId,
			Position = bot.Position,
			Rotation = rotation,
			Velocity = velocity,
			ServerTime = time
		});
	}

	private static Vector3 FindClosestAlivePlayerPosition(Vector3 from)
	{
		float closestDist = float.MaxValue;
		Vector3 closestPos = from;

		// Check local player
		if (LocalPlayerManager._myPlayer != null && !LocalPlayerManager.LocalPlayerState.IsDead)
		{
			Vector3 pos = ((Component)LocalPlayerManager._myPlayer).transform.position;
			float dist = (pos - from).sqrMagnitude;
			if (dist < closestDist)
			{
				closestDist = dist;
				closestPos = pos;
			}
		}

		// Check remote players (other real players, skip bots)
		foreach (var kvp in RemotePlayerManager.GetAllPlayers())
		{
			NetworkedPlayer player = kvp.Value;
			if ((Object)(object)player == (Object)null || player.State.IsDead) continue;
			if (IsBotSteamId(kvp.Key.m_SteamID)) continue;

			Vector3 pos = ((Component)player).transform.position;
			float dist = (pos - from).sqrMagnitude;
			if (dist < closestDist)
			{
				closestDist = dist;
				closestPos = pos;
			}
		}

		return closestPos;
	}

	private static void UpdateBotState(ActiveBot bot)
	{
		PlayerState state = new PlayerState
		{
			CurrentHp = 999,
			MaxHp = 999,
			Shield = 100f,
			MaxShield = 100f,
			Level = 10,
			Xp = 5000,
			IsDead = false
		};
		RemotePlayerManager.UpdatePlayerState(bot.SteamId, state);

		// Broadcast to remote clients
		SteamNetworkServer.Instance?.BroadcastToRemoteClients(new PlayerStateRelayMessage
		{
			SteamUserId = bot.Slot.FakeSteamId,
			Hp = state.CurrentHp,
			MaxHp = state.MaxHp,
			Shield = state.Shield,
			MaxShield = state.MaxShield,
			Level = state.Level,
			Xp = state.Xp,
			IsDead = state.IsDead
		});
	}

	// ── Cleanup ──

	internal static void ClearActiveBots()
	{
		_activeBots.Clear();
		_lastStateUpdate = 0f;
		_lastMoveUpdate = 0f;
	}

	internal static void ClearAll()
	{
		_activeBots.Clear();
		_lobbyBots.Clear();
		_lastStateUpdate = 0f;
		_lastMoveUpdate = 0f;
	}
}
