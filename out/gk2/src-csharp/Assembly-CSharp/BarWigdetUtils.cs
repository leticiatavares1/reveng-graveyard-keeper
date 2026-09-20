using UnityEngine;

public static class BarWigdetUtils
{
	public static int ClampSliderValueToViewableState(int fillRectMaxWidth, int currentSliderValue, int maxSliderValue)
	{
		if (currentSliderValue == 0)
		{
			return 0;
		}
		if (!((float)currentSliderValue / (float)maxSliderValue * (float)fillRectMaxWidth < 1f))
		{
			return currentSliderValue;
		}
		return Mathf.RoundToInt((float)maxSliderValue / (float)fillRectMaxWidth);
	}
}
