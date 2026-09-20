using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Talk Wisp", 0)]
[Category("Game/Dialogue")]
[Color("40addb")]
[Icon("Dialogue", false, "")]
public class Flow_TalkWisp : Flow_Talk
{
	private ValueInput<WispController> wispInput;

	public override string name => "Talk Wisp";

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		wispInput = AddValueInput<WispController>("wispInput".CapitalizeFirst());
		wgoDataInput = null;
	}

	protected override void DoTalk(Flow flow)
	{
		WispController wispController = wispInput.value;
		if (wispController == null)
		{
			wispController = MainGame.PlayerController.WispController;
		}
		string text = base.text.value;
		PhraseData data = default(PhraseData);
		data.text = text;
		data.isPlayer = false;
		data.npcWgoData = wispController.GetWispWgoData();
		data.onFinished = delegate
		{
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.SpeechSaid, text);
			flowOnFinished.Call(flow);
		};
		data.cornerPosition = forceCornerPosition;
		data.isOverBlackout = isOverBlackout;
		data.speechType = speechType.value;
		Bubble.Talk(data);
		flowOutput.Call(flow);
	}
}
