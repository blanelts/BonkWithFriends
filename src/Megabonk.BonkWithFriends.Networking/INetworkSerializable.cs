using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking;

internal interface INetworkSerializable
{
	internal void Serialize(NetworkWriter networkWriter);

	internal void Deserialize(NetworkReader networkReader);
}
