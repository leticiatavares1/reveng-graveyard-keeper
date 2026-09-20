using System;

namespace LazyBearTechnology;

[Serializable]
public class VirtualCursorState : Enumeration
{
	public static VirtualCursorState Default = new VirtualCursorState(0);

	public static VirtualCursorState OverObject = new VirtualCursorState(1);

	public VirtualCursorState(int value)
		: base(value)
	{
	}
}
