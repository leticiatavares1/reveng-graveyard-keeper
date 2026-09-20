namespace Microsoft;

internal class Reason
{
	public ushort code;

	public string reason;

	public bool isClose;

	public Reason(ushort code, string reason, bool isClose)
	{
		this.code = code;
		this.reason = reason;
		this.isClose = true;
	}
}
