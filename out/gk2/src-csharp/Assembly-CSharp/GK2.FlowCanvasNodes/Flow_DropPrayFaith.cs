using System.Collections.Generic;
using System.Linq;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Drop Pray Faith", 0)]
[Category("Game/Item")]
[Color("FFFFFF")]
public class Flow_DropPrayFaith : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<List<WgoData>> wgos;

	private ValueInput<GDPointData> gdPointDataFlyTarget;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Drop);
		@out = AddFlowOutput("out".CapitalizeFirst());
		wgos = AddValueInput<List<WgoData>>("wgos");
		gdPointDataFlyTarget = AddValueInput<GDPointData>("gdPointDataFlyTarget");
	}

	private void Drop(Flow flow)
	{
		if (wgos.value == null || wgos.value.Count == 0)
		{
			@out.Call(flow);
			return;
		}
		int count = wgos.value.Count;
		int num = MainGame.PlayerData.currentSermon.parishionerDatas.Sum((ParishionerData parishionerData) => parishionerData.faithToDrop);
		int num2 = num / count;
		int num3 = num % count;
		for (int i = 0; i < count; i++)
		{
			int num4 = 0;
			if (i < num3 || num2 > 0)
			{
				num4 = num2;
				if (i < num3)
				{
					num4++;
				}
			}
			if (num4 <= 0)
			{
				break;
			}
			Item droppableItem = new Item("faith", num4);
			WgoData wgoData = wgos.value[i];
			List<Item> list = new List<Item>();
			MainGame.Instance.dropSystem.DropItem(droppableItem, wgoData.WorldId, wgoData.Position, list);
			if (!TryGetParamValue(gdPointDataFlyTarget, out var value))
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
					dropView.MoveToCustomPosition(value.Position);
				}
			}
		}
		@out.Call(flow);
	}
}
