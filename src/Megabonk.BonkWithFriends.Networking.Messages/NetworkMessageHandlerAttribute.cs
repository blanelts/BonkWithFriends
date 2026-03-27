using System;

namespace Megabonk.BonkWithFriends.Networking.Messages;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate)]
internal sealed class NetworkMessageHandlerAttribute : Attribute
{
	internal readonly MessageType Type;

	internal NetworkMessageHandlerAttribute(MessageType messageType)
	{
		if (messageType == MessageType.None)
		{
			throw new ArgumentOutOfRangeException("messageType");
		}
		Type = messageType;
	}
}
