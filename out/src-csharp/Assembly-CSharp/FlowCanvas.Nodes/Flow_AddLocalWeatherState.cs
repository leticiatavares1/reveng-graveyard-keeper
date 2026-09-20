using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Add Local Weather State", 0)]
[Category("Game Actions")]
[Description("Add Local Weather State")]
public class Flow_AddLocalWeatherState : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_preset_name = AddValueInput<string>("preset name");
		ValueInput<float> in_start_time = AddValueInput<float>("start time (in days)");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WeatherPreset preset = WeatherPreset.GetPreset(in_preset_name.value);
			if (preset == null)
			{
				Debug.LogError("Weather preset is null!");
			}
			else
			{
				foreach (SwitchableWeatherState item in SwitchableWeatherState.GetStatesFromPreset((Mathf.Abs(in_start_time.value) < 0.0001f) ? MainGame.game_time : in_start_time.value, preset))
				{
					EnvironmentEngine.me.AddLocalWeatherState(item);
				}
				flow_out.Call(f);
			}
		});
	}
}
