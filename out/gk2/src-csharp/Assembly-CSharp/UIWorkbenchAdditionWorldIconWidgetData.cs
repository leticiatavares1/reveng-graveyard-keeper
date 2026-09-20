using LazyBearTechnology;

public class UIWorkbenchAdditionWorldIconWidgetData : LazyWidgetDataBase
{
	public string IconId { get; }

	public bool IsInRange { get; }

	public bool ShowBackground { get; }

	public string CrossIconId { get; }

	public UIWorkbenchAdditionWorldIconWidgetData(string iconId, bool isInRange)
		: this(iconId, isInRange, showBackground: true, null)
	{
	}

	private UIWorkbenchAdditionWorldIconWidgetData(string iconId, bool isInRange, bool showBackground, string crossIconId)
	{
		IconId = iconId;
		IsInRange = isInRange;
		ShowBackground = showBackground;
		CrossIconId = crossIconId;
	}

	public static UIWorkbenchAdditionWorldIconWidgetData StationWithoutCaretaker()
	{
		return new UIWorkbenchAdditionWorldIconWidgetData("i_no_zombie_delivery", isInRange: true, showBackground: false, null);
	}

	public static UIWorkbenchAdditionWorldIconWidgetData NoCaretakerInZone()
	{
		return new UIWorkbenchAdditionWorldIconWidgetData(ZombieDeliveryIndication.GetStationIconId(), isInRange: false, showBackground: false, "i_red_cross_big_icon");
	}
}
