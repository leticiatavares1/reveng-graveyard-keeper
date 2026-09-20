using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("If WGO is null, then self")]
[Category("Game Actions")]
[Name("Camera Cinematics", 0)]
public class Flow_CameraCinematics : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<float> par_duration = AddValueInput<float>("Hold duration");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_finished = AddFlowOutput("On Finished");
		AddFlowInput("In", delegate(Flow f)
		{
			CameraTools.PlayCinematics(WGOParamOrSelf(par_wgo), 1f, par_duration.value, delegate
			{
				flow_finished.Call(f);
			});
			flow_out.Call(f);
		});
	}
}
