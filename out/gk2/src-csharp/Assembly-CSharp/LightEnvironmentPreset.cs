using UnityEngine;

[CreateAssetMenu(fileName = "LightEnvironmentPreset", menuName = "GK2/Light/LightEnvironmentPreset")]
public class LightEnvironmentPreset : ScriptableObject
{
	[Header("")]
	[Space(5f)]
	[SerializeField]
	private bool e_sunLightRotation = true;

	public Vector3 sunLightRotation;

	[SerializeField]
	private bool e_sunLightColor = true;

	public Color sunLightColor;

	[SerializeField]
	private bool e_sunLightIntensity = true;

	public float sunLightIntensity;

	[SerializeField]
	private bool e_sunLightIntensityLim;

	public float sunLightIntensityLim;

	[SerializeField]
	private bool e_sunLightShadowStrength = true;

	public float sunLightShadowStrength;

	[SerializeField]
	private bool e_sunLightShadowStrengthLim;

	public float sunLightShadowStrengthLim;

	[SerializeField]
	private bool e_sunLightAmount = true;

	public float sunLightAmount;

	[Header("")]
	[Space(15f)]
	[SerializeField]
	private bool e_backLightColor = true;

	[Space(10f)]
	public Color backLightColor;

	[SerializeField]
	private bool e_backLightIntensity = true;

	public float backLightIntensity;

	[Header("")]
	[Space(15f)]
	[SerializeField]
	private bool e_gspBacklightColor = true;

	[Space(10f)]
	public Color gspBacklightColor;

	[SerializeField]
	private bool e_gspMaxLightBurn = true;

	public float gspMaxLightBurn = 1f;

	[Header("")]
	[Space(15f)]
	[SerializeField]
	private bool e_ambientLightColor = true;

	[Space(10f)]
	[ColorUsage(true, true)]
	public Color ambientLightColor;

	[Header("")]
	[Space(15f)]
	[SerializeField]
	private bool e_lutTexture = true;

	[Space(10f)]
	private Texture lutTexture;

	[Header("")]
	[Space(15f)]
	[SerializeField]
	private bool e_additiveBloom;

	[Space(10f)]
	[Range(-1f, 1f)]
	public float additiveBloomThreshold;

	private bool lutIsLerped;

	private Texture lutTexture2;

	private float lutLerpFactor;

	private RenderTexture lutRT;

	private Material lutLerpMaterial;

	private static readonly int idMainTex = Shader.PropertyToID("_MainTex");

	private static readonly int idSecondaryTex = Shader.PropertyToID("_SecondaryTex");

	private static readonly int idLerpFactor = Shader.PropertyToID("_LerpFactor");

	public Texture LutTexture
	{
		get
		{
			if (!lutIsLerped)
			{
				return lutTexture;
			}
			return GetLerpedLut();
		}
	}

	private void ApplyPreset()
	{
	}

	public static void Lerp(LightEnvironmentPreset dest, LightEnvironmentPreset p1, LightEnvironmentPreset p2, float v)
	{
		if (!(dest == null) && !(p1 == null) && !(p2 == null))
		{
			dest.sunLightRotation = Lerp(p2.e_sunLightRotation, p1.sunLightRotation, p2.sunLightRotation, v);
			dest.sunLightColor = Lerp(p2.e_sunLightColor, p1.sunLightColor, p2.sunLightColor, v);
			dest.sunLightIntensity = Lerp(p2.e_sunLightIntensity, p1.sunLightIntensity, p2.sunLightIntensity, v);
			dest.sunLightShadowStrength = Lerp(p2.e_sunLightShadowStrength, p1.sunLightShadowStrength, p2.sunLightShadowStrength, v);
			dest.sunLightAmount = Lerp(p2.e_sunLightAmount, p1.sunLightAmount, p2.sunLightAmount, v);
			if (p2.e_sunLightIntensityLim)
			{
				dest.sunLightIntensity = Mathf.Min(dest.sunLightIntensity, p2.sunLightIntensityLim);
			}
			if (p2.e_sunLightShadowStrengthLim)
			{
				dest.sunLightShadowStrength = Mathf.Min(dest.sunLightShadowStrength, p2.sunLightShadowStrengthLim);
			}
			dest.backLightColor = Lerp(p2.e_backLightColor, p1.backLightColor, p2.backLightColor, v);
			dest.backLightIntensity = Lerp(p2.e_backLightIntensity, p1.backLightIntensity, p2.backLightIntensity, v);
			dest.gspBacklightColor = Lerp(p2.e_gspBacklightColor, p1.gspBacklightColor, p2.gspBacklightColor, v);
			dest.gspMaxLightBurn = Lerp(p2.e_gspMaxLightBurn, p1.gspMaxLightBurn, p2.gspMaxLightBurn, v);
			dest.ambientLightColor = Lerp(p2.e_ambientLightColor, p1.ambientLightColor, p2.ambientLightColor, v);
			Texture texture = ((p1.lutIsLerped && p2.e_lutTexture) ? p1.GetLerpedLut() : p1.lutTexture);
			if (texture == null)
			{
				dest.lutIsLerped = false;
				dest.lutTexture = p2.lutTexture;
			}
			else if (p2.lutTexture == null)
			{
				dest.lutIsLerped = false;
				dest.lutTexture = texture;
			}
			else
			{
				dest.lutIsLerped = true;
				dest.lutTexture = texture;
				dest.lutTexture2 = p2.lutTexture;
				dest.lutLerpFactor = v;
			}
			dest.additiveBloomThreshold = Lerp(p2.e_additiveBloom, p1.additiveBloomThreshold, p2.additiveBloomThreshold, v);
		}
	}

	private static Color Lerp(bool enable, Color c1, Color c2, float v)
	{
		if (!enable)
		{
			return c1;
		}
		return Color.Lerp(c1, c2, v * c2.a);
	}

	private static float Lerp(bool enable, float a1, float a2, float v)
	{
		if (!enable)
		{
			return a1;
		}
		return Mathf.Lerp(a1, a2, v);
	}

	private static Vector3 Lerp(bool enable, Vector3 a1, Vector3 a2, float v)
	{
		if (!enable)
		{
			return a1;
		}
		return Vector3.Lerp(a1, a2, v);
	}

	private Texture GetLerpedLut()
	{
		if (lutRT == null)
		{
			lutRT = RenderTexture.GetTemporary(lutTexture.width, lutTexture.height);
		}
		if (lutLerpMaterial == null)
		{
			lutLerpMaterial = new Material(Shader.Find("Custom/TextureLerp"));
		}
		lutLerpMaterial.SetTexture(idMainTex, lutTexture);
		lutLerpMaterial.SetTexture(idSecondaryTex, lutTexture2);
		lutLerpMaterial.SetFloat(idLerpFactor, lutLerpFactor);
		Graphics.Blit(null, lutRT, lutLerpMaterial);
		return lutRT;
	}
}
