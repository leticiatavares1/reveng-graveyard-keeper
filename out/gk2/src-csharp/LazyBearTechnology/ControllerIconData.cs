using System;
using LazyBearTechnology;

[Serializable]
public class ControllerIconData
{
	public GameKey gameKey;

	public ControllerIconDataTyped[] typedIcons;

	public ControllerIconData(GameKey gameKey, ControllerIconDataTyped[] typedIcons)
	{
		this.gameKey = gameKey;
		this.typedIcons = typedIcons;
	}
}
