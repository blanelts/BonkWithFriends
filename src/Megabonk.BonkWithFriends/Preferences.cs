using MelonLoader;
using MelonLoader.Preferences;

namespace Megabonk.BonkWithFriends;

public static class Preferences
{
	private static readonly MelonPreferences_Category category;

	public static readonly MelonPreferences_Entry<int> MaxPlayers;

	public static readonly MelonPreferences_Entry<float> EnemySpawnRate;

	public static readonly MelonPreferences_Entry<float> EnemyHpModifer;

	public static readonly MelonPreferences_Entry<float> EnemyDmgModifer;

	public static readonly MelonPreferences_Entry<float> EnemySpeedModifer;

	public static readonly MelonPreferences_Entry<bool> LevelSync;

	public static readonly MelonPreferences_Entry<bool> PauseSync;

	static Preferences()
	{
		category = MelonPreferences.CreateCategory("Multibonk", "General Settings");
		MaxPlayers = category.CreateEntry<int>("MaxPlayers", 4, (string)null, "Max number of players in a match", false, false, (ValueValidator)null, (string)null);
		EnemySpawnRate = category.CreateEntry<float>("EnemySpawnRate", 2f, (string)null, "Enemy Spawn Multiplier", false, false, (ValueValidator)null, (string)null);
		EnemyHpModifer = category.CreateEntry<float>("EnemyHpModifer", 1.5f, (string)null, "Increases Enemy HP - 2.0f is double", false, false, (ValueValidator)null, (string)null);
		EnemyDmgModifer = category.CreateEntry<float>("EnemyDmgModifer", 1f, (string)null, "Increases Enemy Damage - 2.0f is double", false, false, (ValueValidator)null, (string)null);
		EnemySpeedModifer = category.CreateEntry<float>("EnemySpeedModifer", 1f, (string)null, "Increases Enemy Speed - 2.0f is double", false, false, (ValueValidator)null, (string)null);
		LevelSync = category.CreateEntry<bool>("LevelSync", true, (string)null, "Enable or disable sharing levels and XP", false, false, (ValueValidator)null, (string)null);
		PauseSync = category.CreateEntry<bool>("PauseSync", false, (string)null, "When enabled, your game will automatically pause whenever a friend pauses theirs.", false, false, (ValueValidator)null, (string)null);
	}
}
