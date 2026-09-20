using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Show Folio Window", 0)]
[Category("Game/UI")]
public class Flow_OpenFolioWindow : GKCustomFlowNode
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
		UIAlchemyFolioWindowData uIAlchemyFolioWindowData = new UIAlchemyFolioWindowData();
		uIAlchemyFolioWindowData.FillFromGaveSave(base.SelfWgoData);
		LazyUI.GetWindow<UIAlchemyFolioWindow>().Open(uIAlchemyFolioWindowData);
		@out.Call(flow);
	}
}
