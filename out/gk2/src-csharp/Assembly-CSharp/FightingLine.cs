using System;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using UnityEngine;

[RequireComponent(typeof(AgentsGroupBehaviourController))]
public class FightingLine : MonoBehaviour
{
	public List<EnemySpawnZone> spawnZones = new List<EnemySpawnZone>();

	public List<FightingSector> sectors = new List<FightingSector>();

	[SerializeField]
	private AgentsGroupBehaviourController agentsController;

	[NonSerialized]
	public int lineIdx = -1;

	private int currentSectorToCaptureIdx = -1;

	private bool isLineBreached;

	public bool AreAllSpawnZonesDisabled
	{
		get
		{
			foreach (EnemySpawnZone spawnZone in spawnZones)
			{
				if (spawnZone.gameObject.activeSelf)
				{
					return false;
				}
			}
			return true;
		}
	}

	public void SetActive(bool isActive)
	{
		foreach (FightingSector sector in sectors)
		{
			sector.SetActiveState(isActive);
		}
	}

	public void Init()
	{
		foreach (FightingSector sector in sectors)
		{
			sector.Init(this);
		}
		currentSectorToCaptureIdx = 0;
		agentsController.FightingLine = this;
		UpdateSectorsCapturability();
		agentsController.Init();
		agentsController.SetTargetLineIdx(-1);
		foreach (EnemySpawnZone spawnZone in spawnZones)
		{
			spawnZone.ResetActiveState();
		}
		base.enabled = true;
	}

	public void Deinit()
	{
		base.enabled = false;
		agentsController.DeInit();
		foreach (FightingSector sector in sectors)
		{
			sector.DeInit();
		}
		lineIdx = -1;
	}

	public void AddSpawnedEnemy(ICombatEntity agent)
	{
		if (agent is Wgo wgo && wgo.MainWgoPart.TryGetComponent<FightingAgent>(out var component))
		{
			agentsController.AddAgent(component);
		}
	}

	public void HandleSectorWasCaptured(FightingSector sector, LazyConsts.Fighting.TeamType team)
	{
		int num = FindNearestEnemySectorIdxBy(team);
		UpdateSectorsCapturability();
		switch (team)
		{
		case LazyConsts.Fighting.TeamType.Player:
			if (isLineBreached)
			{
				isLineBreached = false;
				agentsController.SetTargetLineIdx(lineIdx);
			}
			if (num != -1)
			{
				currentSectorToCaptureIdx = num;
			}
			else
			{
				HandleAllSectorsCapturedByPlayer();
			}
			LazySingleton<FightingGameController>.Instance.UIFightingOverlayData.TrackCapturePoint(sector.point);
			break;
		case LazyConsts.Fighting.TeamType.WildZombie:
			if (num != -1)
			{
				currentSectorToCaptureIdx = num;
			}
			else
			{
				HandleLineBreachedByEnemies();
			}
			LazySingleton<FightingGameController>.Instance.UIFightingOverlayData.UntrackCapturePoint(sector.point);
			break;
		}
		UpdateAgentTargets();
	}

	public FightingSector FindNearestEnemySectorBy(LazyConsts.Fighting.TeamType teamType)
	{
		int num = FindNearestEnemySectorIdxBy(teamType);
		if (num == -1)
		{
			return null;
		}
		return sectors[num];
	}

	public ICombatEntity FindNearestTargetOnLine(Vector3 from, LazyConsts.Fighting.TeamType targetTeamType, Func<IEnumerable<ICombatEntity>, ICombatEntity> customNearestFunc = null)
	{
		(ICombatEntity, float) tuple = (null, float.MaxValue);
		IEnumerable<ICombatEntity> enumerable = from e in LazySingleton<FightingGameController>.Instance.TargetsDatabase.GetTargets(lineIdx)
			where e.TeamType == targetTeamType
			select e;
		if (customNearestFunc != null)
		{
			return customNearestFunc(enumerable);
		}
		foreach (ICombatEntity item in enumerable)
		{
			float magnitude = (item.CombatEntityPosition - from).XZ().magnitude;
			if (!(magnitude >= tuple.Item2))
			{
				tuple.Item1 = item;
				tuple.Item2 = magnitude;
			}
		}
		return tuple.Item1;
	}

