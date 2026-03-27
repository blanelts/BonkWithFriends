using System.Collections.Generic;
using Megabonk.BonkWithFriends.Debug;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.MonoBehaviours.Player;
using Megabonk.BonkWithFriends.Networking.Messages;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Managers;

public static class ResurrectionManager
{
	private const float REVIVE_RADIUS = 5f;
	private const float REVIVE_TIME = 7f;
	private const float REVIVE_HP_PERCENT = 0.3f;

	private static readonly Dictionary<ulong, float> _reviveTimers = new();
	private static ulong _currentReviveTarget;
	private static float _currentReviveProgress;
	private static bool _isReviving;

	public static bool IsReviving => _isReviving;
	public static float ReviveProgress => _currentReviveProgress;
	public static string ReviveTargetName { get; private set; } = "";

	public static void Update()
	{
		if (!SteamNetworkManager.IsMultiplayer)
			return;

		// Only alive players can revive
		if (LocalPlayerManager.LocalPlayerState.IsDead)
		{
			ResetReviveState();
			return;
		}

		if (!((Object)(object)LocalPlayerManager.LocalPlayer))
			return;

		Vector3 myPos = ((Component)LocalPlayerManager.LocalPlayer).transform.position;
		ulong closestDeadId = 0;
		float closestDist = float.MaxValue;
		string closestName = "";

		// Find nearest dead remote player within range
		foreach (var kvp in RemotePlayerManager.GetAllPlayers())
		{
			NetworkedPlayer player = kvp.Value;
			if ((Object)(object)player == (Object)null || !player.State.IsDead)
				continue;

			float dist = Vector3.Distance(myPos, ((Component)player).transform.position);
			if (dist < REVIVE_RADIUS && dist < closestDist)
			{
				closestDist = dist;
				closestDeadId = kvp.Key.m_SteamID;
				closestName = Steam.SteamPersonaNameCache.GetCachedName(kvp.Key) ?? kvp.Key.m_SteamID.ToString();
			}
		}

		if (closestDeadId == 0)
		{
			ResetReviveState();
			return;
		}

		// Accumulate revive timer for closest dead player
		if (!_reviveTimers.ContainsKey(closestDeadId))
			_reviveTimers[closestDeadId] = 0f;

		_reviveTimers[closestDeadId] += Time.deltaTime;
		_currentReviveTarget = closestDeadId;
		_currentReviveProgress = _reviveTimers[closestDeadId] / REVIVE_TIME;
		_isReviving = true;
		ReviveTargetName = closestName;

		if (_reviveTimers[closestDeadId] >= REVIVE_TIME)
		{
			// Revive complete — send message
			ulong mySteamId = SteamUser.GetSteamID().m_SteamID;
			PlayerReviveMessage msg = new PlayerReviveMessage
			{
				TargetSteamId = closestDeadId,
				ReviverSteamId = mySteamId
			};

			if (SteamNetworkManager.IsServer)
			{
				SteamNetworkServer.Instance?.BroadcastToRemoteClients(msg);
				// Host must also process revive locally (update RemotePlayerManager)
				HandleRevive(msg.TargetSteamId, msg.ReviverSteamId);
			}
			else
			{
				SteamNetworkClient.Instance?.SendMessage(msg);
			}

			Melon<BonkWithFriendsMod>.Logger.Msg($"[Resurrection] Revived player {closestDeadId}");
			_reviveTimers.Remove(closestDeadId);
			ResetReviveState();
		}
	}

	public static void HandleRevive(ulong targetSteamId, ulong reviverSteamId)
	{
		ulong myId = SteamUser.GetSteamID().m_SteamID;

		// Am I the one being revived?
		if (targetSteamId == myId)
		{
			ReviveLocalPlayer();
			return;
		}

		// Update remote player state (they'll broadcast it themselves, but clear our dead flag early)
		RemotePlayerManager.UpdatePlayerState(
			new CSteamID(targetSteamId),
			new PlayerState
			{
				IsDead = false,
				CurrentHp = 1
			});
	}

