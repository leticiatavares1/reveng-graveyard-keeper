public static class CursorUtils
{
	public static void AddCursorState(this ICursorChanger changer, CursorType type)
	{
		CursorController.AddCursorState(type, changer);
	}

	public static void RemoveCursorState(this ICursorChanger changer)
	{
		CursorController.RemoveCursorState(changer);
	}
}
