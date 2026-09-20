using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Camera Fly", 0)]
[Category("Game Actions")]
public class Flow_CameraFly : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetInputValuePort<GameObject>("Target GO").isConnected)
			{
				return "Camera Fly (back)";
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
		ValueInput<bool> par_move_cam_inst = AddValueInput<bool>("Move instantly?");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_finished = AddFlowOutput("On Finished");
		AddFlowInput("In", delegate(Flow f)
		{
			float duration = (par_move_cam_inst.value ? 0f : 0.7f);
			if (!par_wgo.HasValue())
			{
				Debug.Log("Call CameraFlyBack");
				CameraTools.CameraFlyBack(delegate
				{
					flow_finished.Call(f);
				}, duration);
			}
			else
			{
				Debug.Log("Call CameraFlyTo \"" + par_wgo.value.name + "\"");
				CameraTools.CameraFlyTo(par_wgo.value.transform, delegate
				{
					flow_finished.Call(f);
				}, duration);
			}
			flow_out.Call(f);
		});
	}
}