	private static void ReviveLocalPlayer()
	{
		Melon<BonkWithFriendsMod>.Logger.Msg("[Resurrection] I am being revived!");

		// Access _myPlayer directly — GetPlayerInventory may fail if player is in death state
		var myPlayer = LocalPlayerManager._myPlayer;
		if ((Object)(object)myPlayer == (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[Resurrection] _myPlayer is null, cannot revive!");
			LocalPlayerManager.UpdatePlayerDeath(false);
			if (SpectatorManager.IsSpectating)
				SpectatorManager.ExitSpectatorMode();
			return;
		}

		var inventory = myPlayer.inventory;
		if (inventory == null)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[Resurrection] inventory is null, cannot set HP!");
			LocalPlayerManager.UpdatePlayerDeath(false);
			if (SpectatorManager.IsSpectating)
				SpectatorManager.ExitSpectatorMode();
			return;
		}

		var health = inventory.playerHealth;
		if ((Object)(object)health == (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[Resurrection] playerHealth is null, cannot set HP!");
			LocalPlayerManager.UpdatePlayerDeath(false);
			if (SpectatorManager.IsSpectating)
				SpectatorManager.ExitSpectatorMode();
			return;
		}

		// Restore HP to 30% of max
		int reviveHp = Mathf.Max(1, (int)(health.maxHp * REVIVE_HP_PERCENT));
		health.hp = reviveHp;
		Melon<BonkWithFriendsMod>.Logger.Msg($"[Resurrection] Set HP to {reviveHp} (max={health.maxHp})");

		// Reset native game-over flag so native systems recognize player as alive
		try
		{
			var gm = Il2Cpp.GameManager.Instance;
			if ((Object)(object)gm != (Object)null)
			{
				gm._isGameOver_k__BackingField = false;
			}
		}
		catch (System.Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[Resurrection] Could not reset isGameOver: {ex.Message}");
		}

		// Also call Heal through the native path for proper state updates
		try
		{
			health.Heal(reviveHp);
			Melon<BonkWithFriendsMod>.Logger.Msg($"[Resurrection] Called health.Heal({reviveHp})");
		}
		catch (System.Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[Resurrection] health.Heal failed: {ex.Message}");
		}

		// Re-enable player components that may have been disabled during death
		try
		{
			var playerGo = ((Component)myPlayer).gameObject;

			// Re-enable the player GameObject if it was deactivated
			if (!playerGo.activeSelf)
			{
				playerGo.SetActive(true);
				Melon<BonkWithFriendsMod>.Logger.Msg("[Resurrection] Re-activated player GameObject");
			}

			// Re-enable the MyPlayer MonoBehaviour itself
			if (!((Behaviour)myPlayer).enabled)
			{
				((Behaviour)myPlayer).enabled = true;
				Melon<BonkWithFriendsMod>.Logger.Msg("[Resurrection] Re-enabled MyPlayer Behaviour");
			}

			// Re-enable ALL MonoBehaviours on the player that may have been disabled
			var behaviours = ((Component)myPlayer).GetComponentsInChildren<MonoBehaviour>(true);
			foreach (var b in behaviours)
			{
				if ((Object)(object)b != (Object)null && !((Behaviour)b).enabled)
				{
					try
					{
						((Behaviour)b).enabled = true;
					}
					catch { }
				}
			}

			// Re-enable Rigidbody (may be set kinematic during death) and reset velocity
			var rb = ((Component)myPlayer).GetComponent<Rigidbody>();
			if ((Object)(object)rb != (Object)null)
			{
				if (rb.isKinematic)
				{
					rb.isKinematic = false;
					Melon<BonkWithFriendsMod>.Logger.Msg("[Resurrection] Re-enabled Rigidbody");
				}
				rb.velocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
			}

			// Re-enable all colliders on player
			var colliders = ((Component)myPlayer).GetComponentsInChildren<Collider>(true);
			foreach (var col in colliders)
			{
				if ((Object)(object)col != (Object)null && !col.enabled)
				{
					col.enabled = true;
				}
			}

			// Reset animator to idle state
			var animator = ((Component)myPlayer).GetComponentInChildren<Animator>();
			if ((Object)(object)animator != (Object)null)
			{
				try { animator.SetBool("idle", true); } catch { }
				try { animator.SetBool("moving", false); } catch { }
				try { animator.SetBool("grounded", true); } catch { }
				try { animator.SetBool("jumping", false); } catch { }
				try { animator.SetBool("grinding", false); } catch { }
				try { animator.SetBool("dead", false); } catch { }
				try { animator.SetBool("isDead", false); } catch { }
				try { animator.Play("Idle", 0, 0f); } catch { }
				Melon<BonkWithFriendsMod>.Logger.Msg("[Resurrection] Reset animator to idle state");
			}

			// Call native CheckDead — with HP > 0 it should not re-kill
			try { health.CheckDead(); }
			catch (System.Exception ex)
			{
				Melon<BonkWithFriendsMod>.Logger.Warning($"[Resurrection] CheckDead failed: {ex.Message}");
			}
		}
		catch (System.Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[Resurrection] Error re-enabling components: {ex.Message}");
		}

		// Force immediate broadcast of alive state
		LocalPlayerManager.BroadcastPlayerStateChange(forceImmediate: true);

		// Update our network state and broadcast
		LocalPlayerManager.UpdatePlayerDeath(false);

		// Exit spectator mode
		if (SpectatorManager.IsSpectating)
			SpectatorManager.ExitSpectatorMode();

		Melon<BonkWithFriendsMod>.Logger.Msg($"[Resurrection] Revived with {reviveHp} HP");
	}

	private static void ResetReviveState()
	{
		if (_isReviving)
		{
			_isReviving = false;
			_currentReviveProgress = 0f;
			_currentReviveTarget = 0;
			ReviveTargetName = "";
		}
		// Reset timers for players no longer in range
		_reviveTimers.Clear();
	}

	public static void DrawGui()
	{
		if (!_isReviving) return;

		float barWidth = 200f;
		float barHeight = 20f;
		float x = Screen.width / 2f - barWidth / 2f;
		float y = Screen.height / 2f + 60f;

		// Background
		GUI.Box(new Rect(x, y, barWidth, barHeight), "");

		// Progress fill
		GUI.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
		GUI.DrawTexture(new Rect(x + 2, y + 2, (barWidth - 4) * Mathf.Clamp01(_currentReviveProgress), barHeight - 4), Texture2D.whiteTexture);
		GUI.color = Color.white;

		// Label
		string text = $"Reviving {ReviveTargetName}... {(_currentReviveProgress * 100f):F0}%";
		GUI.Label(new Rect(x, y - 25f, barWidth, 25f), $"<color=yellow>{text}</color>");
	}

	public static void ClearState()
	{
		_reviveTimers.Clear();
		ResetReviveState();
	}
}
