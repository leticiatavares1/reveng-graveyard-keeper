using System;
using LazyBearTechnology;
using UnityEngine;

public class FightingSector : MonoBehaviour
{
	public FightingCapturePoint point;

	public Collider sectorTrigger;

	[SerializeField]
	private bool keepActiveOnAwake;

	public FightingLine fightingLine;

	[NonSerialized]
	public int sectorIdx = -1;

	public bool IsFirst => sectorIdx == 0;

	public event Action<FightingSector> OnCapturedByAllies;

	public void Init(FightingLine fightingLine)
	{
		this.fightingLine = fightingLine;
		point.Init(this);
		point.SetOwnedByTeam(point.OwnedByTeamDefault);
		point.OnCapturedByTeam += HandleCapturePointCaptured;
		foreach (ICombatEntity ally in point.allies)
		{
			LazySingleton<FightingGameController>.Instance.RegisterTargetNonPersistent(ally, fightingLine.lineIdx, sectorIdx);
		}
		foreach (ICombatEntity enemy in point.enemies)
		{
			LazySingleton<FightingGameController>.Instance.RegisterTargetNonPersistent(enemy, fightingLine.lineIdx, sectorIdx);
		}
		if ((bool)sectorTrigger)
		{
			sectorTrigger.enabled = true;
		}
		Debug.Log($"Sector [{fightingLine.lineIdx}, {sectorIdx}]: registered allies: {point.allies.Count}, enemies: {point.enemies.Count}");
	}

	public void DeInit()
	{
		if ((bool)sectorTrigger)
		{
			sectorTrigger.enabled = false;
		}
		point.OnCapturedByTeam -= HandleCapturePointCaptured;
		point.DeInit();
		sectorIdx = -1;
	}

	public void SetActiveState(bool isActive)
	{
		base.gameObject.SetActive(isActive);
		point.SetActiveState(isActive);
	}

	private void HandleCapturePointCaptured(FightingCapturePoint capturePoint)
	{
		switch (capturePoint.OwnedByTeam)
		{
		case LazyConsts.Fighting.TeamType.Player:
			fightingLine.HandleSectorWasCaptured(this, LazyConsts.Fighting.TeamType.Player);
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.FightSectorCaptured, $"{LazySingleton<FightingGameController>.Instance.CurrentLevelId}:{fightingLine.lineIdx}:{sectorIdx}");
			break;
		case LazyConsts.Fighting.TeamType.WildZombie:
			fightingLine.HandleSectorWasCaptured(this, LazyConsts.Fighting.TeamType.WildZombie);
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.FightSectorLost, $"{LazySingleton<FightingGameController>.Instance.CurrentLevelId}:{fightingLine.lineIdx}:{sectorIdx}");
			break;
		}
	}

	private void Awake()
	{
		fightingLine = GetComponentInParent<FightingLine>();
		if (!sectorTrigger)
		{
			sectorTrigger = GetComponent<Collider>();
		}
		if ((bool)sectorTrigger)
		{
			Debug.Log("Sector [" + base.name + "]: sectorTrigger disabled", this);
			sectorTrigger.enabled = false;
		}
		if (!keepActiveOnAwake)
		{
			SetActiveState(isActive: false);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		ICombatEntity componentInParent = other.GetComponentInParent<ICombatEntity>();
		if (componentInParent != null && componentInParent.IsActiveCombatant)
		{
			LazySingleton<FightingGameController>.Instance.RegisterTargetNonPersistent(componentInParent, fightingLine.lineIdx, sectorIdx);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		ICombatEntity componentInParent = other.GetComponentInParent<ICombatEntity>();
		if (componentInParent != null)
		{
			LazySingleton<FightingGameController>.Instance.UpdateTargetLocation(componentInParent, fightingLine.lineIdx);
		}
	}
}
