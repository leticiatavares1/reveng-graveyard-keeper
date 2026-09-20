using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
[RequireComponent(typeof(RectTransform))]
public class OverrideLayoutElement : LayoutElement
{
	public enum SizeSnappingType
	{
		Even,
		Odd
	}

	public float maxHeight;

	public float maxWidth;

	public bool useMaxWidth;

	public bool useMaxHeight;

	public bool useHeightSnapping;

	public SizeSnappingType heightSnappingType;

	private bool ignoreOnGettingPreferedSize;

	public override int layoutPriority
	{
		get
		{
			if (!ignoreOnGettingPreferedSize)
			{
				return base.layoutPriority;
			}
			return -1;
		}
		set
		{
			base.layoutPriority = value;
		}
	}

	public override float preferredHeight
	{
		get
		{
			if (!useMaxHeight)
			{
				if (!useHeightSnapping)
				{
					return base.preferredHeight;
				}
				return GetSnappedValue(base.preferredHeight, heightSnappingType);
			}
			bool flag = ignoreOnGettingPreferedSize;
			ignoreOnGettingPreferedSize = true;
			float num = LayoutUtility.GetPreferredHeight(base.transform as RectTransform);
			ignoreOnGettingPreferedSize = flag;
			float num2 = ((num > maxHeight) ? maxHeight : num);
			if (!useHeightSnapping)
			{
				return num2;
			}
			return GetSnappedValue(num2, heightSnappingType);
		}
		set
		{
			base.preferredHeight = value;
		}
	}

	public override float preferredWidth
	{
		get
		{
			if (!useMaxWidth)
			{
				return base.preferredWidth;
			}
			bool flag = ignoreOnGettingPreferedSize;
			ignoreOnGettingPreferedSize = true;
			float num = LayoutUtility.GetPreferredWidth(base.transform as RectTransform);
			ignoreOnGettingPreferedSize = flag;
			if (!(num > maxWidth))
			{
				return num;
			}
			return maxWidth;
		}
		set
		{
			base.preferredWidth = value;
		}
	}

	public override float minHeight
	{
		get
		{
			if (!useHeightSnapping)
			{
				return base.minHeight;
			}
			return GetSnappedValue(base.minHeight, heightSnappingType);
		}
	}

	private float GetSnappedValue(float actualSize, SizeSnappingType snappingType)
	{
		int num = Mathf.RoundToInt(actualSize);
		float result = actualSize;
		switch (snappingType)
		{
		case SizeSnappingType.Even:
			if (num % 2 == 1)
			{
				result = num + 1;
			}
			break;
		case SizeSnappingType.Odd:
			if (num % 2 == 0)
			{
				result = actualSize + 1f;
			}
			break;
		}
		return result;
	}
}
