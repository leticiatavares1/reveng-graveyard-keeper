using System;

namespace LazyBearTechnology;

[Serializable]
public class GamepadType : Enumeration
{
	public static GamepadType Xbox_XboxController = new GamepadType(0);

	public static GamepadType Sony_DualShock = new GamepadType(1);

	public static GamepadType Sony_DualSense = new GamepadType(2);

	public static GamepadType Switch_JoyCon_Dual = new GamepadType(3);

	public static GamepadType Switch_JoyCon_Left = new GamepadType(4);

	public static GamepadType Switch_JoyCon_Right = new GamepadType(5);

	public static GamepadType Switch_Pro = new GamepadType(6);

	public static GamepadType Switch_Handheld = new GamepadType(7);

	public GamepadType(int value)
		: base(value)
	{
	}
}
