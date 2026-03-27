namespace Megabonk.BonkWithFriends.Networking.Steam;

internal enum SteamNetConnectionEnd
{
	Invalid = 0,
	Generic = 1000,
	ServerShutdown = 1001,
	ClientShutdown = 1002,
	ExceptionGeneric = 2000,
	ServerError = 2001,
	ClientError = 2002
}
