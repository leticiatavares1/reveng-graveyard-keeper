using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Add money to vendor", 0)]
[Category("Game/UI")]
public class Flow_AddMoneyToVendor : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> vendorIdInput;

	private ValueInput<int> moneyInput;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Show);
		@out = AddFlowOutput("out".CapitalizeFirst());
		vendorIdInput = AddValueInput<string>("vendorIdInput");
		moneyInput = AddValueInput<int>("moneyInput");
	}

	private void Show(Flow flow)
	{
		if (!string.IsNullOrEmpty(vendorIdInput.value))
		{
			MainGame.Instance.GameSave.vendorSystem.AddMoneyToVendor(vendorIdInput.value, moneyInput.value);
		}
		else
		{
			Debug.LogError("Flow_AddMoneyToVendor: vendor id is empty.");
		}
		@out.Call(flow);
	}
}
