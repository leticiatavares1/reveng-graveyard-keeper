using System;
using System.Collections.Generic;
using UnityEngine;

public class VerticalFog : MonoBehaviour
{
	public bool isGlobalController;

	private static VerticalFog globalInstance = null;

	[NonSerialized]
	public static List<VerticalFog> secondaryFogs = new List<VerticalFog>();

	[Space(20f)]
	public bool fogEnabled;

	public bool priorityFog;

	[Space]
	public bool colorIsGradient;

	[SerializeField]
	private Color color;

	public Gradient colorGradient = new Gradient();

	[Range(-10f, 30f)]
	public float level;

	[Range(0f, 20f)]
	public float height;

	[Range(0f, 20f)]
	public float power = 1f;

	[Range(0f, 1f)]
	public float intensity;

	[Space(10f)]
	[Range(0f, 1f)]
	public float ambientInfluence = 0.5f;

	public bool additionalColorIsGradient;

	public Color additionalFogColor = Color.black;

	public Gradient additionalColorGradient = new Gradient();

	[Space(10f)]
	[Range(0f, 3f)]
	public float scale = 1f;

	[Range(0f, 3f)]
	public float xscale = 1f;

	[Range(0f, 1f)]
	public float noiseIntensity = 1f;

	[Range(-1f, 1f)]
	public float noiseAnimSpeed = 1f;

	private float fogPosition;

	private static readonly int PropVertFog = Shader.PropertyToID("_VertFog");

	private static readonly int PropFogColor = Shader.PropertyToID("_FogColor");

	private static readonly int PropFogLevel = Shader.PropertyToID("_FogLevel");

	private static readonly int PropFogHeight = Shader.PropertyToID("_FogHeight");

	private static readonly int PropFogPower = Shader.PropertyToID("_FogPower");

	private static readonly int PropFogIntensity = Shader.PropertyToID("_FogIntensity");

	private static readonly int PropFogScale = Shader.PropertyToID("_FogScale");

	private static readonly int PropFogXScale = Shader.PropertyToID("_FogXScale");

	private static readonly int PropFogPosition = Shader.PropertyToID("_FogPosition");

	private static readonly int PropNoiseIntensity = Shader.PropertyToID("_FogNoiseIntensity");

	private static readonly int PropNoiseAnimSpeed = Shader.PropertyToID("_FogNoiseAnimSpeed");

	private static readonly int PropFogAmbientIntensity = Shader.PropertyToID("_FogAmbientIntensity");

	private static readonly int PropFogAdditionalLight = Shader.PropertyToID("_FogAdditionalLight");

	private WeatherComponent weatherComponent;

	public static VerticalFog GlobalInstance => globalInstance;

	public Color Color
	{
		get
		{
			if (!colorIsGradient)
			{
				return color;
			}
			return colorGradient.Evaluate(EnvironmentEngine.Instance.timeOfDay);
		}
	}

	public Color AdditionalColor
	{
		get
		{
			if (!additionalColorIsGradient)
			{
				return additionalFogColor;
			}
			return additionalColorGradient.Evaluate(EnvironmentEngine.Instance.timeOfDay);
		}
	}

	public WeatherComponent WeatherComponent
	{
		get
		{
			if (!weatherComponent)
			{
				weatherComponent = GetComponent<WeatherComponent>();
			}
			return weatherComponent;
		}
	}

	private void Awake()
	{
		if (isGlobalController && globalInstance == null)
		{
			globalInstance = this;
		}
	}

	private void OnEnable()
	{
		Debug.Log("#dbg# Enabling " + base.name, this);
		if (!isGlobalController)
		{
			secondaryFogs.AddIfNotContains(this);
			Debug.Log("#dbg# Added " + base.name + " to GlobalInstance.secondaryFogs", this);
		}
	}

	private void OnDisable()
	{
		if (!isGlobalController)
		{
			secondaryFogs.Remove(this);
		}
	}

	private void OnDestroy()
	{
		Debug.Log("#dbg# Destroying " + base.name, this);
	}

	public void ApplyFogParameters()
	{
		ApplyFogParametersWithIntensity(1f);
	}

	public void ApplyFogParametersWithIntensity(float presetIntensity)
	{
		if (isGlobalController)
		{
			if (isGlobalController && colorIsGradient)
			{
				Debug.LogError("Global vertical fog can't use a color gradient!");
			}
			Shader.SetGlobalFloat(PropVertFog, fogEnabled ? 1 : 0);
			float num = 0f;
			if (!fogEnabled)
			{
				Shader.DisableKeyword("VERT_FOG");
			}
			else
			{
				Shader.EnableKeyword("VERT_FOG");
				fogPosition += Time.deltaTime * noiseAnimSpeed;
				Shader.SetGlobalColor(PropFogColor, Color);
				Shader.SetGlobalFloat(PropFogLevel, level);
				Shader.SetGlobalFloat(PropFogHeight, height);
				num = intensity * presetIntensity * VerticalFogGlobalK.GlobalFogCoefficient;
				Shader.SetGlobalFloat(PropFogIntensity, num);
				Shader.SetGlobalFloat(PropFogPower, power);
				Shader.SetGlobalFloat(PropFogScale, scale);
				Shader.SetGlobalFloat(PropFogXScale, xscale);
				Shader.SetGlobalFloat(PropNoiseIntensity, noiseIntensity);
				Shader.SetGlobalFloat(PropFogPosition, fogPosition);
				Shader.SetGlobalFloat(PropFogAmbientIntensity, ambientInfluence);
				Shader.SetGlobalColor(PropFogAdditionalLight, additionalFogColor);
			}
			HBAOColorManager.UpdateColorFromFog(this, num);
		}
	}

