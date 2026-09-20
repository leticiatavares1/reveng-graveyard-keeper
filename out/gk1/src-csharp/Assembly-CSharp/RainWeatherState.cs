public class RainWeatherState : WeatherState
{
	public CameraFilterPack_Atmosphere_Rain_Pro rain;

	protected override void WeatherAmountDelegate(float a)
	{
		rain.enabled = a > 0f;
		rain.Fade = a;
	}
}
