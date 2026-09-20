using System;
using LazyBearTechnology;

public class PerkWidgetData : LazyWidgetDataBase
{
	public PerkData PerkData { get; private set; }

	public Action<PerkWidgetData> OnPress { get; private set; }

	public Action<PerkWidgetData> OnOver { get; private set; }

	public Action<PerkWidgetData> OnOut { get; private set; }

	public bool IsActive { get; set; }

	public PerkWidgetData(PerkData perkData, bool isActive, Action<PerkWidgetData> onPress, Action<PerkWidgetData> onOver, Action<PerkWidgetData> onOut)
	{
		PerkData = perkData;
		OnPress = onPress;
		OnOver = onOver;
		OnOut = onOut;
		IsActive = isActive;
	}
}
