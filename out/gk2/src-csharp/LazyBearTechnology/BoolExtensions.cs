public static class BoolExtensions
{
	public static int ToInt(this bool value, int bit = 0)
	{
		if (!value)
		{
			return 0;
		}
		return 1 << bit;
	}
}
