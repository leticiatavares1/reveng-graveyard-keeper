using System.Collections.Generic;
using LazyBearTechnology;
using LinqTools;
using TMPro;
using UnityEngine;

public class UIPorterStationWindow : LazyWindow<UIPorterStationWindowData>
{
	[SerializeField]
	private TextMeshProUGUI noItemsLabel;

	[SerializeField]
	private FlexibleHorizontalSizeGridLayoutGroup grid;

	[SerializeField]
	private UIInfoWidget uiInfoWidget;

	private List<UIPorterStationItemCell> drawnCells = new List<UIPorterStationItemCell>();

	public override void Redraw()
	{
		base.Redraw();
		foreach (UIPorterStationItemCell drawnCell in drawnCells)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(drawnCell);
		}
		drawnCells.Clear();
		BuildingDef dataOrNull = GameBalance.Me.GetDataOrNull<BuildingDef>(data.Station.id + "_p");
		if (dataOrNull == null)
		{
			dataOrNull = GameBalance.Me.GetDataOrNull<BuildingDef>(data.Station.id + "_s");
			if (dataOrNull == null)
			{
				uiInfoWidget.Draw(new UIInfoWidgetData(data.Station, "i_b_grn_zombie_delivery", defineIconBackgroundFromWgo: false));
			}
			else
			{
				uiInfoWidget.Draw(new UIInfoWidgetData(data.Station, dataOrNull.BuildResultIcon, defineIconBackgroundFromWgo: false));
			}
		}
		else
		{
			uiInfoWidget.Draw(new UIInfoWidgetData(data.Station, dataOrNull.BuildResultIcon, defineIconBackgroundFromWgo: false));
		}
		if (data.PorterStationDef.items.Count > 0)
		{
			noItemsLabel.gameObject.SetActive(value: false);
			grid.gameObject.SetActive(value: true);
			foreach (NeedItemData item in data.PorterStationDef.items)
			{
				UIPorterStationItemCell elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UIPorterStationItemCell>(base.transform);
				elementFromPool.Draw(new Item(item.id), data.Station.GetGameResInt(item.id) == 1, data.OnItemCellPress);
				drawnCells.Add(elementFromPool);
			}
		}
		else
		{
			noItemsLabel.gameObject.SetActive(value: true);
			grid.gameObject.SetActive(value: false);
		}
		grid.Draw(drawnCells.Select((UIPorterStationItemCell go) => go.RectTransform).ToList());
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		WgoData station = new WgoData("porter_station_carrier", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		Open(new UIPorterStationWindowData(station));
	}
}
