using Il2Cpp;
using Il2CppAssets.Scripts.Movement;
using Megabonk.BonkWithFriends.Networking.Messages.Shared;
using Megabonk.BonkWithFriends.Networking.Steam;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.Player;

[RegisterTypeInIl2Cpp]
public class PlayerAnimationBroadcaster : MonoBehaviour
{
	private EMovementState _lastSentState;

	private PlayerMovement _playerMovement;

	private float _lastSendTime;

	private const float SYNC_INTERVAL = 0.1f;

	public ulong SteamUserId { get; private set; }

	private void Awake()
	{
		_playerMovement = ((Component)this).GetComponentInParent<PlayerMovement>();
	}

	public void Initialize(ulong steamId)
	{
		SteamUserId = steamId;
	}

	private void Update()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)((Object)(object)_playerMovement)) || Time.unscaledTime - _lastSendTime < 0.05f)
		{
			return;
		}
		EMovementState movementState = _playerMovement.GetMovementState();
		if (movementState != _lastSentState)
		{
			_lastSentState = movementState;
			_lastSendTime = Time.unscaledTime;
			if (SteamNetworkManager.IsMultiplayer)
			{
				AnimationStateRelayMessage relayMsg = new AnimationStateRelayMessage
				{
					SteamUserId = SteamUserId,
					StateFlags = (byte)movementState
				};
				SteamNetworkClient.Instance?.SendMessage(relayMsg);
			}
		}
	}
}
