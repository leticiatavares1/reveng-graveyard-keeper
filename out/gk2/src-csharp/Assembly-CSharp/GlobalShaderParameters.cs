using LazyBearTechnology;
using UnityEngine;

[ExecuteInEditMode]
public class GlobalShaderParameters : LazySingleton<GlobalShaderParameters>
{
	private static readonly int idBacklightColor = Shader.PropertyToID("_BacklightColor");

	private static readonly int idBacklightContrast = Shader.PropertyToID("_BacklightContrast");

	private static readonly int idLightSpread = Shader.PropertyToID("_LightSpread");

	private static readonly int idLightMaxBurn = Shader.PropertyToID("_LightMaxBurn");

	private static readonly int idLightFocus = Shader.PropertyToID("_LightFocus");

	public static readonly int idSunLight = Shader.PropertyToID("_SunLight");

	public static readonly int idTimeOfDay = Shader.PropertyToID("_TimeOfDay");

	public static readonly int idWindValue = Shader.PropertyToID("_WindValue");

	[Header("Backlight")]
	public Color backlightColor = new Color(0.2f, 0.2f, 0.6f, 0f);

	[Range(0f, 5f)]
	public float backlightContrast = 1f;

	[Space(10f)]
	[Header("Light overburn limit")]
	[Range(0f, 5f)]
	public float lightSpread = 1f;

	[Range(0.01f, 10f)]
	public float lightMaxBurn = 3f;

	[Range(0f, 5f)]
	public float lightFocus = 1f;

	[Space(10f)]
	[Header("Global params")]
	[Range(0f, 1f)]
	public float sunLight = 1f;

	private new void Awake()
	{
		ApplyShaderParameters();
	}

	public void ApplyShaderParameters()
	{
		Shader.SetGlobalColor(idBacklightColor, backlightColor);
		Shader.SetGlobalFloat(idBacklightContrast, backlightContrast);
		Shader.SetGlobalFloat(idLightSpread, lightSpread);
		Shader.SetGlobalFloat(idLightMaxBurn, lightMaxBurn);
		Shader.SetGlobalFloat(idLightFocus, lightFocus);
		Shader.SetGlobalFloat(idSunLight, sunLight);
		Shader.SetGlobalFloat(idTimeOfDay, EnvironmentEngine.Instance.timeOfDay);
	}
}
