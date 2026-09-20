using System.Collections.Generic;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Drop Item", 0)]
[Category("Game/Item")]
[Color("FFFFFF")]
public class Flow_DropItem : GKCustomFlowNode
{
	public enum DropTargetType
	{
		Player,
		Wgo,
		GDPoint
	}

	[GatherPortsCallback]
	public DropTargetType dropTargetType = DropTargetType.Wgo;

	[GatherPortsCallback]
	public bool shouldFly;

	[GatherPortsCallback]
	public bool dropBody;

	[GatherPortsCallback]
	public bool customCountForItem;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<Item> item;

	private ValueInput<int> count;

	private ValueInput<WgoData> wgoDataDropOn;

	private ValueInput<GDPointData> gdPointDataDropOn;

	private ValueInput<GDPointData> gdPointDataFlyTarget;

	private Item lastDroppedItem;

	private ValueOutput<Item> droppedItem;

	public override string name => string.Format("Drop {0} On {1}{2}", (!dropBody) ? "Item" : "Body", dropTargetType, shouldFly ? " And Fly" : "");

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Drop);
		@out = AddFlowOutput("out".CapitalizeFirst());
		item = AddValueInput<Item>("item");
		item.skipSelfInstanceAssignment = false;
		if (customCountForItem)
		{
			count = AddValueInput<int>("count");
			count.serializedValue = 1;
		}
		switch (dropTargetType)
		{
		case DropTargetType.Wgo:
			wgoDataDropOn = AddValueInput<WgoData>("wgoDataDropOn");
			break;
		case DropTargetType.GDPoint:
			gdPointDataDropOn = AddValueInput<GDPointData>("gdPointDataDropOn");
			break;
		}
		if (shouldFly)
		{
			gdPointDataFlyTarget = AddValueInput<GDPointData>("gdPointDataFlyTarget");
		}
		droppedItem = AddValueOutput("droppedItem", () => lastDroppedItem);
	}

	private void Drop(Flow flow)
	{
		Vector3 pos = Vector3.zero;
		string worldId = string.Empty;
		bool flag = false;
		switch (dropTargetType)
		{
		case DropTargetType.Player:
			pos = MainGame.PlayerController.PlayerData.position.Value;
			worldId = MainGame.PlayerData.currentGameSceneId;
			flag = true;
			break;
		case DropTargetType.Wgo:
		{
			if (TryGetParamValue(wgoDataDropOn, out var value2))
			{
				pos = value2.Position;
				worldId = value2.WorldId;
				flag = true;
			}
			break;
		}
		case DropTargetType.GDPoint:
		{
			if (TryGetParamValue(gdPointDataDropOn, out var value))
			{
				pos = value.Position;
				worldId = value.GameSceneDataId;
				flag = true;
			}
			break;
		}
		}
		if (flag)
		{
			Item value3 = item.value;
			bool flag2 = value3.Definition != null && value3.Definition.itemGroupIds.Contains("body");
			lastDroppedItem = ((dropBody || flag2) ? value3 : new Item(value3.id, customCountForItem ? count.value : value3.Count));
			List<Item> list = new List<Item>();
			if (lastDroppedItem.Definition == null)
			{
				Debug.LogError("DropItem Error: No ItemDef found for id \"" + lastDroppedItem.id + "\"");
				@out.Call(flow);
				return;
			}
			MainGame.Instance.dropSystem.DropItem(lastDroppedItem, worldId, pos, list);
			if (shouldFly && TryGetParamValue(gdPointDataFlyTarget, out var value4))
			{
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
						dropView.MoveToCustomPosition(value4.Position);
					}
				}
			}
		}
		@out.Call(flow);
	}
}
