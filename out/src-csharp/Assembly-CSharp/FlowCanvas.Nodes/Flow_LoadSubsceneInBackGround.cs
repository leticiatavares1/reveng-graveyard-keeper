using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Load scene additive to the current scene in the background")]
[Icon("CubePlus", false, "")]
[Category("Game Actions")]
[Name("Just Load Subscene", 0)]
public class Flow_LoadSubsceneInBackGround : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> par_custom_tag = AddValueInput<string>("Scene name");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_out_on_scene_loaded = AddFlowOutput("On loaded");
		AddFlowInput("In", delegate(Flow f)
		{
			SubsceneLoadManager.Load(par_custom_tag.value, delegate
			{
				flow_out_on_scene_loaded.Call(f);
			});
			flow_out.Call(f);
		});
	}
}
