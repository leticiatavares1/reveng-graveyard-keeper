using System;

[Serializable]
public class CursorState
{
	public CursorType type;

	public readonly ICursorChanger changer;

	public CursorState(CursorType type, ICursorChanger changer)
	{
		this.type = type;
		this.changer = changer;
	}
}
