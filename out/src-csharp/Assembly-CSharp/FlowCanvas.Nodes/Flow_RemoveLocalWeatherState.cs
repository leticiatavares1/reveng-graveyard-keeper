using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Remove Local Weather State", 0)]
[Category("Game Actions")]
[Description("Remove Local Weather State")]
public class Flow_RemoveLocalWeatherState : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_preset_name = AddValueInput<string>("preset name");
		ValueInput<float> in_start_time = AddValueInput<float>("start time (in days)");
		ValueInput<float> in_dec_time_in_seconds = AddValueInput<float>("dec time (sec)");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (WeatherPreset.GetPreset(in_preset_name.value) == null)
			{
				Debug.LogError("Weather preset is null!");
			}
			else
			{
				float start_removing_time = ((Mathf.Abs(in_start_time.value) < 0.0001f) ? MainGame.game_time : in_start_time.value);
				float num = TimeOfDay.FromSecondsToTimeK(in_dec_time_in_seconds.value);
				if (num < 0.01f)
				{
					num = 0.1f;
				}
				EnvironmentEngine.me.TryRemoveLocalWeatherState(in_preset_name.value, start_removing_time, num);
				flow_out.Call(f);
			}
		});
	}
}
