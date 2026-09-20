using UnityEngine;

[DisallowMultipleComponent]
public class LightSourceGroup : DayNightLightBase
{
	[SerializeField]
	private DayNightLight dayNightLight;

	[SerializeField]
	private LightFaker lightFaker;

	[SerializeField]
	private DayNightSprite dayNightSprite;

	private void Reset()
	{
		dayNightLight = GetComponent<DayNightLight>();
		lightFaker = GetComponent<LightFaker>();
		dayNightSprite = GetComponentInChildren<DayNightSprite>(includeInactive: true);
	}

	private void Awake()
	{
		ApplyLightMode(mode);
	}

	protected override void ApplyLightMode(LightMode lightMode)
	{
		mode = lightMode;
		dayNightLight?.ApplyLightModeInt((int)lightMode);
		lightFaker?.ApplyLightModeInt((int)lightMode);
		dayNightSprite?.ApplyLightModeInt((int)lightMode);
	}
}
