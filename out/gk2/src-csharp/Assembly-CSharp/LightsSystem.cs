using PI.NGSS;
using UnityEngine;

public class LightsSystem : MonoBehaviour
{
	[SerializeField]
	private Light sunLight;

	[SerializeField]
	private Light backLight;

	private static LightsSystem instance;

	private bool ngssFeatureEnabled = true;

	private bool backLightEnabled = true;

	public static LightsSystem Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Object.FindObjectOfType<LightsSystem>(includeInactive: true);
			}
			return instance;
		}
	}

	public Light SunLight => sunLight;

	public Light BackLight => backLight;

	public bool NgssFeatureEnabled => ngssFeatureEnabled;

	public bool BackLightEnabled => backLightEnabled;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		PlatformFeatures.ApplyShadowSettings();
		PlatformFeatures.ApplyNgssQuality();
		PlatformFeatures.ApplyBackLightSettings();
	}

	public void DisableNGSS()
	{
		NGSS_Local componentInChildren = GetComponentInChildren<NGSS_Local>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.enabled = false;
		}
		NGSS_Directional componentInChildren2 = GetComponentInChildren<NGSS_Directional>(includeInactive: true);
		if (componentInChildren2 != null)
		{
			componentInChildren2.enabled = false;
		}
		ngssFeatureEnabled = false;
	}

	public void EnableNGSS()
	{
		NGSS_Local componentInChildren = GetComponentInChildren<NGSS_Local>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.enabled = true;
		}
		NGSS_Directional componentInChildren2 = GetComponentInChildren<NGSS_Directional>(includeInactive: true);
		if (componentInChildren2 != null)
		{
			componentInChildren2.enabled = true;
		}
		ngssFeatureEnabled = true;
	}

	public void DisableBackLight()
	{
		if (backLight != null)
		{
			backLight.enabled = false;
		}
		backLightEnabled = false;
	}

	public void EnableBackLight()
	{
		if (backLight != null)
		{
			backLight.enabled = true;
		}
		backLightEnabled = true;
	}
}
