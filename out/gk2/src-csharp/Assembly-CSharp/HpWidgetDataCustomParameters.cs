using UnityEngine;

public class HpWidgetDataCustomParameters : MonoBehaviour
{
	public bool hasCustomWidth;

	public float customWidth;

	public bool hasCustomHeight;

	public float customHeight;

	public bool GetCustomWidthIfHasSet(out float width)
	{
		width = (hasCustomWidth ? customWidth : 0f);
		return hasCustomWidth;
	}

	public bool GetCustomHeightIfHasSet(out float height)
	{
		height = (hasCustomHeight ? customHeight : 0f);
		return hasCustomHeight;
	}
}
