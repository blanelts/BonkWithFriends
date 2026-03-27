using System;
using System.Reflection;
using System.Threading.Tasks;
using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages;

internal abstract class MessageBase : INetworkSerializable, IAsyncNetworkSerializable
{
	public virtual void Deserialize(NetworkReader networkReader)
	{
	}

	public virtual void Serialize(NetworkWriter networkWriter)
	{
	}

	public virtual async ValueTask DeserializeAsync(NetworkReader networkReader)
	{
	}

	public virtual async ValueTask SerializeAsync(NetworkWriter networkWriter)
	{
	}

	internal static (MessageType type, MessageSendFlags sendFlags) GetMessageTypeAndSendFlags(Type t)
	{
		if (t == null)
		{
			throw new ArgumentNullException("t");
		}
		if (!t.IsSubclassOf(typeof(MessageBase)))
		{
			throw new ArgumentException("t");
		}
		NetworkMessageAttribute customAttribute = t.GetCustomAttribute<NetworkMessageAttribute>();
		if (customAttribute == null)
		{
			throw new NullReferenceException("networkMessageAttribute");
		}
		return (type: customAttribute.Type, sendFlags: customAttribute.SendFlags);
	}
}
