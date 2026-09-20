public class RainControllerComponent : AbstractControllerComponent
{
	public CameraFilterPack_Atmosphere_Rain_Pro rain;

	public override void Set(float a)
	{
		rain.enabled = a > 0f;
		rain.Fade = a;
	}
}
