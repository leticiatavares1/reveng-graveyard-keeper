using System;

namespace ParadoxNotion.Design;

[AttributeUsage(AttributeTargets.Class)]
public class IconAttribute : Attribute
{
	public string iconName;

	public bool fixedColor;

	public string runtimeIconTypeCallback;

	public IconAttribute(string iconName = "", bool fixedColor = false, string runtimeIconTypeCallback = "")
	{
		this.iconName = iconName;
		this.fixedColor = fixedColor;
		this.runtimeIconTypeCallback = runtimeIconTypeCallback;
	}
}
