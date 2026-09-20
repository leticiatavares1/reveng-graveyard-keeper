using System;
using System.Globalization;
using UnityEngine;

namespace ParadoxNotion.Design;

[AttributeUsage(AttributeTargets.Class)]
public class ColorAttribute : Attribute
{
	public string hexColor;

	private Color32? resolved;

	public ColorAttribute(string hexColor)
	{
		this.hexColor = hexColor;
	}

	public Color32 Resolve()
	{
		if (resolved.HasValue)
		{
			return resolved.Value;
		}
		resolved = default(Color32);
		if (hexColor.Length == 6)
		{
			byte r = byte.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber);
			byte g = byte.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber);
			byte b = byte.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber);
			resolved = new Color32(r, g, b, byte.MaxValue);
		}
		return resolved.Value;
	}
}
