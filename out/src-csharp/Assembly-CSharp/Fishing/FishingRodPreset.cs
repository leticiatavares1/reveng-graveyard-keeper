using UnityEngine;

namespace Fishing;

[CreateAssetMenu(fileName = "FishingRodPreset", menuName = "Mini Games/Fishing Rod Preset", order = 1)]
public class FishingRodPreset : ScriptableObject
{
	[Header("Size of \"catching\" bar")]
	public int rect_size = 30;

	[Space]
	public float gravity = 0.8f;

	public float force = 0.1f;

	public float impulse = 1f;

	public float mass = 1f;
}
