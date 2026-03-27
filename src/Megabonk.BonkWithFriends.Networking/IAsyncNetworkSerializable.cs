using System.Threading.Tasks;
using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking;

internal interface IAsyncNetworkSerializable
{
	internal ValueTask SerializeAsync(NetworkWriter networkWriter);

	internal ValueTask DeserializeAsync(NetworkReader networkReader);
}
