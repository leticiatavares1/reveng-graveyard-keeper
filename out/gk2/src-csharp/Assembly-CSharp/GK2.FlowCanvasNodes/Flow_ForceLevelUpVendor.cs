using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Force level up vendor", 0)]
[Category("Game/UI")]
public class Flow_ForceLevelUpVendor : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> vendorIdInput;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Show);
		@out = AddFlowOutput("out".CapitalizeFirst());
		vendorIdInput = AddValueInput<string>("vendorIdInput");
	}

	private void Show(Flow flow)
	{
		if (!string.IsNullOrEmpty(vendorIdInput.value))
		{
			MainGame.Instance.GameSave.vendorSystem.ForceLevelUpVendor(vendorIdInput.value);
		}
		else
		{
			Debug.LogError("Flow_ForceLevelUpVendor: vendor id is empty.");
		}
		@out.Call(flow);
	}
}
