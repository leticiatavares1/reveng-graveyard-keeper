using System;
using LazyBearTechnology;

[Serializable]
public class GameKeyIconType : Enumeration
{
	public static GameKeyIconType Default = new GameKeyIconType(0);

	public static GameKeyIconType Inactive = new GameKeyIconType(1);

	public GameKeyIconType(int value)
		: base(value)
	{
	}
}
