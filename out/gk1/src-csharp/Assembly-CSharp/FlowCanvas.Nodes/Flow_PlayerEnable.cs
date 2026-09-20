using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Player Enable", 0)]
[Color("4155be")]
public class Flow_PlayerEnable : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetInputValuePort<bool>("Enable player").value)
			{
				return "Player Disable";
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
		ValueInput<bool> par_enable = AddValueInput<bool>("Enable player");
		ValueInput<bool> par_cinematic = AddValueInput<bool>("Affect cinematic");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GS.SetPlayerEnable(par_enable.value, par_cinematic.value);
			ObjectDefinition currentInteractiveNPC = GUIElements.me.relation.GetCurrentInteractiveNPC();
			if (!par_enable.value)
			{
				if (currentInteractiveNPC != null)
				{
					SmartAudioEngine.me.OnStartNPCInteraction(currentInteractiveNPC);
				}
			}
			else
			{
				SmartAudioEngine.me.OnEndNPCInteraction();
			}
			flow_out.Call(f);
		});
	}
}
