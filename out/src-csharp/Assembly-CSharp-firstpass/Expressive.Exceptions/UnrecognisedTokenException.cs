using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Expressive.Exceptions;

[Serializable]
public sealed class UnrecognisedTokenException : Exception
{
	public string Token { get; private set; }

	internal UnrecognisedTokenException(string token)
		: base("Unrecognised token '" + token + "'")
	{
		Token = token;
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Token", Token);
	}
}