	public void GlobalFogUpdate()
	{
		if (!isGlobalController)
		{
			Debug.LogError("Calling GlobalFogUpdate() for a non-global controller.");
			return;
		}
		fogEnabled = false;
		Color color = new Color(0f, 0f, 0f, 0f);
		Color color2 = new Color(0f, 0f, 0f, 0f);
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 0f;
		float num8 = 0f;
		float num9 = 0f;
		float num10 = 0f;
		int num11 = 0;
		VerticalFog verticalFog = null;
		foreach (VerticalFog secondaryFog in secondaryFogs)
		{
			if (secondaryFog.priorityFog && secondaryFog.fogEnabled && secondaryFog.intensity > 0f)
			{
				verticalFog = secondaryFog;
				break;
			}
		}
		VerticalFog verticalFog2 = null;
		VerticalFog verticalFog3 = null;
		foreach (VerticalFog secondaryFog2 in secondaryFogs)
		{
			if (secondaryFog2.fogEnabled && (bool)secondaryFog2.WeatherComponent)
			{
				if (secondaryFog2.WeatherComponent.Animating == WeatherComponent.AnimationType.FadeIn)
				{
					verticalFog2 = secondaryFog2;
				}
				else if (secondaryFog2.WeatherComponent.Animating == WeatherComponent.AnimationType.FadeOut)
				{
					verticalFog3 = secondaryFog2;
				}
			}
		}
		if ((bool)verticalFog2 && (bool)verticalFog3 && verticalFog == null)
		{
			fogEnabled = true;
			float t = verticalFog2.WeatherComponent.intensity;
			this.color = Color.Lerp(verticalFog3.Color, verticalFog2.Color, t);
			level = Mathf.Lerp(verticalFog3.level, verticalFog2.level, t);
			height = Mathf.Lerp(verticalFog3.height, verticalFog2.height, t);
			power = Mathf.Lerp(verticalFog3.power, verticalFog2.power, t);
			scale = Mathf.Lerp(verticalFog3.scale, verticalFog2.scale, t);
			xscale = Mathf.Lerp(verticalFog3.xscale, verticalFog2.xscale, t);
			noiseIntensity = Mathf.Lerp(verticalFog3.noiseIntensity, verticalFog2.noiseIntensity, t);
			noiseAnimSpeed = Mathf.Lerp(verticalFog3.noiseAnimSpeed, verticalFog2.noiseAnimSpeed, t);
			ambientInfluence = Mathf.Lerp(verticalFog3.ambientInfluence, verticalFog2.ambientInfluence, t);
			additionalFogColor = Color.Lerp(verticalFog3.AdditionalColor, verticalFog2.AdditionalColor, t);
			intensity = Mathf.Lerp(verticalFog3.intensity, verticalFog2.intensity, t);
			ApplyFogParameters();
			return;
		}
		foreach (VerticalFog secondaryFog3 in secondaryFogs)
		{
			if (!(verticalFog != null) || secondaryFog3.priorityFog)
			{
				float num12 = secondaryFog3.intensity;
				if ((bool)secondaryFog3.WeatherComponent)
				{
					num12 *= secondaryFog3.WeatherComponent.intensity;
				}
				if (secondaryFog3.fogEnabled && secondaryFog3.gameObject.activeSelf && num12 != 0f)
				{
					fogEnabled = true;
					num9 += num12;
					num10 += num12 * num12;
					num11++;
					color += secondaryFog3.Color * num12;
					num += secondaryFog3.level * num12;
					num2 += secondaryFog3.height * num12;
					num3 += secondaryFog3.power * num12;
					num4 += secondaryFog3.scale * num12;
					num5 += secondaryFog3.xscale * num12;
					num6 += secondaryFog3.noiseIntensity * num12;
					num7 += secondaryFog3.noiseAnimSpeed * num12;
					num8 += secondaryFog3.ambientInfluence * num12;
					color2 += secondaryFog3.AdditionalColor * num12;
				}
			}
		}
		if (fogEnabled && num9 > 0f)
		{
			this.color = color / num9;
			level = num / num9;
			height = num2 / num9;
			intensity = Mathf.Min(1f, num9);
			power = num3 / num9;
			scale = num4 / num9;
			xscale = num5 / num9;
			noiseIntensity = num6 / num9;
			noiseAnimSpeed = num7 / num9;
			additionalFogColor = color2 / num9;
			ambientInfluence = num8 / num9;
		}
		ApplyFogParameters();
	}
}
