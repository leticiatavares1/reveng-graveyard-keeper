using System;

public class HubMenuItemAttribute : Attribute
{
	public string icon_name;

	public string text;

	public string tooltip;

	public HubMenuItemAttribute(string icon_name, string text = "", string tooltip = "")
	{
		this.icon_name = icon_name;
		this.text = text;
		this.tooltip = tooltip;
	}
}
