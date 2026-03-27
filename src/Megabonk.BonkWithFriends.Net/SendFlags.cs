namespace Megabonk.BonkWithFriends.Net;

public static class SendFlags
{
	public const int Unreliable = 0;

	public const int NoNagle = 1;

	public const int UnreliableNoNagle = 1;

	public const int NoDelay = 4;

	public const int UnreliableNoDelay = 4;

	public const int Reliable = 8;

	public const int ReliableNoNagle = 9;

	public static int For(Op op)
	{
		switch (op)
		{
		case Op.PlayerMovement:
		case Op.PlayerMovementRelay:
		case Op.EnemyDamaged:
		case Op.WeaponProjectileSpawned:
		case Op.WeaponProjectileHit:
		case Op.AnimationState:
		case Op.TimelineEvent:
		case Op.WaveCue:
		case Op.WaveFinalCue:
		case Op.BossSpawnSync:
		case Op.BossDied:
		case Op.WavesStopped:
			return 1;
		case Op.HostWelcome:
		case Op.ClientIntroduce:
		case Op.PlayerJoined:
		case Op.PlayerLeft:
		case Op.SeedSync:
		case Op.PlayerState:
		case Op.Chat:
		case Op.LoadLevel:
		case Op.Ready:
		case Op.XpGained:
		case Op.LevelUp:
		case Op.PickupSpawned:
		case Op.EnemySpawned:
		case Op.EnemyDied:
		case Op.EnemyStateBatch:
		case Op.WeaponAdded:
		case Op.WeaponAttackStarted:
		case Op.GameStarted:
		case Op.GameOver:
		case Op.InteractableSpawnBatch:
		case Op.InteractableUsed:
			return 9;
		default:
			return 0;
		}
	}
}
