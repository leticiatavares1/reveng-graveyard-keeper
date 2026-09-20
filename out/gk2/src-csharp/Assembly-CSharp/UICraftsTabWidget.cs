using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using UnityEngine;

public class UICraftsTabWidget : LazyWidget<UICraftsTabWidgetData>
{
	[SerializeField]
	private Transform craftsParent;

	[Space]
	[SerializeField]
	private Sprite defaultTabBgIcon;

	[SerializeField]
	private Sprite conveyorTabBgIcon;

	private List<UICraftPreviewItemCell> cellWidgets;

	public override void Redraw()
	{
		base.Redraw();
		DrawCraftsCells();
	}

	public override void Hide()
	{
		base.Hide();
		HideCraftsCells();
	}

	private void DrawCraftsCells()
	{
		UICraftWindow craftWindow = LazyUI.GetWindow<UICraftWindow>();
		string text = data.Crafts[0].tabId;
		bool flag = data.Crafts.Any((CraftDef x) => x.craftsIn.Contains(craftWindow.Data.AssignedWgo.Data.id));
		if (string.IsNullOrEmpty(text))
		{
			text = craftWindow.Data.AssignedWgo.Data.Definition.id;
		}
		cellWidgets = new List<UICraftPreviewItemCell>();
		bool flag2 = craftWindow.Data.ExtensionCrafts.Exists((KeyValuePair<string, List<CraftDef>> x) => x.Key == data.TabId);
		if (data.WgoData.id == "alchemy_workbench" || flag2)
		{
			text = data.TabId;
		}
		if (flag2 || !flag)
		{
			string key = text;
			bool useTabAsIcon = false;
			if (flag2 && data.WgoData.id != "alchemy_workbench" && GameBalance.Me.buildableWgos.TryGetValue(data.TabId, out var value))
			{
				text = value.BuildResultIcon;
				key = value.wgoId;
				useTabAsIcon = true;
			}
			UICraftPreviewItemCell craftWidget = craftWindow.GetCraftWidget(craftsParent.transform, flag2 ? new UICraftPreviewItemCellData(text, data.TabId, data.WgoData.WorkbenchExtensionsCrafts.ContainsKey(key), useTabAsIcon, data.IsGravePartRemove, GetTabBackgroundIcon()) : new UICraftPreviewItemCellData(null, null, null, null, text, data.IsGravePartRemove, GetTabBackgroundIcon()));
			craftWidget.transform.SetParent(craftsParent.transform);
			cellWidgets.Add(craftWidget);
		}
		foreach (CraftDef craft in data.Crafts)
		{
			UICraftPreviewItemCellData uICraftPreviewItemCellData = new UICraftPreviewItemCellData(craft, data.WgoData, data.OnCraftToQueueAdded, data.OnCraftStarted, "", data.IsGravePartRemove);
			UICraftPreviewItemCell craftWidget2 = craftWindow.GetCraftWidget(craftsParent.transform, uICraftPreviewItemCellData);
			craftWidget2.transform.SetParent(craftsParent.transform);
			cellWidgets.Add(craftWidget2);
		}
	}

	private void HideCraftsCells()
	{
		UICraftWindow window = LazyUI.GetWindow<UICraftWindow>();
		foreach (UICraftPreviewItemCell cellWidget in cellWidgets)
		{
			window.ReleaseCraftWidget(cellWidget);
		}
		cellWidgets.Clear();
	}

	private Sprite GetTabBackgroundIcon()
	{
		if (data.WgoData != null && data.WgoData.Definition.conveyorType != 0)
		{
			return conveyorTabBgIcon;
		}
		return defaultTabBgIcon;
	}

	protected override void TestDraw()
	{
	}
}
