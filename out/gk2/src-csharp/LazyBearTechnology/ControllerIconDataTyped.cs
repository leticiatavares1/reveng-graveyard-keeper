using System;

[Serializable]
public class ControllerIconDataTyped
{
	public GameKeyIconType iconType;

	public string iconId;

	public ControllerIconDataTyped(GameKeyIconType iconType, string iconId)
	{
		this.iconType = iconType;
		this.iconId = iconId;
	}
}
