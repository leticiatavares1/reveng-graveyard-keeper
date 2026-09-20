using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Expressive.Exceptions;

[Serializable]
public sealed class FunctionNameAlreadyRegisteredException : Exception
{
	public string Name { get; private set; }

	internal FunctionNameAlreadyRegisteredException(string name)
		: base($"A function has already been registered '{name}'")
	{
		Name = name;
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Name", Name);
	}
}
