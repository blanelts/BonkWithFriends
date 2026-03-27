using Il2CppAssets.Scripts.Movement;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.MonoBehaviours.Player;

[RegisterTypeInIl2Cpp]
public class RemoteAnimationController : MonoBehaviour
{
	private Animator animator;

	private EMovementState currentState;

	public void Initialize(Animator anim)
	{
		animator = anim;
		if (!((Object)((Object)(object)animator)))
		{
			((Behaviour)this).enabled = false;
		}
	}

	public void OnAnimationStateUpdate(byte stateValue)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		EMovementState val = (EMovementState)stateValue;
		if (currentState != val)
		{
			currentState = val;
			bool flag = ((int)val & 2) > 0;
			bool flag2 = ((int)val & 8) > 0;
			bool flag3 = ((int)val & 0x10) > 0;
			bool flag4 = ((int)val & 0x20) > 0;
			bool flag5 = (int)val == 1;
			bool flag6 = flag || flag2 || flag5 || flag4;
			animator.SetBool("moving", flag);
			animator.SetBool("grounded", flag6);
			animator.SetBool("jumping", flag3);
			animator.SetBool("grinding", flag2);
			animator.SetBool("idle", flag5);
		}
	}
}
