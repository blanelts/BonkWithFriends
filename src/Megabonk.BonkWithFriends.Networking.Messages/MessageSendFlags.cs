using System;

namespace Megabonk.BonkWithFriends.Networking.Messages;

[Flags]
internal enum MessageSendFlags
{
	Unreliable = 0,
	NoNagle = 1,
	UnreliableNoNagle = 1,
	NoDelay = 4,
	UnreliableNoDelay = 5,
	Reliable = 8,
	ReliableNoNagle = 9
}
