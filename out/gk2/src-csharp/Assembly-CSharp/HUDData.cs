using System.Collections.Generic;
using LazyBearTechnology;

public class HUDData : LazyWidgetDataBase
{
	public UIEnergySanityBarData EnergySanityBarData { get; private set; }

	public UIGameResNotificatorData GameResNotificatiorData { get; private set; }

	public UIBuffsDisplayData BuffsDisplayData { get; private set; }

	public UIHotBarWidgetData HotBarWidgetData { get; private set; }

	public HUDData(GameSave gameSave)
	{
		EnergySanityBarData = new UIEnergySanityBarData(gameSave);
		GameResNotificatiorData = new UIGameResNotificatorData();
		BuffsDisplayData = new UIBuffsDisplayData(new List<PerkType> { PerkType.Buff });
		HotBarWidgetData = new UIHotBarWidgetData(gameSave, isUsable: true);
	}
}
