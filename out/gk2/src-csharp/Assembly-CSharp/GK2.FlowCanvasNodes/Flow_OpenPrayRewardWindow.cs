using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Open Pray Reward Window", 0)]
[Category("Game/UI")]
public class Flow_OpenPrayRewardWindow : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onClosed;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), delegate(Flow flow)
		{
			GetWgoData();
			UIPrayReportWindowData data = new UIPrayReportWindowData(MainGame.PlayerData.currentSermon, delegate
			{
				onClosed.Call(flow);
			});
			LazyUI.GetWindow<UIPrayReportWindow>().Open(data);
		});
		@out = AddFlowOutput("out".CapitalizeFirst());
		onClosed = AddFlowOutput("onClosed".CapitalizeFirst());
	}
}
