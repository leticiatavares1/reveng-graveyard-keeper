using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Add Nature Weather State")]
[Category("Game Actions")]
[Name("Add Nature Weather State", 0)]
public class Flow_AddNatureWeatherState : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_preset_name = AddValueInput<string>("preset name");
		ValueInput<float> in_start_time = AddValueInput<float>("start time (in days)");
		ValueInput<float> in_remove_time = AddValueInput<float>("remove time (in days)");
		ValueInput<float> in_dec_time = AddValueInput<float>("dec time (sec)");
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
				float num = ((Mathf.Abs(in_start_time.value) < 0.0001f) ? MainGame.game_time : in_start_time.value);
				if (in_remove_time.value < num)
				{
					Debug.LogError("Wrong remove time for preset " + preset.preset_name + ": {" + num + ", " + in_remove_time.value + "}");
				}
				else
				{
					float num2 = TimeOfDay.FromSecondsToTimeK(in_dec_time.value);
					if (num2 < 0.0005f)
					{
						Debug.LogError("Wrong dec_time for preset " + preset.preset_name + ": " + in_dec_time.value + "}");
					}
					else
					{
						foreach (SwitchableWeatherState item in SwitchableWeatherState.GetStatesFromPreset(num, preset))
						{
							EnvironmentEngine.me.AddNatureWeatherState(item);
							EnvironmentEngine.me.TryRemoveNatureWeatherState(preset.preset_name, in_remove_time.value, num2);
						}
						flow_out.Call(f);
					}
				}
			}
		});
	}
}
