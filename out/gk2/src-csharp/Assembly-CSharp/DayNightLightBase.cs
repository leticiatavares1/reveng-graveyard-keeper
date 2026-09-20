using UnityEngine;

public abstract class DayNightLightBase : MonoBehaviour
{
	public enum LightMode
	{
		Night,
		Day,
		Static
	}

	protected static readonly int idLightMode = Shader.PropertyToID("_LightMode");

	protected static readonly string idSunLightDependencyKeyword = "USE_SUN_LIGHT_DEPENDENCY";

	public LightMode mode;

	protected abstract void ApplyLightMode(LightMode mode);

	public void ApplyLightModeInt(int mode)
	{
		ApplyLightMode((LightMode)mode);
	}
}
