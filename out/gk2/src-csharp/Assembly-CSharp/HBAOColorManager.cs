using HorizonBasedAmbientOcclusion;
using UnityEngine;

[ExecuteInEditMode]
[DefaultExecutionOrder(-1)]
public class HBAOColorManager : MonoBehaviour
{
	private static HBAO hbao;

	public Color color;

	public Gradient fogAdditionalColor = new Gradient();

	private static Color defaultColor = Color.white;

	private static Gradient defaultFogAdditionalColor;

	private static Color currentColor;

	private static Color currentAdditionalColor;

	private static HBAO Hbao
	{
		get
		{
			if (!hbao)
			{
				hbao = CameraSystem.Instance?.MainCamera?.GetComponent<HBAO>();
			}
			return hbao;
		}
	}

	private void OnEnable()
	{
		PlatformFeatures.ApplyHBAO();
		if (Hbao != null)
		{
			Debug.Log("HBAO ColorManager initialized, enabled = " + Hbao.enabled);
		}
		UpdateColors();
	}

	public static void UpdateColorFromFog(VerticalFog fog, float totalFogIntensity)
	{
		if (GameSettings.IsHBAOEnabled() && (bool)Hbao && Application.isPlaying)
		{
			currentAdditionalColor = defaultFogAdditionalColor.Evaluate(EnvironmentEngine.Instance.timeOfDay);
			currentColor = Color.Lerp(defaultColor, fog.Color + currentAdditionalColor, totalFogIntensity * (float)fog.fogEnabled.ToInt());
			Hbao.SetAoColor(currentColor);
		}
	}

	private void UpdateColors()
	{
		if (Application.isPlaying)
		{
			defaultColor = color;
			defaultFogAdditionalColor = fogAdditionalColor;
		}
	}
}
