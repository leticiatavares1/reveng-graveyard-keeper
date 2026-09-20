using UnityEngine;

public class GraphicsObjectOptimizer
{
	private GameObject _go;

	public GraphicsObjectOptimizer(GameObject obj)
	{
		_go = obj;
		PixelPerfect[] componentsInChildren = obj.GetComponentsInChildren<PixelPerfect>(includeInactive: true);
		foreach (PixelPerfect pixelPerfect in componentsInChildren)
		{
			if (pixelPerfect.only_in_editor)
			{
				pixelPerfect.enabled = false;
			}
		}
	}
}
