using System;
using LazyBearTechnology;

public class UISaveSlotsWindowLimitedPopUpData : LazyWidgetDataBase
{
	public int TargetSlotIndex { get; }

	public Action OnImported { get; }

	public UISaveSlotsWindowLimitedPopUpData(int targetSlotIndex, Action onImported)
	{
		TargetSlotIndex = targetSlotIndex;
		OnImported = onImported;
	}
}
