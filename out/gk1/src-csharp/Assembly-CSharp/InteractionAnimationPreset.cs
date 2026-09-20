using UnityEngine;

[CreateAssetMenu]
public class InteractionAnimationPreset : ScriptableObject
{
	public float duration = 1f;

	public float duration_random = 0.15f;

	public AnimationCurve alpha_curve;

	public float rotation_a = 10f;
}
