using System;
using Il2Cpp;
using Il2CppAssets.Scripts.Objects.Pooling;
using Il2CppAssets.Scripts._Data;
using Megabonk.BonkWithFriends.Managers.Player;
using Megabonk.BonkWithFriends.Player;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace Megabonk.BonkWithFriends.MonoBehaviours.Player;

[RegisterTypeInIl2Cpp]
public class NetworkedPlayer : MonoBehaviour
{
	public PlayerState State;

	public ulong SteamId { get; private set; }

	public bool IsLocalPlayer { get; private set; }

	public bool IsHost { get; private set; }

	public ECharacter Character { get; private set; }

	public GameObject ModelInstance { get; private set; }

	public Rigidbody CachedRigidbody { get; private set; }

	public void Initialize(ulong steamId, bool isLocal, bool isHost)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		SteamId = steamId;
		IsLocalPlayer = isLocal;
		IsHost = isHost;
		if (IsLocalPlayer)
		{
			PlayerRenderer componentInChildren = ((Component)this).GetComponentInChildren<PlayerRenderer>();
			if (((Object)((Object)(object)componentInChildren)) && ((Object)((Object)(object)componentInChildren.characterData)))
			{
				Character = componentInChildren.characterData.eCharacter;
			}
			GameObject val = new GameObject("PlayerMovementBroadcaster");
			val.transform.SetParent(((Component)this).transform);
			val.AddComponent<PlayerMovementBroadcaster>().Initialize(steamId);
			GameObject val2 = new GameObject("PlayerAnimationBroadcaster");
			val2.transform.SetParent(((Component)this).transform);
			val2.AddComponent<PlayerAnimationBroadcaster>().Initialize(steamId);
			_ = IsHost;
		}
	}

	public void SetupVisuals(ECharacter character, ESkinType skinType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		TryInstantiateCharacterModel(character, skinType);
	}

	private bool TryInstantiateCharacterModel(ECharacter character, ESkinType skinType)
	{
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!((Object)((Object)(object)DataManager.Instance)) || !((Object)((Object)(object)PoolManager.Instance)))
			{
				Melon<BonkWithFriendsMod>.Logger.Warning("Data manager or Pool Manager null!");
				return false;
			}
			CharacterData characterData = DataManager.Instance.GetCharacterData(character);
			if (!((Object)((Object)(object)characterData)) || !((Object)((Object)(object)characterData.prefab)))
			{
				Melon<BonkWithFriendsMod>.Logger.Warning("charData or charData prefab null!");
				return false;
			}
			ModelInstance = Object.Instantiate<GameObject>(characterData.prefab, ((Component)this).transform);
			if (!((Object)((Object)(object)ModelInstance)))
			{
				Melon<BonkWithFriendsMod>.Logger.Warning("ModelInstance null!");
				return false;
			}
			((Object)ModelInstance).name = $"{character}_Visual";
			ModelInstance.transform.localPosition = new Vector3(0f, -1.92f, 0f);
			ModelInstance.transform.localRotation = Quaternion.identity;
			ModelInstance.SetActive(true);
			Rigidbody val = ModelInstance.AddComponent<Rigidbody>();
			val.isKinematic = true;
			val.useGravity = false;
			val.constraints = (RigidbodyConstraints)112;
			CachedRigidbody = val;
			GameObject val2 = new GameObject("RemotePlayerInterpolation");
			val2.transform.SetParent(((Component)this).transform);
			val2.AddComponent<RemotePlayerInterpolation>();
			GameObject val3 = new GameObject("RemoteAnimationController");
			val3.transform.SetParent(((Component)this).transform);
			Animator component = ModelInstance.GetComponent<Animator>();
			val3.AddComponent<RemoteAnimationController>().Initialize(component);
			GameObject val4 = new GameObject("RemoteAttackController");
			val4.transform.SetParent(((Component)this).transform);
			val4.AddComponent<RemoteAttackController>();
			string friendPersonaName = SteamFriends.GetFriendPersonaName(new CSteamID(SteamId));
			ModelInstance.AddComponent<NameplateController>().Initialize(friendPersonaName);
			foreach (Renderer componentsInChild in ModelInstance.GetComponentsInChildren<Renderer>(true))
			{
				componentsInChild.enabled = true;
				((Component)componentsInChild).gameObject.SetActive(true);
			}
			return true;
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Warning($"[NetworkedPlayer] Failed to instantiate {character} model: {ex.Message}\n{ex.StackTrace}");
			return false;
		}
	}
}
