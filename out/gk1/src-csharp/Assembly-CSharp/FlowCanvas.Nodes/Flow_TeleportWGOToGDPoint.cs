using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Teleport WGO To GD Point", 0)]
public class Flow_TeleportWGOToGDPoint : MyFlowNode
{
	public bool dont_move_camera_while_tp;

	public override string name
	{
		get
		{
			if (GetInputValuePort<WorldGameObject>("Who").value == null && !GetInputValuePort<WorldGameObject>("Who").isConnected)
			{
				return "Player Teleport To GD point";
			}
			return base.name;
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_who = AddValueInput<WorldGameObject>("Who");
		ValueInput<string> in_gd_point_tag = AddValueInput<string>("GD Point Tag");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = in_who.value;
			if (worldGameObject == null)
			{
				worldGameObject = MainGame.me.player;
			}
			else
			{
				worldGameObject.RedrawBubble();
			}
			worldGameObject.TeleportToGDPoint(in_gd_point_tag.value, dont_move_camera_while_tp);
			flow_out.Call(f);
		});
	}
}
