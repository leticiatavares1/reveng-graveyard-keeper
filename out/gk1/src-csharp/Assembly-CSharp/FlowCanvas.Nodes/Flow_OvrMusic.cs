using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Ovr Music", 0)]
public class Flow_OvrMusic : MyFlowNode
{
	public override string name
	{
		get
		{
			if (string.IsNullOrEmpty(GetInputValuePort<string>("music id").value))
			{
				return base.name + " [stop any]";
			}
			return base.name + " [" + (GetInputValuePort<bool>("play?").value ? "play" : "stop") + "]";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<string> in_music = AddValueInput<string>("music id");
		ValueInput<bool> in_play = AddValueInput<bool>("play?");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_play.value)
			{
				SmartAudioEngine.me.PlayOvrMusic(in_music.value);
			}
			else
			{
				SmartAudioEngine.me.StopOvrMusic(in_music.value);
			}
			flow_out.Call(f);
		});
	}
}
