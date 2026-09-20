using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
[ExecuteAlways]
public class LightFaker : DayNightLightBase
{
	private const string CIRCLE_SHADER = "Custom/Light Faker Circle";

	private static readonly int idIntensity = Shader.PropertyToID("_Intensity");

	private static readonly int idLocalOffset = Shader.PropertyToID("_LocalOffset");

	private static readonly int idCookie = Shader.PropertyToID("_Cookie");

	private static readonly float DEFAULT_INTENSITY_MULTIPLIER = 0.1f;

	private static readonly Quaternion WorldFlatRotation = Quaternion.Euler(90f, 0f, 0f);

	[SerializeField]
	private Light sourceLight;

	[SerializeField]
	private MeshRenderer circleRenderer;

	[SerializeField]
	[Tooltip("When enabled, circle mesh localScale is not driven by Light.range (keep authored size).")]
	private bool ignoreRangeScale;

	[SerializeField]
	private float rangeMultiplier = 1f;

	[SerializeField]
	private float intensityMultiplier = DEFAULT_INTENSITY_MULTIPLIER;

	[SerializeField]
	[Tooltip("Optional LightRT mask (alpha channel). Overrides Light.cookie when assigned.")]
	private Texture2D mask;

	[Space]
	[SerializeField]
	private bool keptGameObjectsActiveState;

	[SerializeField]
	private List<GameObject> keepGameObjectsActiveAccordingToLightingPolicy = new List<GameObject>();

	private MaterialPropertyBlock propertyBlock;

	private float syncedIntensity;

	private Color syncedColor = Color.white;

	private float syncedRange = 8f;

	private Vector3 localVisualOffset;

	private Light SourceLight
	{
		get
		{
			if (sourceLight == null)
			{
				sourceLight = GetComponent<Light>();
			}
			return sourceLight;
		}
	}

	private void Awake()
	{
		EnsureCircleRenderer();
		ApplyLightMode(mode);
		UpdateCustomObjectsActiveState();
	}

	private void OnEnable()
	{
		if (Application.isPlaying)
		{
			LightRTManager.Instance.Register(this);
		}
		SyncFromSourceLight();
		ApplyVisual();
	}

	private void OnDisable()
	{
		if (Application.isPlaying && LightRTManager.Instance != null)
		{
			LightRTManager.Instance.Unregister(this);
		}
	}

	private void Update()
	{
		SyncFromSourceLight();
		ApplyVisual();
		PropagateModeToSiblings();
	}

	protected override void ApplyLightMode(LightMode lightMode)
	{
		mode = lightMode;
		SyncFromSourceLight();
		ApplyVisual();
		PropagateModeToSiblings();
		if (TryGetComponent<DayNightLight>(out var component) && component.mode != lightMode)
		{
			component.ApplyLightModeInt((int)lightMode);
		}
	}

