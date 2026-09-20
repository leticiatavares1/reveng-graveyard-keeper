using System;

namespace Microsoft;

internal class ErrorEventArgs : EventArgs
{
	public readonly string Message;

	public readonly int Code;

	public ErrorEventArgs(string errorMessage)
		: this(1, errorMessage)
	{
	}

	public ErrorEventArgs(int code, string errorMessage)
	{
		Code = code;
		Message = errorMessage;
	}
}
