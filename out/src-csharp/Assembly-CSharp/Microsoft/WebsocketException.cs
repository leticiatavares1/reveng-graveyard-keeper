using System;

namespace Microsoft;

internal class WebsocketException : Exception
{
	public WebsocketException()
	{
	}

	public WebsocketException(string message)
		: base(message)
	{
	}

	public WebsocketException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
