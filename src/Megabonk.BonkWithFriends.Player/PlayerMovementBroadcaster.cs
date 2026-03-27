using Il2Cpp;
using Il2CppAssets.Scripts.Actors.Player;
using Megabonk.BonkWithFriends.Managers.Server;
using Megabonk.BonkWithFriends.Networking.Messages.Server;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Networking.Steam;
using Megabonk.BonkWithFriends.Steam;
using MelonLoader;
using SteamManager = Megabonk.BonkWithFriends.Steam.SteamManager;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Player;

[RegisterTypeInIl2Cpp]
public class PlayerMovementBroadcaster : MonoBehaviour
{
	private Vector3 _lastSentPosition = Vector3.zero;

	private Quaternion _lastSentRotation = Quaternion.identity;

	private const float MinPositionDeltaSqr = 0.0001f;

	private const float MinRotationDelta = 1f;

	private PlayerMovement _playerMovement;

	private PlayerRenderer _playerRenderer;

	private bool _componentsCached;

	public ulong SteamUserId { get; private set; }

	public bool IsLocalPlayer { get; private set; }

	public void Initialize(ulong steamId)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		SteamUserId = steamId;
		IsLocalPlayer = SteamUserId == SteamManager.Instance.CurrentUserId.m_SteamID;
		CacheComponents();
	}

	private void CacheComponents()
	{
		if ((Object)(object)_playerMovement == (Object)null)
		{
			_playerMovement = ((Component)this).GetComponentInParent<PlayerMovement>();
		}
		if ((Object)(object)_playerRenderer == (Object)null)
		{
			MyPlayer componentInParent = ((Component)this).GetComponentInParent<MyPlayer>();
			_playerRenderer = ((componentInParent != null) ? ((Component)componentInParent).GetComponentInChildren<PlayerRenderer>() : null);
		}
		_componentsCached = (Object)(object)_playerMovement != (Object)null && (Object)(object)_playerRenderer != (Object)null;
	}

	private void Update()
	{
		if (!IsLocalPlayer)
		{
			return;
		}
		if (!_componentsCached)
		{
			CacheComponents();
			if (!_componentsCached)
			{
				return;
			}
		}
		SendMovementUpdate();
	}

	private void SendMovementUpdate()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)((Object)(object)_playerMovement)) && !((Object)((Object)(object)_playerRenderer)))
		{
			return;
		}
		Vector3 position = ((Component)_playerMovement).transform.position;
		Quaternion rotation = ((Component)_playerRenderer).transform.rotation;
		Vector3 velocity = (((Object)((Object)(object)_playerMovement)) ? _playerMovement.GetVelocity() : Vector3.zero);
		Vector3 val = position - _lastSentPosition;
		float sqrMagnitude = val.sqrMagnitude;
		float num = Quaternion.Angle(rotation, _lastSentRotation);
		if (sqrMagnitude > 0.0001f || num > 1f)
		{
			if (SteamNetworkManager.IsMultiplayer)
			{
				PlayerMovementRelayMessage relayMsg = new PlayerMovementRelayMessage
				{
					SteamUserId = SteamUserId,
					Position = position,
					Rotation = rotation,
					Velocity = velocity,
					ServerTime = (NetworkTimeSync.IsInitialized ? NetworkTimeSync.CurrentServerTime : Time.unscaledTime)
				};
				SteamNetworkClient.Instance?.SendMessage(relayMsg);
			}
			_lastSentPosition = position;
			_lastSentRotation = rotation;
		}
	}
}
