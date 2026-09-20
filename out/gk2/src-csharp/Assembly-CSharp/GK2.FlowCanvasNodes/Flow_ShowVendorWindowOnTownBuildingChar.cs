using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Show Vendor Window On Town Building Char", 0)]
[Category("Game/UI")]
public class Flow_ShowVendorWindowOnTownBuildingChar : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onClosed;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Show);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onClosed = AddFlowOutput("onClosed".CapitalizeFirst());
	}

	private void Show(Flow flow)
	{
		Trading trading = new Trading();
		UIVendorWindowData uIVendorWindowData = new UIVendorWindowData();
		trading.FillVendorWindowData(uIVendorWindowData, MainGame.WorldData.GetWgoData(base.SelfWgoData.LinkedToTownBuildingUniqueId).TownBuildingWgoComponent.TownBuildingDef.vendorId, delegate
		{
			onClosed.Call(flow);
		});
		LazyUI.GetWindow<UIVendorWindow>().Open(uIVendorWindowData);
		@out.Call(flow);
	}
}
