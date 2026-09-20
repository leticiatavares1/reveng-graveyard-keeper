using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Show Vendor Orders Window", 0)]
[Category("Game/UI")]
public class Flow_OpenVendorOrdersWindow : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Show);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void Show(Flow flow)
	{
		if (!MainGame.PlayerData.interactedWithChalkBoardOnce && MainGame.PlayerData.GetRes("commerce_tutorial_available") > 0f)
		{
			MainGame.PlayerData.interactedWithChalkBoardOnce = true;
			UITutorialWindowData data = new UITutorialWindowData("tut_commerce_hdr");
			LazyUI.GetWindow<UITutorialWindow>().Open(data, delegate
			{
				MainGame.PlayerData.SetRes("chalk_board_enabled", 1f);
				LazyUI.GetWindow<UIVendorOrdersWindow>().Open(new UIVendorOrdersWindowData());
			});
		}
		else
		{
			LazyUI.GetWindow<UIVendorOrdersWindow>().Open(new UIVendorOrdersWindowData());
		}
		@out.Call(flow);
	}
}
