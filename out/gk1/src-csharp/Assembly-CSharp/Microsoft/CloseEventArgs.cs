using System;

namespace Microsoft;

internal class CloseEventArgs : EventArgs
{
	public readonly string Reason;

	public readonly int Code;

	public CloseEventArgs(int code, string reason)
	{
		Code = code;
		Reason = reason;
	}
}
