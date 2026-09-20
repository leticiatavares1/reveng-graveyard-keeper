using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Tavern Engine remove visitors for event", 0)]
public class Flow_TavernEngineRemoveVisitors : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<bool>("place back").value)
			{
				return "Place visitors back after event";
			}
			return "Remove visitors for event";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<bool> in_place_back = AddValueInput<bool>("place back");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_place_back.value)
			{
				MainGame.me.save.players_tavern_engine.PlaceVisitorsBackAfterEvent();
			}
			else
			{
				MainGame.me.save.players_tavern_engine.TemporarilyRemoveVisitors();
			}
			flow_out.Call(f);
		});
	}
}
