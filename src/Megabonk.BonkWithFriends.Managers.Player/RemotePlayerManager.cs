using System;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppAssets.Scripts.Camera;
using Il2CppAssets.Scripts._Data;
using Megabonk.BonkWithFriends.MonoBehaviours.Player;
using MelonLoader;
using Steamworks;
using UnityEngine;
namespace Megabonk.BonkWithFriends.Managers.Player;

public static class RemotePlayerManager
{
	public struct PlayerTarget
	{
		public Transform Transform;

		public Rigidbody Rigidbody;
	}

	private static readonly Dictionary<CSteamID, NetworkedPlayer> RemotePlayers = new Dictionary<CSteamID, NetworkedPlayer>();

	private static GameObject playerObject;

	private static Rigidbody _localPlayerRbCache = null;

	private static MinimapCamera _minimapCamera;

	private static MinimapCamera GetMinimapCamera()
	{
		if (!((Object)((Object)(object)_minimapCamera)))
		{
			_minimapCamera = Object.FindObjectOfType<MinimapCamera>();
		}
		return _minimapCamera;
	}

	public static void OnGameStarted(CSteamID steamId, ECharacter character, ESkinType skinType)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (!RemotePlayers.ContainsKey(steamId))
		{
			playerObject = new GameObject($"RemotePlayer_{steamId}");
			playerObject.transform.position = Vector3.zero;
			playerObject.transform.rotation = Quaternion.identity;
			playerObject.SetActive(true);
			NetworkedPlayer networkedPlayer = playerObject.AddComponent<NetworkedPlayer>();
			networkedPlayer.Initialize(steamId.m_SteamID, isLocal: false, isHost: false);
			networkedPlayer.SetupVisuals(character, skinType);
			RemotePlayers[steamId] = networkedPlayer;
		}
	}

	public static void AddMiniMapIcon()
	{
		MinimapCamera minimapCameraComp = GetMinimapCamera();
		if (!((Object)((Object)(object)minimapCameraComp)))
		{
			Melon<BonkWithFriendsMod>.Logger.Warning("[RemotePlayerManager] MinimapCamera not found");
			return;
		}

		Camera minimapCam = ((Component)minimapCameraComp).GetComponent<Camera>();
		Camera mainCam = Camera.main;

		if ((Object)(object)minimapCam == (Object)null || (Object)(object)mainCam == (Object)null)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning("[RemotePlayerManager] Could not get minimap or main camera");
			return;
		}

		int minimapMask = minimapCam.cullingMask;
		int mainMask = mainCam.cullingMask;
		int minimapOnlyLayers = minimapMask & ~mainMask;

		Melon<BonkWithFriendsMod>.Logger.Msg($"[RemotePlayerManager] MinimapMask: {minimapMask:X8}, MainMask: {mainMask:X8}, MinimapOnly: {minimapOnlyLayers:X8}");

		int targetLayer = -1;

		// Strategy 1: layer visible ONLY to minimap camera (not main camera)
		if (minimapOnlyLayers != 0)
		{
			for (int i = 0; i < 32; i++)
			{
				if ((minimapOnlyLayers & (1 << i)) != 0)
				{
					targetLayer = i;
					Melon<BonkWithFriendsMod>.Logger.Msg($"[RemotePlayerManager] Using minimap-only layer {i} ({LayerMask.LayerToName(i)})");
					break;
				}
			}
		}

		// Strategy 2: find unused layer, add it to minimap camera only
		if (targetLayer == -1)
		{
			for (int i = 31; i >= 8; i--)
			{
				if ((minimapMask & (1 << i)) == 0 && (mainMask & (1 << i)) == 0)
				{
					targetLayer = i;
					minimapCam.cullingMask |= (1 << i);
					Melon<BonkWithFriendsMod>.Logger.Msg($"[RemotePlayerManager] Assigned unused layer {i} to minimap camera");
					break;
				}
			}
		}

		if (targetLayer == -1)
		{
			Melon<BonkWithFriendsMod>.Logger.Error("[RemotePlayerManager] No suitable layer for minimap icons");
			return;
		}

		foreach (NetworkedPlayer value2 in RemotePlayers.Values)
		{
			if (((Object)((Object)(object)value2)) && ((Object)((Object)(object)((Component)value2).gameObject)))
			{
				try
				{
					GameObject iconObj = GameObject.CreatePrimitive((PrimitiveType)0); // Sphere
					((Object)iconObj).name = "MinimapIcon_Friend";
					iconObj.transform.SetParent(((Component)value2).transform, false);
					iconObj.transform.localPosition = Vector3.zero;
					iconObj.transform.localScale = new Vector3(15f, 15f, 15f);
					iconObj.layer = targetLayer;

					// Remove collider
					Collider col = iconObj.GetComponent<Collider>();
					if ((Object)(object)col != (Object)null)
						Object.Destroy((Object)(object)col);

					// Bright blue with emission
					Renderer renderer = iconObj.GetComponent<Renderer>();
					if ((Object)(object)renderer != (Object)null)
					{
						Material mat = renderer.material;
						mat.color = new Color(0.2f, 0.8f, 1f, 1f);
						mat.SetFloat("_Glossiness", 0f);
						mat.SetFloat("_Metallic", 0f);
						mat.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 1f, 1f));
						mat.EnableKeyword("_EMISSION");
					}

					Melon<BonkWithFriendsMod>.Logger.Msg($"[RemotePlayerManager] Minimap icon on layer {targetLayer}");
				}
				catch (Exception value)
				{
					Melon<BonkWithFriendsMod>.Logger.Error($"[RemotePlayerManager] Failed to add minimap icon: {value}");
				}
			}
		}
	}

	public static void OnPlayerLeft(CSteamID steamId)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (!RemotePlayers.TryGetValue(steamId, out var value))
		{
			return;
		}
		if (((Object)((Object)(object)value)) && ((Object)((Object)(object)((Component)value).gameObject)))
		{
			MinimapCamera minimapCamera = GetMinimapCamera();
			if (((Object)((Object)(object)minimapCamera)))
			{
				try
				{
					minimapCamera.RemoveArrow(((Component)value).transform);
				}
				catch (Exception value2)
				{
					Melon<BonkWithFriendsMod>.Logger.Error($"[RemotePlayerManager] Failed to remove minimap icon: {value2}");
				}
			}
			Object.Destroy((Object)(object)((Component)value).gameObject);
		}
		RemotePlayers.Remove(steamId);
	}

	public static NetworkedPlayer GetPlayer(CSteamID steamId)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		RemotePlayers.TryGetValue(steamId, out var value);
		return value;
	}

	public static IEnumerable<KeyValuePair<CSteamID, NetworkedPlayer>> GetAllPlayers()
	{
		return RemotePlayers;
	}

	public static void UpdatePlayerState(CSteamID steamId, PlayerState state)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if (RemotePlayers.TryGetValue(steamId, out var value) && ((Object)((Object)(object)value)))
		{
			value.State = state;
		}
	}

	public static bool AreAllRemotePlayersDead()
	{
		if (RemotePlayers.Count == 0)
		{
			return true;
		}
		foreach (NetworkedPlayer value in RemotePlayers.Values)
		{
			if (((Object)((Object)(object)value)) && !value.State.IsDead)
			{
				return false;
			}
		}
		Melon<BonkWithFriendsMod>.Logger.Msg("[RemotePlayerManager] AreAllRemotePlayersDead: Yes");
		return true;
	}

	public static void FillAllPlayerTargets(List<PlayerTarget> dst)
	{
		dst.Clear();
		if (((Object)((Object)(object)LocalPlayerManager.LocalPlayer)) && !LocalPlayerManager.LocalPlayerState.IsDead)
		{
			if (!((Object)((Object)(object)_localPlayerRbCache)))
			{
				_localPlayerRbCache = ((Component)LocalPlayerManager.LocalPlayer).GetComponent<Rigidbody>();
			}
			dst.Add(new PlayerTarget
			{
				Transform = ((Component)LocalPlayerManager.LocalPlayer).transform,
				Rigidbody = _localPlayerRbCache
			});
		}
		foreach (NetworkedPlayer value in RemotePlayers.Values)
		{
			if (((Object)((Object)(object)value)) && ((Object)((Object)(object)((Component)value).gameObject)) && !value.State.IsDead)
			{
				dst.Add(new PlayerTarget
				{
					Transform = ((Component)value).transform,
					Rigidbody = value.CachedRigidbody
				});
			}
		}
	}

	public static void ClearState()
	{
		MinimapCamera minimapCamera = GetMinimapCamera();
		foreach (NetworkedPlayer value2 in RemotePlayers.Values)
		{
			if (!((Object)((Object)(object)value2)) || !((Object)((Object)(object)((Component)value2).gameObject)))
			{
				continue;
			}
			if (((Object)((Object)(object)minimapCamera)))
			{
				try
				{
					minimapCamera.RemoveArrow(((Component)value2).transform);
				}
				catch (Exception value)
				{
					Melon<BonkWithFriendsMod>.Logger.Error($"[RemotePlayerManager] Failed to remove minimap icon: {value}");
				}
			}
			Object.Destroy((Object)(object)((Component)value2).gameObject);
		}
		RemotePlayers.Clear();
		Melon<BonkWithFriendsMod>.Logger.Msg("[RemotePlayerManager] State cleared.");
	}
}
