using System;
using LazyBearTechnology;

public class UIPorterStationWindowData : LazyWidgetDataBase
{
	public PorterStationDef PorterStationDef { get; private set; }

	public WgoData Station { get; private set; }

	public Action<UIItemCell> OnItemCellPress { get; private set; }

	public UIPorterStationWindowData(WgoData station)
	{
		PorterStationDef = GameBalance.Me.GetData<PorterStationDef>(station.id);
		Station = station;
		OnItemCellPress = delegate(UIItemCell cell)
		{
			bool flag = station.GetGameResInt(cell.DisplayingItem.id) == 1;
			station.SetGameRes(cell.DisplayingItem.id, (!flag) ? 1 : 0);
		};
	}
}
