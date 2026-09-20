using UnityEngine;

public class WorldFxColliderTriggerComponent : ColliderTriggerComponentBase
{
	[SerializeField]
	private Transform playTarget;

	[SerializeField]
	private WorldFxColliderTriggerData onEnter;

	[SerializeField]
	private WorldFxColliderTriggerData onExit;

	private void Awake()
	{
		onEnter.PlayTarget = playTarget;
		onExit.PlayTarget = playTarget;
		Init(onEnter, onExit);
	}
}
