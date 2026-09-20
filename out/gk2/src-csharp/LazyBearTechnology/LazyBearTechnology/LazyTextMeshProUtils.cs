using TMPro;
using UnityEngine;

namespace LazyBearTechnology;

public static class LazyTextMeshProUtils
{
	public static void FitToAspectOrOverflowAndGetScrollHeight(this TextMeshProUGUI label, float aspectRatio, float minHeight, float maxHeight, out float heightForScroll, float step = 5f)
	{
		label.textWrappingMode = TextWrappingModes.Normal;
		label.overflowMode = TextOverflowModes.Overflow;
		label.autoSizeTextContainer = false;
		float num = minHeight * aspectRatio;
		float size = minHeight;
		heightForScroll = maxHeight;
		float num2 = minHeight;
		float num3 = maxHeight;
		bool flag = false;
		while (num3 - num2 > step)
		{
			float num4 = (num2 + num3) / 2f;
			float num5 = num4 * aspectRatio;
			if (label.GetPreferredValues(label.text, num5, float.PositiveInfinity).y <= num4)
			{
				num3 = num4;
				num = num5;
				size = num4;
				heightForScroll = num4;
				flag = true;
			}
			else
			{
				num2 = num4;
			}
		}
		if (!flag)
		{
			num = maxHeight * aspectRatio;
			size = label.GetPreferredValues(label.text, num, float.PositiveInfinity).y;
			heightForScroll = maxHeight;
		}
		label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
		label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num);
	}
}