	private int FindNearestEnemySectorIdxBy(LazyConsts.Fighting.TeamType teamType)
	{
		if (sectors.Count == 0)
		{
			return -1;
		}
		int num = ((teamType != 0) ? (sectors.Count - 1) : 0);
		int num2 = ((teamType == LazyConsts.Fighting.TeamType.Player) ? (sectors.Count - 1) : 0);
		int num3 = ((teamType == LazyConsts.Fighting.TeamType.Player) ? 1 : (-1));
		for (int i = num; (num3 > 0) ? (i <= num2) : (i >= num2); i += num3)
		{
			if (sectors[i].point.OwnedByTeam != teamType)
			{
				return i;
			}
		}
		switch (teamType)
		{
		case LazyConsts.Fighting.TeamType.Player:
			if (sectors[0].point.OwnedByTeam == LazyConsts.Fighting.TeamType.WildZombie)
			{
				return 0;
			}
			break;
		case LazyConsts.Fighting.TeamType.WildZombie:
		{
			List<FightingSector> list = sectors;
			if (list[list.Count - 1].point.OwnedByTeam == LazyConsts.Fighting.TeamType.Player)
			{
				return sectors.Count - 1;
			}
			break;
		}
		}
		return -1;
	}

	private void UpdateSectorsCapturability()
	{
		if (sectors.Count == 0)
		{
			return;
		}
		if (sectors.Count == 1)
		{
			sectors[0].point.LockedForCapture = false;
			return;
		}
		bool flag = false;
		for (int i = 0; i < sectors.Count - 1; i++)
		{
			FightingSector fightingSector = sectors[i];
			FightingSector fightingSector2 = sectors[i + 1];
			if (i == 0)
			{
				fightingSector.point.LockedForCapture = fightingSector.point.OwnedByTeam == fightingSector2.point.OwnedByTeam;
			}
			if (fightingSector.point.OwnedByTeam == fightingSector2.point.OwnedByTeam)
			{
				fightingSector2.point.LockedForCapture = true;
				continue;
			}
			fightingSector2.point.LockedForCapture = false;
			flag = true;
		}
		if (!flag)
		{
			if (sectors[0].point.OwnedByTeam == LazyConsts.Fighting.TeamType.WildZombie)
			{
				sectors[0].point.LockedForCapture = false;
			}
			List<FightingSector> list = sectors;
			if (list[list.Count - 1].point.OwnedByTeam == LazyConsts.Fighting.TeamType.Player)
			{
				List<FightingSector> list2 = sectors;
				list2[list2.Count - 1].point.LockedForCapture = false;
			}
		}
	}

	private void HandleAllSectorsCapturedByPlayer()
	{
		Debug.Log("Line " + base.gameObject.name + " captured all sectors!");
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.FightLineCaptured, $"{LazySingleton<FightingGameController>.Instance.CurrentLevelId}:{lineIdx}");
	}

	private void UpdateAgentTargets()
	{
		agentsController.TryRetargetAgents();
	}

	private void HandleLineBreachedByEnemies()
	{
		if (isLineBreached)
		{
			return;
		}
		isLineBreached = true;
		LazySingleton<FightingGameController>.Instance.BreachedLines.Add(this);
		agentsController.SetTargetLineIdx(-1);
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.FightLineLost, $"{LazySingleton<FightingGameController>.Instance.CurrentLevelId}:{lineIdx}");
		Debug.LogWarning($"Line {lineIdx} has been breached by enemies!");
		foreach (ICombatEntity item in (from t in LazySingleton<FightingGameController>.Instance.TargetsDatabase.AllTargets
			where t.Team == LazyConsts.Fighting.TeamType.WildZombie && t.LineId == lineIdx
			select t.entity).ToList())
		{
			LazySingleton<FightingGameController>.Instance.UpdateTargetLocation(item);
			if (item is Wgo wgo && wgo.MainWgoPart.TryGetComponent<FightingAgent>(out var component))
			{
				component.ParentController = LazySingleton<FightingGameController>.Instance.BaseDefenseAgentsController;
				LazySingleton<FightingGameController>.Instance.BaseDefenseAgentsController.AddAgent(component);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		ICombatEntity componentInParent = other.GetComponentInParent<ICombatEntity>();
		if (componentInParent != null && componentInParent.TeamType == agentsController.TargetTeam)
		{
			UpdateAgentTargets();
		}
	}

	public void CustomUpdate(float deltaTime)
	{
		agentsController.CustomUpdate(deltaTime);
	}

	private void Awake()
	{
		base.enabled = false;
	}
}
