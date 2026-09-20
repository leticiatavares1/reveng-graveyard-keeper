using System.Collections.Generic;
using UnityEngine;

public class DynamicLights : MonoBehaviour
{
	private static List<Light> _lights_ground = new List<Light>();

	private static List<DynamicSpritePreset> _dyn_lights_ground = new List<DynamicSpritePreset>();

	private static List<Light> _lights_default = new List<Light>();

	private static List<DynamicLight> _dyn_lights_default = new List<DynamicLight>();

	private static List<float> _lights_ground_k = new List<float>();

	private static List<float> _lights_default_k = new List<float>();

	public static List<DynamicLight> dyn_lights = new List<DynamicLight>();

	public static HashSet<ObjectDynamicShadow> shadows = new HashSet<ObjectDynamicShadow>();

	private static DynamicLights _me = null;

	private static float _intensity_ground = 2.3f;

	private static float _intensity_default = 2.6f;

	private const float MIN_INTENSITY = 0.2f;

	public static void Init()
	{
		_me = SingletonGameObjects.FindOrCreate<DynamicLights>();
	}

	public static void SearchForLightsInNewObject(GameObject go)
	{
		Light[] componentsInChildren = go.GetComponentsInChildren<Light>(includeInactive: true);
		foreach (Light light in componentsInChildren)
		{
			GroundLight groundLight = light.gameObject.GetComponentInParent<GroundLight>();
			if (groundLight == null)
			{
				groundLight = light.gameObject.transform.parent.GetComponent<GroundLight>();
			}
			if (groundLight == null)
			{
				groundLight = light.gameObject.GetComponentInChildren<GroundLight>();
			}
			if (groundLight != null)
			{
				_lights_ground.Add(light);
				_lights_ground_k.Add(groundLight.intensity_k);
				_dyn_lights_ground.Add(groundLight.intensity_preset);
				continue;
			}
			DynamicLight dynamicLight = light.gameObject.GetComponentInParent<DynamicLight>() ?? light.gameObject.transform.parent.GetComponent<DynamicLight>();
			if (dynamicLight == null)
			{
				dynamicLight = light.gameObject.GetComponentInChildren<DynamicLight>();
			}
			if (dynamicLight == null)
			{
				Debug.LogError("Strange light, skipping. Name = " + light.name, light);
				continue;
			}
			_lights_default.Add(light);
			_dyn_lights_default.Add(dynamicLight);
			_lights_default_k.Add(dynamicLight.intensity_k);
		}
	}

	public static void SearchForLightsInDestroyedObject(GameObject go)
	{
		Light[] componentsInChildren = go.GetComponentsInChildren<Light>(includeInactive: true);
		foreach (Light item in componentsInChildren)
		{
			int num = _lights_default.IndexOf(item);
			if (num != -1)
			{
				_lights_default.RemoveAt(num);
				_lights_default_k.RemoveAt(num);
				_dyn_lights_default.RemoveAt(num);
			}
			num = _lights_ground.IndexOf(item);
			if (num != -1)
			{
				_lights_ground.RemoveAt(num);
				_lights_ground_k.RemoveAt(num);
				_dyn_lights_ground.RemoveAt(num);
			}
		}
	}

	public void Update()
	{
		float k = Mathf.Clamp(TimeOfDay.light_intensity_k, 0.2f, 1f);
		AdjustDefaultLightsIntensity(k);
		AdjustGroundLightsIntensity(k);
		foreach (DynamicLight dyn_light in dyn_lights)
		{
			dyn_light.CustomUpdate();
		}
		foreach (ObjectDynamicShadow shadow in shadows)
		{
			shadow.CheckLightsRange(dyn_lights);
		}
	}

	private void AdjustDefaultLightsIntensity(float k)
	{
		for (int i = 0; i < _lights_default.Count; i++)
		{
			Light light = _lights_default[i];
			if (!(light == null) && !(light.gameObject == null) && light.gameObject.activeInHierarchy)
			{
				DynamicLight dynamicLight = _dyn_lights_default[i];
				DynamicSpritePreset dynamicSpritePreset = ((dynamicLight == null) ? null : dynamicLight.intensity_preset);
				float num = ((dynamicSpritePreset == null) ? 1f : dynamicSpritePreset.EvaluateAlpha());
				light.intensity = _intensity_default * k * _lights_default_k[i] * num;
			}
		}
	}

	private void AdjustGroundLightsIntensity(float k)
	{
		for (int i = 0; i < _lights_ground.Count; i++)
		{
			Light light = _lights_ground[i];
			if (!(light == null) && !(light.gameObject == null) && light.gameObject.activeInHierarchy)
			{
				DynamicSpritePreset dynamicSpritePreset = _dyn_lights_ground[i];
				float num = ((dynamicSpritePreset == null) ? 1f : dynamicSpritePreset.EvaluateAlpha());
				light.intensity = _intensity_ground * k * _lights_ground_k[i] * num;
			}
		}
	}
}
