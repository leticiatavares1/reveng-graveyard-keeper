using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

[Name("Enable Lazy Animation Controller", 0)]
[Category("Game/Animation")]
public class Flow_EnableLazyAnimationController : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool isEnable;

	private FlowInput @in;

	private FlowOutput @out;

	public override string name => (isEnable ? "Enable" : "Disable") + " LazyAnimationController";

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), Toggle);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void Toggle(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		if (wgoData != null)
		{
			LazyAnimationController componentInChildren = GameScene.GetWgoViewGlobal(wgoData.UniqueId).gameObject.GetComponentInChildren<LazyAnimationController>(includeInactive: true);
			if (componentInChildren != null)
			{
				componentInChildren.enabled = isEnable;
			}
			else
			{
				Debug.LogError("Tried to toggle LazyAnimationController: not found component");
			}
		}
		@out.Call(flow);
	}
}
