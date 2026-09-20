using System;
using UnityEngine;

[Serializable]
public class IngameProgressBar : MonoBehaviour
{
	public enum ScaleType
	{
		Vertical,
		Horizontal
	}

	public GameObject bar_object;

	public ScaleType scale_type;

	public float min_scale;

	public float max_scale;

	public string game_res;

	public void UpdateBar()
	{
		WorldObjectPart component = GetComponent<WorldObjectPart>();
		if (component == null)
		{
			return;
		}
		WorldGameObject parent = component.parent;
		if (!(parent == null))
		{
			float param = parent.GetParam(game_res);
			float num = min_scale + (max_scale - min_scale) * param;
			Vector3 localScale = bar_object.transform.localScale;
			switch (scale_type)
			{
			case ScaleType.Vertical:
				bar_object.transform.localScale = new Vector3(localScale.x, num, localScale.z);
				break;
			case ScaleType.Horizontal:
				bar_object.transform.localScale = new Vector3(num, localScale.y, localScale.z);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}
