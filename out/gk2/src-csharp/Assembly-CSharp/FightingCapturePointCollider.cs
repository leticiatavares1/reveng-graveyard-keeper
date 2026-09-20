using UnityEngine;

public class FightingCapturePointCollider : MonoBehaviour
{
	private FightingCapturePoint capturePoint;

	private void Awake()
	{
		capturePoint = GetComponentInParent<FightingCapturePoint>();
	}

	private void OnTriggerEnter(Collider other)
	{
		ICombatEntity componentInParent = other.GetComponentInParent<ICombatEntity>();
		if (componentInParent == null || !componentInParent.IsActiveCombatant)
		{
			return;
		}
		switch (componentInParent.TeamType)
		{
		case LazyConsts.Fighting.TeamType.Player:
			if (capturePoint.allies.Contains(componentInParent))
			{
				return;
			}
			capturePoint.allies.Add(componentInParent);
			break;
		case LazyConsts.Fighting.TeamType.WildZombie:
			if (capturePoint.enemies.Contains(componentInParent))
			{
				return;
			}
			capturePoint.enemies.Add(componentInParent);
			break;
		}
		capturePoint.CustomUpdate();
	}

	private void OnTriggerExit(Collider other)
	{
		ICombatEntity componentInParent = other.GetComponentInParent<ICombatEntity>();
		if (componentInParent != null && (capturePoint.allies.Remove(componentInParent) || capturePoint.enemies.Remove(componentInParent)))
		{
			capturePoint.CustomUpdate();
		}
	}
}
