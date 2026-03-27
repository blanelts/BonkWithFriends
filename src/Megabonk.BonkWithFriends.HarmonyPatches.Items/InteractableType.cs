namespace Megabonk.BonkWithFriends.HarmonyPatches.Items;

internal enum InteractableType : byte
{
	Chest = 0,
	ChestFree = 1,
	Shrine = 2,
	Pot = 3,
	Portal = 4,
	Other = byte.MaxValue
}
