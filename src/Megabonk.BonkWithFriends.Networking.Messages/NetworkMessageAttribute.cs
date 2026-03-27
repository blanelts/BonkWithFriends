using System;

namespace Megabonk.BonkWithFriends.Networking.Messages;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
internal sealed class NetworkMessageAttribute : Attribute
{
	internal readonly MessageType Type;

	internal readonly MessageSendFlags SendFlags;

	internal NetworkMessageAttribute(MessageType messageType, MessageSendFlags sendFlags)
	{
		if (messageType == MessageType.None)
		{
			throw new ArgumentOutOfRangeException("messageType");
		}
		Type = messageType;
		SendFlags = sendFlags;
	}
}
