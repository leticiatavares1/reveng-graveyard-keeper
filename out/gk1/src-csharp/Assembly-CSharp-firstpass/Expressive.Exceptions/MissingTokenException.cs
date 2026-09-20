using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Expressive.Exceptions;

[Serializable]
public sealed class MissingTokenException : Exception
{
	public char MissingToken { get; private set; }

	internal MissingTokenException(string message, char missingToken)
		: base(message)
	{
		MissingToken = missingToken;
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("MissingToken", MissingToken);
	}
}
