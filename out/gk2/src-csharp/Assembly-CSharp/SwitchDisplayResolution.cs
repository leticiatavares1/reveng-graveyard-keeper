public static class SwitchDisplayResolution
{
	public static bool TryGet(out int width, out int height)
	{
		width = 0;
		height = 0;
		return false;
	}

	public static void InitChangeEvent()
	{
	}

	public static bool TryConsumeChange()
	{
		return false;
	}
}
