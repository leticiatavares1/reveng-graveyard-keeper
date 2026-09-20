using UnityEngine;

[CreateAssetMenu(fileName = "FightingAgentSettings", menuName = "GK2/Fighting/FightingAgentSettings")]
public class FightingAgentSettings : ScriptableObject
{
	[Space]
	public float aiPathRadius = 0.22f;

	public float aiPathHeight = 2f;

	[Space]
	public bool enableRvoDensityBehavior;

	[Range(0f, 1f)]
	public float rvoDensityBehaviorDensityThreshold = 0.5f;

	public bool useDockPointPrioritizationByWeapon;

	public float defaultRvoPriority = 0.5f;

	[Space]
	public bool slowWhenNotFacingTarget;

	public bool preventMovingBackwards;

	public float wallForce = 3f;

	public float wallDist = 1f;

	[Range(0.1f, 4f)]
	public float rvoAgentTimeHorizon = 0.3f;

	[Range(0.1f, 1f)]
	public float rvoAgentObstacleTimeHorizon = 0.15f;

	[Range(4f, 32f)]
	public int rvoMaxNeighbours = 10;

	[Range(0f, 1f)]
	public float mainHeroPushAllyRvoPriority = 0.15f;

	[Range(0.05f, 2f)]
	public float mainHeroPushHoldTime = 0.35f;
}
