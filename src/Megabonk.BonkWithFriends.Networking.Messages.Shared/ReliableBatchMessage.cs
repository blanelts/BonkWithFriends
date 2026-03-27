using System;
using System.Collections.Generic;
using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.ReliableBatch, MessageSendFlags.ReliableNoNagle)]
internal sealed class ReliableBatchMessage : MessageBase
{
	private readonly List<MessageBase> _messages;

	internal ReliableBatchMessage()
	{
		_messages = new List<MessageBase>();
	}

	internal ReliableBatchMessage(List<MessageBase> messages)
	{
		if (messages == null || messages.Count <= 0)
		{
			throw new ArgumentNullException("messages");
		}
		_messages = messages;
	}

	public override void Serialize(NetworkWriter networkWriter)
	{
		foreach (MessageBase message in _messages)
		{
			message.Serialize(networkWriter);
		}
	}

	public override void Deserialize(NetworkReader networkReader)
	{
		foreach (MessageBase message in _messages)
		{
			message.Deserialize(networkReader);
		}
	}
}
