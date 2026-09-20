using System;

namespace Microsoft;

internal class MessageEventArgs : EventArgs
{
	public readonly string Message;

	public MessageEventArgs(string message)
	{
		Message = message;
	}
}
