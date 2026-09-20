using System;
using UnityEngine;

[Serializable]
public class NPCLifeSimulatorActionData
{
	[SerializeField]
	private float remainingTimeToAction;

	[SerializeField]
	private SGuid wgoId;

	[SerializeField]
	private NPCLifeSimulatorActionType actionType;

	public SGuid WgoId => wgoId;

	public NPCLifeSimulatorActionType ActionType => actionType;

	public float RemainingTimeToAction
	{
		get
		{
			return remainingTimeToAction;
		}
		set
		{
			remainingTimeToAction = value;
		}
	}

	public NPCLifeSimulatorActionData(SGuid wgoId, float rolledTime, NPCLifeSimulatorActionType actionType)
	{
		this.wgoId = wgoId;
		remainingTimeToAction = rolledTime;
		this.actionType = actionType;
	}
}
