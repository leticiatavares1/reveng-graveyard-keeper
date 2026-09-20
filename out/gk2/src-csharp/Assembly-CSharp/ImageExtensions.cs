using UnityEngine;
using UnityEngine.UI;

public static class ImageExtensions
{
	private static readonly int tint = Shader.PropertyToID("_Color");

	public static void BlueColorReplace(this Image image, Color color)
	{
		Material material = image.material;
		image.material = new Material(material);
		image.material.SetColor(tint, color);
		if (material.name.EndsWith("(Instance)"))
		{
			Object.Destroy(material);
		}
	}
}
