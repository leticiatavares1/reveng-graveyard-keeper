using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Set Weather Global State", 0)]
[Category("Game Actions")]
public class Flow_SetWeatherGlobalState : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<EnvironmentEngine.State> state = AddValueInput<EnvironmentEngine.State>("state");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			EnvironmentEngine.me.SetEngineGlobalState(state.value);
			flow_out.Call(f);
		});
	}
}
