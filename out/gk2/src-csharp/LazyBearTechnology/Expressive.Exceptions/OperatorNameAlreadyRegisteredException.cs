using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Expressive.Exceptions;

[Serializable]
public sealed class OperatorNameAlreadyRegisteredException : Exception
{
	public string Tag { get; }

	internal OperatorNameAlreadyRegisteredException(string tag)
		: base("An operator has already been registered '" + tag + "'")
	{
		Tag = tag;
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Tag", Tag);
	}
}