	private void PropagateModeToSiblings()
	{
		DayNightSprite[] componentsInChildren = GetComponentsInChildren<DayNightSprite>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].mode != mode)
			{
				componentsInChildren[i].ApplyLightModeInt((int)mode);
			}
		}
	}

	private void SyncFromSourceLight()
	{
		Light light = SourceLight;
		if (light != null)
		{
			syncedColor = light.color;
			syncedRange = light.range * rangeMultiplier;
		}
		syncedIntensity = GetBaseIntensity();
		float effectiveIntensity = GetEffectiveIntensity();
		if (light != null)
		{
			SwitchLightPolicy.ApplyRealLightEnabled(light, effectiveIntensity);
		}
	}

	private float GetBaseIntensity()
	{
		if (TryGetComponent<DayNightLight>(out var component))
		{
			return component.intensity * intensityMultiplier;
		}
		Light light = SourceLight;
		if (light != null)
		{
			return light.intensity * intensityMultiplier;
		}
		return syncedIntensity;
	}

	private float GetEffectiveIntensity()
	{
		float globalFloat = Shader.GetGlobalFloat(GlobalShaderParameters.idSunLight);
		return mode switch
		{
			LightMode.Night => Mathf.Lerp(syncedIntensity, 0f, globalFloat), 
			LightMode.Day => Mathf.Lerp(0f, syncedIntensity, globalFloat), 
			_ => syncedIntensity, 
		};
	}

	private void ApplyVisual()
	{
		EnsureCircleRenderer();
		if ((bool)circleRenderer)
		{
			float effectiveIntensity = GetEffectiveIntensity();
			circleRenderer.transform.rotation = WorldFlatRotation;
			if (!ignoreRangeScale)
			{
				float num = Mathf.Max(0.01f, syncedRange * 2f);
				circleRenderer.transform.localScale = new Vector3(num, num, 1f);
			}
			if (propertyBlock == null)
			{
				propertyBlock = new MaterialPropertyBlock();
			}
			circleRenderer.GetPropertyBlock(propertyBlock);
			propertyBlock.SetColor("_Color", syncedColor);
			propertyBlock.SetFloat(idIntensity, effectiveIntensity);
			propertyBlock.SetVector(idLocalOffset, localVisualOffset);
			propertyBlock.SetTexture(idCookie, ResolveCookieTexture());
			circleRenderer.SetPropertyBlock(propertyBlock);
			circleRenderer.enabled = SwitchLightPolicy.UseLightRT && effectiveIntensity > 0.001f;
		}
	}

	public void RefreshPolicyState()
	{
		UpdateCustomObjectsActiveState();
		SyncFromSourceLight();
		ApplyVisual();
	}

	private void EnsureCircleRenderer()
	{
		if (circleRenderer != null)
		{
			circleRenderer.gameObject.layer = 9;
			return;
		}
		Transform transform = base.transform.Find("LightFakerCircle");
		GameObject gameObject;
		if (transform != null)
		{
			gameObject = transform.gameObject;
		}
		else
		{
			gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
			if (gameObject.TryGetComponent<MeshRenderer>(out var component))
			{
				component.receiveShadows = false;
				component.shadowCastingMode = ShadowCastingMode.Off;
			}
			gameObject.name = "LightFakerCircle";
			gameObject.transform.SetParent(base.transform, worldPositionStays: false);
			gameObject.transform.rotation = WorldFlatRotation;
			if (gameObject.TryGetComponent<Collider>(out var component2))
			{
				Object.Destroy(component2);
			}
		}
		gameObject.layer = 9;
		circleRenderer = gameObject.GetComponent<MeshRenderer>();
		if (circleRenderer.sharedMaterial == null || circleRenderer.sharedMaterial.shader.name != "Custom/Light Faker Circle")
		{
			circleRenderer.sharedMaterial = LazySingletonSO<GlobalResources>.Instance.fakeLightMaterial;
		}
	}

	public void ApplyExternalState(Color color, float intensity, float range)
	{
		syncedColor = color;
		syncedRange = range * rangeMultiplier;
		syncedIntensity = ((mode == LightMode.Static) ? (intensity * intensityMultiplier) : GetBaseIntensity());
		ApplyVisual();
	}

	public void SetLocalVisualOffset(Vector3 worldOffset)
	{
		localVisualOffset = worldOffset;
		ApplyVisual();
	}

	private Texture ResolveCookieTexture()
	{
		if (mask != null)
		{
			return mask;
		}
		Light light = SourceLight;
		if (light != null && light.cookie is Texture2D result)
		{
			return result;
		}
		return Texture2D.whiteTexture;
	}

	private void UpdateCustomObjectsActiveState()
	{
		if (!SwitchLightPolicy.UseLightRT)
		{
			return;
		}
		foreach (GameObject item in keepGameObjectsActiveAccordingToLightingPolicy)
		{
			item.SetActive(keptGameObjectsActiveState);
		}
	}
}
