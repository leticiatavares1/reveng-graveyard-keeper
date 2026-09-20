using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Talk", 0)]
[Category("Game/Dialogue")]
[Color("40addb")]
[Icon("Dialogue", false, "")]
public class Flow_Talk : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool isPlayer;

	public UIBasicBubble.ForceCornerPosition forceCornerPosition;

	[GatherPortsCallback]
	public bool fixedShowTime;

	[ShowIf("fixedShowTime", 1)]
	public float fixedShowTimeValue = -1f;

	[GatherPortsCallback]
	public bool isOverBlackout;

	protected FlowInput flowInput;

	protected FlowOutput flowOutput;

	protected FlowOutput flowOnFinished;

	protected ValueOutput<WgoData> wgoOutput;

	protected ValueInput<SpeechBubbleType> speechType;

	protected ValueInput<string> text;

	public override string name => string.Format("{0} {1}", speechType.value, isPlayer ? "Player" : "NPC");

	public override Alignment2x2 iconAlignment => Alignment2x2.Left;

	protected override void RegisterPorts()
	{
		if (!isPlayer)
		{
			base.RegisterPorts();
		}
		flowInput = AddFlowInput("In", DoTalk);
		flowOutput = AddFlowOutput("Out");
		flowOnFinished = AddFlowOutput("On Finished");
		speechType = AddValueInput<SpeechBubbleType>("Speech type");
		speechType.SetDefaultAndSerializedValue(SpeechBubbleType.Talk);
		text = AddValueInput<string>("text");
	}

	protected virtual void DoTalk(Flow flow)
	{
		if (!fixedShowTime)
		{
			fixedShowTimeValue = 0f;
		}
		string text = this.text.value;
		PhraseData data = default(PhraseData);
		data.text = text;
		data.isPlayer = isPlayer;
		data.npcWgoData = (isPlayer ? null : GetWgoData());
		data.onFinished = delegate
		{
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.SpeechSaid, text);
			flowOnFinished.Call(flow);
		};
		data.cornerPosition = forceCornerPosition;
		data.fixedShowTimeValue = fixedShowTimeValue;
		data.isOverBlackout = isOverBlackout;
		data.speechType = speechType.value;
		Bubble.Talk(data);
		flowOutput.Call(flow);
	}
}
