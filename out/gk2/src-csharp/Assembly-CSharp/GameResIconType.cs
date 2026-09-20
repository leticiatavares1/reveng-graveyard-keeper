using System;
using LazyBearTechnology;

[Serializable]
public class GameResIconType : Enumeration
{
	public static GameResIconType Common = new GameResIconType(0);

	public static GameResIconType MoneyBig = new GameResIconType(1);

	public static GameResIconType TechPointSmall = new GameResIconType(2);

	public GameResIconType(int value)
		: base(value)
	{
	}
}
