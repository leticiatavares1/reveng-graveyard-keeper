using System;
using System.Collections.Generic;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Drop Item And Split", 0)]
[Category("Game/Item")]
[Color("FFFFFF")]
public class Flow_DropItemAndSplitBetweenWgos : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool shouldFly;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<Item> item;

	private ValueInput<List<WgoData>> wgos;

	private ValueInput<GDPointData> gdPointDataFlyTarget;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Drop);
		@out = AddFlowOutput("out".CapitalizeFirst());
		item = AddValueInput<Item>("item");
		wgos = AddValueInput<List<WgoData>>("wgos");
		if (shouldFly)
		{
			gdPointDataFlyTarget = AddValueInput<GDPointData>("gdPointDataFlyTarget");
		}
	}

	private void Drop(Flow flow)
	{
		if (wgos.value == null || wgos.value.Count == 0 || item.value == null || item.value.Count == 0)
		{
			@out.Call(flow);
			return;
		}
		int count = wgos.value.Count;
		int count2 = item.value.Count;
		int num = Math.Min(count, count2);
		int num2 = count2;
		for (int i = 0; i < num; i++)
		{
			if (num2 <= 0)
			{
				break;
			}
			int value = ((count2 <= count) ? (count / count2) : (count2 / count));
			value = Math.Clamp(value, 0, num2);
			num2 -= value;
			Item droppableItem = new Item(item.value.id, value);
			WgoData wgoData = wgos.value[Math.Clamp(i, 0, wgos.value.Count - 1)];
			List<Item> list = new List<Item>();
			MainGame.Instance.dropSystem.DropItem(droppableItem, wgoData.WorldId, wgoData.Position, list);
			if (!shouldFly || !TryGetParamValue(gdPointDataFlyTarget, out var value2))
			{
				continue;
			}
			foreach (Item item in list)
			{
				DropView dropView = null;
				foreach (GameScene loadedGameScene in LazySingleton<GameSceneManager>.Instance.LoadedGameScenes)
				{
					dropView = loadedGameScene.GetDropView(item);
					if (dropView != null)
					{
						break;
					}
				}
				if (dropView != null)
				{
					dropView.MoveToCustomPosition(value2.Position);
				}
			}
		}
		@out.Call(flow);
	}
}
