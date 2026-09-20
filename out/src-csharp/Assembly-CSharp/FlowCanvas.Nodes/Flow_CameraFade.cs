using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Camera Fade", 0)]
[Category("Game Actions")]
[Description("If WGO is null, then self")]
public class Flow_CameraFade : MyFlowNode
{
	private enum FadeType
	{
		InOut,
		In,
		Out
	}

	public override string name
	{
		get
		{
			FadeType value = GetInputValuePort<FadeType>("Fade type").value;
			return value switch
			{
				FadeType.In => $"<color=#0D3C1A>Camera Fade {value}</color>", 
				FadeType.Out => $"<color=#700B25>Camera Fade {value}</color>", 
				_ => base.name, 
			};
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<float> par_time = AddValueInput<float>("Time");
		ValueInput<bool> par_immediate = AddValueInput<bool>("immediate");
		ValueInput<Color> par_color = AddValueInput<Color>("Color fade");
		ValueInput<FadeType> par_fade_type = AddValueInput<FadeType>("Fade type");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_middle = AddFlowOutput("Middle");
		FlowOutput flow_finished = AddFlowOutput("On Finished");
		AddFlowInput("In", delegate(Flow f)
		{
			float time = par_time.value;
			GUIElements.ChangeBubblesVisibility(show: false);
			float num = (par_immediate.value ? 0f : time);
			Color fade_color = (par_color.isDefaultValue ? Color.black : par_color.value);
			switch (par_fade_type.value)
			{
			case FadeType.InOut:
				CameraTools.Fade(delegate
				{
					flow_middle.Call(f);
					CameraTools.UnFade(delegate
					{
						GUIElements.ChangeBubblesVisibility(MainGame.me.player_char.control_enabled);
						flow_finished.Call(f);
					}, time, fade_color);
				}, num, fade_color);
				break;
			case FadeType.In:
				CameraTools.Fade(delegate
				{
					flow_finished.Call(f);
				}, num, fade_color);
				GJTimer.AddTimer(num / 2f, delegate
				{
					flow_middle.Call(f);
				});
				break;
			case FadeType.Out:
				CameraTools.UnFade(delegate
				{
					GUIElements.ChangeBubblesVisibility(MainGame.me.player_char.control_enabled);
					flow_finished.Call(f);
				}, num, fade_color);
				GJTimer.AddTimer(num / 2f, delegate
				{
					flow_middle.Call(f);
				});
				break;
			}
			flow_out.Call(f);
		});
	}
}
