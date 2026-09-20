using Pathfinding;
using UnityEngine;

public class PathCalculationData
{
	public SGuid agentGuid;

	public RichAI richAI;

	public bool isOutDated;

	private ICombatEntity entity;

	private Vector3 targetPos;

	public Vector3 Destination => entity?.CombatEntityPosition ?? targetPos;

	public PathCalculationData(SGuid agentGuid, RichAI richAI, ICombatEntity entity)
	{
		this.agentGuid = agentGuid;
		this.richAI = richAI;
		this.entity = entity;
	}

	public PathCalculationData(SGuid agentGuid, RichAI richAI, Vector3 targetPos)
	{
		this.agentGuid = agentGuid;
		this.richAI = richAI;
		this.targetPos = targetPos;
	}
}
