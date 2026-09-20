using System;

namespace LazyBearTechnology;

[Serializable]
public class GamepadButton : Enumeration
{
	public static GamepadButton None = new GamepadButton(0);

	public static GamepadButton B = new GamepadButton(1);

	public static GamepadButton A = new GamepadButton(2);

	public static GamepadButton X = new GamepadButton(3);

	public static GamepadButton Y = new GamepadButton(4);

	public static GamepadButton LB = new GamepadButton(5);

	public static GamepadButton RB = new GamepadButton(6);

	public static GamepadButton Back = new GamepadButton(7);

	public static GamepadButton Start = new GamepadButton(8);

	public static GamepadButton DUp = new GamepadButton(9);

	public static GamepadButton DDown = new GamepadButton(10);

	public static GamepadButton DLeft = new GamepadButton(11);

	public static GamepadButton DRight = new GamepadButton(12);

	public static GamepadButton LT = new GamepadButton(13);

	public static GamepadButton RT = new GamepadButton(14);

	public static GamepadButton RStick = new GamepadButton(15);

	public static GamepadButton LStick = new GamepadButton(16);

	protected GamepadButton(int value)
		: base(value)
	{
	}
}
