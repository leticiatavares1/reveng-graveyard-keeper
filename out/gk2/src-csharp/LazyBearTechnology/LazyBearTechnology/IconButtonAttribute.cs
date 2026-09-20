using System;
using Sirenix.OdinInspector;

namespace LazyBearTechnology;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class IconButtonAttribute : ShowInInspectorAttribute
{
	public string iconResource;

	public int buttonHeight = 31;

	public IconButtonAttribute(string iconResource)
	{
		this.iconResource = iconResource;
	}

	public IconButtonAttribute(string iconResource, ButtonSizes buttonSize)
	{
		this.iconResource = iconResource;
		buttonHeight = (int)buttonSize;
	}
}
