using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "FightingGlobal", menuName = "GK2/Fighting/Global")]
public class FightingGlobal : ScriptableObject
{
	[FormerlySerializedAs("defaultZombieAIAgentSettings")]
	public FightingAgentSettings defaultFightingAgentSettings;

	[Range(0f, 1f)]
	public float bloodDecalSpawnProbability = 0.5f;

	[Range(0f, 1f)]
	public float bonesDecalSpawnProbability = 1f;

	[Range(0f, 1f)]
	public float gutsDecalSpawnProbability = 1f;

	public float decalsLifeTime = 10f;
}
