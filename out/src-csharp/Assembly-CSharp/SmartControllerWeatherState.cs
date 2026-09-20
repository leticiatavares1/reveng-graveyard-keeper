public class SmartControllerWeatherState : WeatherState
{
	public SmartController controller;

	protected override void WeatherAmountDelegate(float a)
	{
		controller.value = a;
		controller.Update();
	}
}
