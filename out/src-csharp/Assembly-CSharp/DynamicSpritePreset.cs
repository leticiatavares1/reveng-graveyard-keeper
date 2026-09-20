using UnityEngine;

[CreateAssetMenu(menuName = "DynamicSprite Preset")]
public class DynamicSpritePreset : ScriptableObject
{
	public AnimationCurve daytime_alpha;

	public float EvaluateAlpha()
	{
		if (daytime_alpha != null)
		{
			return daytime_alpha.Evaluate(Mathf.Abs(TimeOfDay.me.time_of_day));
		}
		return 1f;
	}
}
