using System;
using Rewired.Data.Mapping;

namespace LazyBearTechnology;

[Serializable]
public class GamepadTypeConfiguration
{
	public GamepadType gamepadType;

	public HardwareJoystickMap[] maps;
}
