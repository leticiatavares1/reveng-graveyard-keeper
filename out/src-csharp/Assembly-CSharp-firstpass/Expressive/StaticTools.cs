namespace Expressive;

public static class StaticTools
{
	public static bool HasFlag(this ExpressiveOptions o, ExpressiveOptions flag)
	{
		return (o & flag) != 0;
	}
}
