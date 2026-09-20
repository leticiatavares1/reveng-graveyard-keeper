using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Has WGO Data In World", 0)]
[Category("Game/Script")]
[Color("70f1ff")]
[Icon("FS", false, "")]
public class Flow_HasWgoDataInWorld : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput yes;

	private FlowOutput no;

	private ValueInput<string> wgoId;

	private ValueInput<string> customTag;

	private ValueOutput<string> wgoIdOut;

	private ValueOutput<string> customTagOut;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), HasWgoDataInWorld);
		yes = AddFlowOutput("yes".CapitalizeFirst());
		no = AddFlowOutput("no".CapitalizeFirst());
		wgoId = AddValueInput<string>("wgoId".CapitalizeFirst());
		customTag = AddValueInput<string>("customTag");
		wgoIdOut = AddValueOutput("wgoId", () => wgoId.value);
		customTagOut = AddValueOutput("customTag", () => customTag.value);
	}

	private void HasWgoDataInWorld(Flow flow)
	{
		WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(wgoId.value);
		if (wgoData == null)
		{
			wgoData = MainGame.Instance.GameSave.worldData.GetWgoDataByCustomTag(customTag.value);
		}
		if (wgoData != null)
		{
			yes.Call(flow);
		}
		else
		{
			no.Call(flow);
		}
	}
}
