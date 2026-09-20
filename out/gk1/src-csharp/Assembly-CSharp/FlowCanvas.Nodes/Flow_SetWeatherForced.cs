using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Set Weather Forced", 0)]
[Color("000000")]
[Description("Set Weather Now. Use only when time is disabled")]
public class Flow_SetWeatherForced : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<float> in_rain = AddValueInput<float>("Rain");
		ValueInput<float> in_wind = AddValueInput<float>("Wind");
		ValueInput<float> in_fog = AddValueInput<float>("Fog");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			Debug.Log("Flow_SetWeatherForced");
			SmartWeatherState[] states = EnvironmentEngine.me.states;
			foreach (SmartWeatherState smartWeatherState in states)
			{
				if (!(smartWeatherState == null))
				{
					switch (smartWeatherState.type)
					{
					case SmartWeatherState.WeatherType.Rain:
						smartWeatherState.forced_value = in_rain.value;
						break;
					case SmartWeatherState.WeatherType.Fog:
						smartWeatherState.forced_value = in_fog.value;
						break;
					case SmartWeatherState.WeatherType.Wind:
						smartWeatherState.forced_value = in_wind.value;
						break;
					case SmartWeatherState.WeatherType.LUT:
						smartWeatherState.SetValueImmediate(0f);
						Debug.LogError("WHAT??? LUT??? Call Bulat!");
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
			}
			flow_out.Call(f);
		});
	}
}
