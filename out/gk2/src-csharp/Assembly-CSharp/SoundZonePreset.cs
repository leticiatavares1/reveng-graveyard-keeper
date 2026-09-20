using UnityEngine;

[CreateAssetMenu(fileName = "SoundZonePreset", menuName = "GK2/SoundZonePreset", order = 1)]
public class SoundZonePreset : ScriptableObject
{
	public SoundZoneValuesType type;

	public float minDistance;

	public float maxDistance;

	public AnimationCurve curve;

	public float Evaluate(float volumeCoef)
	{
		if (type == SoundZoneValuesType.Curve)
		{
			return curve.Evaluate(volumeCoef);
		}
		return volumeCoef;
	}
}
