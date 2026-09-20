using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Camera Cinematic Fly", 0)]
[Category("Game Actions")]
[Description("Duration 0 -- default")]
public class Flow_CameraCinematicFly : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetInputValuePort<GameObject>("Target GO").isConnected)
			{
				return "Camera Cinematic Fly (back)";
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
		ValueInput<GameObject> par_wgo = AddValueInput<GameObject>("Target GO");
		ValueInput<float> par_duration = AddValueInput<float>("Duration");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_finished = AddFlowOutput("On Finished");
		float set_duration = 0.7f;
		AddFlowInput("In", delegate(Flow f)
		{
			if (!Mathf.Approximately(par_duration.value, 0f))
			{
				set_duration = par_duration.value;
			}
			if (!par_wgo.HasValue())
			{
				Debug.Log("Call CameraFlyBack");
				CameraTools.CameraFlyBack(delegate
				{
					flow_finished.Call(f);
				});
			}
			else
			{
				Debug.Log("Call CameraCinematicFly \"" + par_wgo.value.name + "\"");
				CameraTools.CameraCinematicFly(par_wgo.value.transform, delegate
				{
					flow_finished.Call(f);
				}, set_duration);
			}
			flow_out.Call(f);
		});
	}
}
