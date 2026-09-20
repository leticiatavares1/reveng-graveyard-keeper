using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Add Forced Weather State")]
[Category("Game Actions")]
[Name("Add Forced Weather State", 0)]
public class Flow_AddForcedWeatherState : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<SmartWeatherState.WeatherType> in_type = AddValueInput<SmartWeatherState.WeatherType>("type");
		ValueInput<Texture2D> in_lut_texture = AddValueInput<Texture2D>("lut_texture");
		ValueInput<float> in_value = AddValueInput<float>("value");
		ValueInput<float> in_start_time = AddValueInput<float>("start_time (Time K)");
		ValueInput<float> in_t_atk_in_seconds = AddValueInput<float>("atk_time (sec)");
		ValueInput<float> in_t_flat_in_seconds = AddValueInput<float>("flat_time (sec)");
		ValueInput<float> in_t_dec_in_seconds = AddValueInput<float>("dec_time (sec)");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_value.value < 0f || in_value.value > 5f)
			{
				Debug.LogError("Forced weather :: wrong value.");
			}
			else if (in_start_time.value < 1f)
			{
				Debug.LogError("Forced weather :: wrong start time.");
			}
			else
			{
				float t_atk = TimeOfDay.FromSecondsToTimeK(in_t_atk_in_seconds.value);
				float t_flat = TimeOfDay.FromSecondsToTimeK(in_t_flat_in_seconds.value);
				float t_dec = TimeOfDay.FromSecondsToTimeK(in_t_dec_in_seconds.value);
				EnvironmentEngine.me.AddForcedWeatherState(new ForcedWeatherState(in_type.value, in_lut_texture.value, in_value.value, in_start_time.value, t_atk, t_flat, t_dec));
				flow_out.Call(f);
			}
		});
	}
}
