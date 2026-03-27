using System;
using System.Reflection;
using Megabonk.BonkWithFriends.IO;
using MelonLoader;
using Semver;

namespace Megabonk.BonkWithFriends.Networking.Messages.Client;

[NetworkMessage(MessageType.ClientHello, MessageSendFlags.ReliableNoNagle)]
internal sealed class ClientHelloMessage : MessageBase
{
	internal SemVersion SemVersion { get; private set; }

	public ClientHelloMessage()
	{
	}

	internal ClientHelloMessage(SemVersion semVersion)
	{
		if (semVersion == (SemVersion)null)
		{
			throw new ArgumentNullException("semVersion");
		}
		SemVersion = semVersion;
	}

	internal void RetrieveSemVersion()
	{
		if (SemVersion != (SemVersion)null)
		{
			throw new InvalidOperationException("SemVersion");
		}
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		if (!(executingAssembly == null))
		{
			MelonInfoAttribute customAttribute = executingAssembly.GetCustomAttribute<MelonInfoAttribute>();
			if (customAttribute != null)
			{
				SemVersion = customAttribute.SemanticVersion;
			}
		}
	}

	public override void Serialize(NetworkWriter networkWriter)
	{
		if (SemVersion == (SemVersion)null)
		{
			throw new NullReferenceException("SemVersion");
		}
		int major = SemVersion.Major;
		int minor = SemVersion.Minor;
		int patch = SemVersion.Patch;
		networkWriter.Write(major);
		networkWriter.Write(minor);
		networkWriter.Write(patch);
	}

	public override void Deserialize(NetworkReader networkReader)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		int num = networkReader.ReadInt32();
		int num2 = networkReader.ReadInt32();
		int num3 = networkReader.ReadInt32();
		SemVersion = new SemVersion(num, num2, num3, "", "");
	}
}
