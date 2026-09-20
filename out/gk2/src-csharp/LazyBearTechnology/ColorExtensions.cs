using System.Text;
using UnityEngine;

public static class ColorExtensions
{
	public static string ToHex(this Color32 color, bool useAlpha = false)
	{
		StringBuilder stringBuilder = new StringBuilder().Append(color.r.ToString("X2")).Append(color.g.ToString("X2")).Append(color.b.ToString("X2"));
		if (useAlpha)
		{
			stringBuilder.Append(color.a.ToString("X2"));
		}
		return stringBuilder.ToString();
	}
}
