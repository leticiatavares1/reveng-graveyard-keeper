using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Show Vendor Window", 0)]
[Category("Game/UI")]
public class Flow_ShowVendorWindow : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onClosed;

	private ValueInput<string> vendorIdInput;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Show);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onClosed = AddFlowOutput("onClosed".CapitalizeFirst());
		vendorIdInput = AddValueInput<string>("vendorIdInput");
	}

	private void Show(Flow flow)
	{
		if (!string.IsNullOrEmpty(vendorIdInput.value))
		{
			Trading trading = new Trading();
			UIVendorWindowData uIVendorWindowData = new UIVendorWindowData();
			trading.FillVendorWindowData(uIVendorWindowData, vendorIdInput.value, delegate
			{
				onClosed.Call(flow);
			});
			LazyUI.GetWindow<UIVendorWindow>().Open(uIVendorWindowData);
		}
		else
		{
			Debug.LogError("Flow_ShowVendorWindow: vendor id is empty.");
		}
		@out.Call(flow);
	}
}
