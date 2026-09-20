using System.Collections.Generic;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Drop Wgo Inventory", 0)]
[Category("Game/Item")]
[Color("FFFFFF")]
public class Flow_DropInventory : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool shouldFly;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<GDPointData> gdPointDataFlyTarget;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), Drop);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (shouldFly)
		{
			gdPointDataFlyTarget = AddValueInput<GDPointData>("gdPointDataFlyTarget");
		}
	}

	private void Drop(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		Vector3 pos = wgoData.Position;
		string worldId = wgoData.WorldId;
		foreach (Item item in wgoData.Inventory.Data.Inventory)
		{
			List<Item> list = new List<Item>();
			MainGame.Instance.dropSystem.DropItem(item, worldId, pos, list);
			if (!shouldFly || !TryGetParamValue(gdPointDataFlyTarget, out var value))
			{
				continue;
			}
			foreach (Item item2 in list)
			{
				DropView dropView = null;
				foreach (GameScene loadedGameScene in LazySingleton<GameSceneManager>.Instance.LoadedGameScenes)
				{
					dropView = loadedGameScene.GetDropView(item2);
					if (dropView != null)
					{
						break;
					}
				}
				if (dropView != null)
				{
					dropView.MoveToCustomPosition(value.Position);
				}
			}
		}
		wgoData.Inventory.Data.RemoveAllItems();
		@out.Call(flow);
	}
}
