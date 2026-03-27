using System;
using System.Collections.Generic;
using Megabonk.BonkWithFriends.IO;

namespace Megabonk.BonkWithFriends.Networking.Messages.Shared;

[NetworkMessage(MessageType.UnreliableBatch, MessageSendFlags.NoNagle)]
internal sealed class UnreliableBatchMessage : MessageBase
{
	private readonly List<MessageBase> _messages;

	internal UnreliableBatchMessage()
	{
		_messages = new List<MessageBase>();
	}

	internal UnreliableBatchMessage(List<MessageBase> messages)
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
