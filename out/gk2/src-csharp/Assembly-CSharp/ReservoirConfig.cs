using UnityEngine;

[CreateAssetMenu(fileName = "ReservoirConfig", menuName = "ScriptableObjects/ReservoirConfig")]
public class ReservoirConfig : ScriptableObject
{
	[SerializeField]
	public float fishSpawnHorOffset = 1.75f;

	[SerializeField]
	public float fishSpawnVertOffset = 0.2f;

	[SerializeField]
	public float progressRange = 0.7f;
}
