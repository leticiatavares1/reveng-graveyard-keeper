using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set Post Process Profile", 0)]
[Category("Game/Camera")]
[Color("313c8f")]
public class Flow_SetPostProcessProfile : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool reset;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> profileName;

	public override string name => (reset ? "Reset" : "Set") + " Post Process Profile";

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ChangeProfile);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (!reset)
		{
			profileName = AddValueInput<string>("profileName");
		}
	}

	private void ChangeProfile(Flow flow)
	{
		if (!reset)
		{
			if (!string.IsNullOrEmpty(profileName.value))
			{
				CameraSystem.Instance.MainCamera.SetPostProcessProfile(profileName.value);
			}
		}
		else
		{
			CameraSystem.Instance.MainCamera.ResetPostProcessProfile();
		}
		@out.Call(flow);
	}
}
