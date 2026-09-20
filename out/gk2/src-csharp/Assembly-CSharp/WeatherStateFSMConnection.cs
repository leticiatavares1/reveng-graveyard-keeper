using NodeCanvas.StateMachines;

public class WeatherStateFSMConnection : FSMConnection
{
	public override void OnDestroy()
	{
		foreach (FSMWeatherState.WeatherStateExit exit in (base.sourceNode as FSMWeatherState).exits)
		{
			if (exit.connection == this)
			{
				exit.connection = null;
			}
		}
		base.OnDestroy();
	}
}
