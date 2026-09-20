using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Run script", 0)]
public class Flow_RunScript : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_script_name = AddValueInput<string>("Script name");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_on_finished = AddFlowOutput("On finished");
		AddFlowInput("In", delegate(Flow f)
		{
			string value = in_script_name.value;
			if (!string.IsNullOrEmpty(value))
			{
				if (value[0] == ':')
				{
					FlowScriptEngine.SendEvent(value.Substring(1));
				}
				else
				{
					GS.RunFlowScript(value, delegate
					{
						flow_on_finished.Call(f);
					});
				}
			}
			flow_out.Call(f);
		});
	}
}
