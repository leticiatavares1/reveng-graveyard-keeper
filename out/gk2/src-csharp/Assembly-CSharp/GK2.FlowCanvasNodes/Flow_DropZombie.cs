using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Drop Zombie", 0)]
[Category("Game/Item")]
[Color("FFFFFF")]
public class Flow_DropZombie : GKCustomFlowNode
{
	public enum DropTargetType
	{
		Player,
		Wgo,
		GDPoint
	}

	[GatherPortsCallback]
	public DropTargetType dropTargetType = DropTargetType.Wgo;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> bodyId;

	private ValueInput<WgoData> wgoDataDropOn;

	private ValueInput<GDPointData> gdPointDataDropOn;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Drop);
		@out = AddFlowOutput("out".CapitalizeFirst());
		bodyId = AddValueInput<string>("bodyId");
		switch (dropTargetType)
		{
		case DropTargetType.Wgo:
			wgoDataDropOn = AddValueInput<WgoData>("wgoDataDropOn");
			break;
		case DropTargetType.GDPoint:
			gdPointDataDropOn = AddValueInput<GDPointData>("gdPointDataDropOn");
			break;
		case DropTargetType.Player:
			break;
		}
	}

	private void Drop(Flow flow)
	{
		Vector3 pos = Vector3.zero;
		string text = string.Empty;
		bool flag = false;
		switch (dropTargetType)
		{
		case DropTargetType.Player:
		{
			Vector2 vector = MainGame.PlayerData.Direction * 1f;
			pos = MainGame.PlayerController.PlayerData.position.Value + new Vector3(vector.x, 0f, vector.y);
			text = MainGame.PlayerData.currentGameSceneId;
			flag = true;
			break;
		}
		case DropTargetType.Wgo:
		{
			if (TryGetParamValue(wgoDataDropOn, out var value2))
			{
				pos = value2.Position;
				text = value2.WorldId;
				flag = true;
			}
			break;
		}
		case DropTargetType.GDPoint:
		{
			if (TryGetParamValue(gdPointDataDropOn, out var value))
			{
				pos = value.Position;
				text = value.GameSceneDataId;
				flag = true;
			}
			break;
		}
		}
		if (flag)
		{
			Item item = GameBalance.Me.GetData<BodyDef>(bodyId.value).GenerateItem();
			MainGame.ZombieSystemData.CreateZombieDrop("zombie", pos, text, item);
			MainGame.Instance.dropSystem.DropItem(item, text, pos);
		}
		@out.Call(flow);
	}
}
