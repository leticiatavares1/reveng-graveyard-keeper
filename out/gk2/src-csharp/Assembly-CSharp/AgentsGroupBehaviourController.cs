using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LazyBearTechnology;
using UnityEngine;

public class AgentsGroupBehaviourController : MonoBehaviour, IWgoCustomComponent<WCCD_AgentsGroupBehaviourController>
{
	private const int MAX_PATH_CALCULATIONS_PER_FRAME = 10;

	[SerializeField]
	[Space]
	private LazyConsts.Fighting.TeamType targetTeam;

	[SerializeField]
	private AgentAIEntityTypeMaskProvider agentAIByEntityTypeMask = new AgentAIEntityTypeMaskProvider();

	private List<PathCalculationData> pathCalculationDataList = new List<PathCalculationData>();

	private Dictionary<Guid, int> pathCalculationsByGuid = new Dictionary<Guid, int>();

	private List<FightingAgent> agents = new List<FightingAgent>();

	private Dictionary<SGuid, FightingAgent> agentsByIds = new Dictionary<SGuid, FightingAgent>();

	private Dictionary<SGuid, FightingAgent> availableAgents = new Dictionary<SGuid, FightingAgent>();

	private Dictionary<SGuid, FightingAgent> busyAgents = new Dictionary<SGuid, FightingAgent>();

	[CanBeNull]
	[SerializeField]
	private FightingLine fightingLine;

	private int targetLineIdx = -1;

	public int AgentsCount => agents.Count;

	public IReadOnlyList<FightingAgent> Agents => agents;

	public IReadOnlyCollection<FightingAgent> AvailableAgents => availableAgents.Values;

	public IReadOnlyCollection<FightingAgent> BusyAgents => busyAgents.Values;

	public FightingLine FightingLine
	{
		get
		{
			return fightingLine;
		}
		set
		{
			fightingLine = value;
		}
	}

	private IReadOnlyList<ICombatEntity> Targets
	{
		get
		{
			if (targetLineIdx != -1)
			{
				IEnumerable<ICombatEntity> targets = LazySingleton<FightingGameController>.Instance.TargetsDatabase.GetTargets(targetLineIdx);
				if (targets != null && targets.Any())
				{
					return targets.Where((ICombatEntity t) => t.TeamType == targetTeam).ToList();
				}
			}
			return LazySingleton<FightingGameController>.Instance.TargetsDatabase.GetTargetsByTeam(targetTeam);
		}
	}

	public LazyConsts.Fighting.TeamType TargetTeam
	{
		get
		{
			return targetTeam;
		}
		set
		{
			targetTeam = value;
		}
	}

	public void Init()
	{
		MainGame.PlayerController.CurrentGameScene.GameSceneData.OnWgoDataPreRemove += HandleZombieRemoved;
	}

	public void DeInit()
	{
		if (MainGame.PlayerController.TryGetCurrentGameScene(out var gameScene) && gameScene.GameSceneData != null)
		{
			gameScene.GameSceneData.OnWgoDataPreRemove -= HandleZombieRemoved;
		}
	}

	public void AddPathCalculation(PathCalculationData data)
	{
		if (data == null)
		{
			return;
		}
		if (pathCalculationsByGuid.TryGetValue(data.agentGuid.Guid, out var value) && value >= 0 && value < pathCalculationDataList.Count && pathCalculationDataList[value].agentGuid.Guid == data.agentGuid.Guid)
		{
			pathCalculationDataList[value] = data;
			return;
		}
		int num = pathCalculationDataList.FindIndex((PathCalculationData p) => p.agentGuid.Guid == data.agentGuid.Guid);
		if (num >= 0)
		{
			pathCalculationDataList[num] = data;
			pathCalculationsByGuid[data.agentGuid.Guid] = num;
		}
		else
		{
			pathCalculationDataList.Add(data);
			pathCalculationsByGuid[data.agentGuid.Guid] = pathCalculationDataList.Count - 1;
		}
	}

	public void RemovePathCalculation(Guid agentGuid)
	{
		int num = -1;
		if (pathCalculationsByGuid.TryGetValue(agentGuid, out var value) && value >= 0 && value < pathCalculationDataList.Count && pathCalculationDataList[value].agentGuid.Guid == agentGuid)
		{
			num = value;
		}
		if (num < 0)
		{
			num = pathCalculationDataList.FindIndex((PathCalculationData p) => p.agentGuid.Guid == agentGuid);
		}
		if (num >= 0)
		{
			pathCalculationDataList.RemoveAt(num);
			RebuildPathCalculationsByGuid();
		}
	}

	public FightingAgent AddWgoAsAgent(Wgo wgo, List<PathfindingPenalty> penalties = null, bool snapToNavmesh = true)
	{
		if (!wgo)
		{
			Debug.LogError("[AgentsGroupBehaviourController] Cannot add null Wgo as agent.");
			return null;
		}
		WgoPart wgoPart = wgo.MainWgoPart;
		if (!wgoPart || wgoPart.Wgo != wgo)
		{
			wgoPart = wgo.GetComponentInChildren<WgoPart>(includeInactive: true);
		}
		if (!wgoPart)
		{
			Debug.LogError($"[AgentsGroupBehaviourController] Main WgoPart is missing for [{wgo.Id}] [{wgo.Data.UniqueId}]");
			return null;
		}
		if (!wgoPart.TryGetComponent<FightingAgent>(out var component))
		{
			component = wgoPart.gameObject.AddComponent<FightingAgent>();
		}
		component.Init(wgo, this);
		component.SetPathPenalties(penalties);
		component.SetPosition(wgo.Data.Position, stopCurrentCommand: true, snapToNavmesh);
		AddAgent(component);
		return component;
	}

	public void AddAgent(FightingAgent agent)
	{
		agent.ParentController?.RemoveAgent(agent);
		agent.ParentController = this;
		agent.OnCommandCompleted += HandleAgentCommandCompleted;
		agents.Add(agent);
		agentsByIds.Add(agent.Wgo.Data.UniqueId, agent);
		availableAgents.Add(agent.Wgo.Data.UniqueId, agent);
		if (agentAIByEntityTypeMask.TryGetAgentAI(agent.Wgo.EntityType, out var ai))
		{
			agent.SetAgentAI(ai);
		}
	}

	public void RemoveAgent(int idx)
	{
		FightingAgent fightingAgent = agents[idx];
		fightingAgent.ParentController = null;
		SGuid uniqueId = fightingAgent.Wgo.Data.UniqueId;
		fightingAgent.OnCommandCompleted -= HandleAgentCommandCompleted;
		agents.RemoveAt(idx);
		agentsByIds.Remove(uniqueId);
		availableAgents.Remove(uniqueId);
		busyAgents.Remove(uniqueId);
		if (agentAIByEntityTypeMask.TryGetAgentAI(fightingAgent.Wgo.EntityType, out var _))
		{
			fightingAgent.SetAgentAI(null);
		}
	}

	public void RemoveAgent(SGuid guid)
	{
		if (agentsByIds.TryGetValue(guid, out var value))
		{
			agentsByIds.Remove(guid);
			availableAgents.Remove(guid);
			busyAgents.Remove(guid);
			if ((bool)value)
			{
				agents.Remove(value);
				value.OnCommandCompleted -= HandleAgentCommandCompleted;
			}
		}
	}

	public void RemoveAgent(FightingAgent agent)
	{
		if (!agent)
		{
			return;
		}
		bool flag = false;
		for (int num = pathCalculationDataList.Count - 1; num >= 0; num--)
		{
			if (!((UnityEngine.Object)(object)pathCalculationDataList[num]?.richAI != (UnityEngine.Object)(object)agent.RichAI))
			{
				pathCalculationDataList.RemoveAt(num);
				flag = true;
			}
		}
		if (flag)
		{
			RebuildPathCalculationsByGuid();
		}
		agent.OnCommandCompleted -= HandleAgentCommandCompleted;
		if (agent.ParentController == this)
		{
			agent.ParentController = null;
		}
		agents.RemoveAll((FightingAgent a) => a == agent);
		RemoveAgentMappingsFor(agent, agentsByIds);
		RemoveAgentMappingsFor(agent, availableAgents);
		RemoveAgentMappingsFor(agent, busyAgents);
	}

	public void RemoveAllAgents()
	{
		for (int i = 0; i < agents.Count; i++)
		{
			FightingAgent fightingAgent = agents[i];
			if ((bool)fightingAgent)
			{
				fightingAgent.OnCommandCompleted -= HandleAgentCommandCompleted;
				if (fightingAgent.ParentController == this)
				{
					fightingAgent.ParentController = null;
				}
			}
		}
		agents.Clear();
		agentsByIds.Clear();
		availableAgents.Clear();
		busyAgents.Clear();
		pathCalculationDataList.Clear();
		pathCalculationsByGuid.Clear();
	}

	public void TrySetCommand(SGuid agentSGuid)
	{
		if (availableAgents.TryGetValue(agentSGuid, out var value) && !value.IsExecutingCommand)
		{
			OrderNewCommand(value);
		}
	}

	public void CustomUpdate(float deltaTime)
	{
		UpdatePathCalculations();
		UpdateAgentsState(deltaTime);
		UpdateCommands();
	}

	public void TryRetargetAgents()
	{
		foreach (FightingAgent agent in agents)
		{
			MobCommand newCommand = GetNewCommand(agent);
			if (agent.MobCommand == null || newCommand == null)
			{
				continue;
			}
			newCommand.Init(agent);
			if (agent.MobCommand.IsTheSameCommand(newCommand))
			{
				continue;
			}
			agent.StopCommandExecution();
			pathCalculationDataList.ForEach(delegate(PathCalculationData p)
			{
				if (p.agentGuid == agent.Wgo.Data.UniqueId)
				{
					p.isOutDated = true;
				}
			});
			agent.SetCommand(newCommand);
		}
	}

	public void SetTargetLineIdx(int lineIdx)
	{
		targetLineIdx = lineIdx;
	}

	private void UpdatePathCalculations()
	{
		if (pathCalculationDataList.Count == 0)
		{
			return;
		}
		int num = 0;
		bool flag = false;
		int num2 = 0;
		while (num2 < pathCalculationDataList.Count && num < 10)
		{
			PathCalculationData pathCalculationData = pathCalculationDataList[num2];
			if (pathCalculationData == null || pathCalculationData.isOutDated || (UnityEngine.Object)(object)pathCalculationData.richAI == null)
			{
				pathCalculationDataList.RemoveAt(num2);
				flag = true;
				continue;
			}
			if (pathCalculationData.richAI.pathPending)
			{
				num2++;
				continue;
			}
			pathCalculationData.richAI.destination = pathCalculationData.Destination;
			pathCalculationData.richAI.SearchPath();
			pathCalculationDataList.RemoveAt(num2);
			flag = true;
			num++;
		}
		if (flag)
		{
			RebuildPathCalculationsByGuid();
		}
	}

	private void RebuildPathCalculationsByGuid()
	{
		pathCalculationsByGuid.Clear();
		for (int i = 0; i < pathCalculationDataList.Count; i++)
		{
			pathCalculationsByGuid[pathCalculationDataList[i].agentGuid.Guid] = i;
		}
	}

	private void HandleZombieRemoved(WgoData wgoData)
	{
		RemoveAgent(wgoData.UniqueId);
	}

	private void UpdateAgentsState(float deltaTime)
	{
		for (int num = agents.Count - 1; num >= 0; num--)
		{
			agents[num].CustomUpdate(deltaTime);
		}
	}

	private void UpdateCommands()
	{
		List<FightingAgent> list = new List<FightingAgent>();
		foreach (FightingAgent agent in agents)
		{
			if (agent.Wgo.Data.HpComponent.Hp <= 0)
			{
				list.Add(agent);
			}
			else if (!agent.IsExecutingCommand)
			{
				OrderNewCommand(agent);
			}
		}
		int num;
		for (num = 0; num < list.Count; num++)
		{
			RemoveAgent(list[num].Wgo.Data.UniqueId);
			list[num].ClearCommand();
			list[num].PlayDying();
			list.RemoveAt(num);
			num--;
		}
	}

	private MobCommand GetNewCommand(FightingAgent agent)
	{
		if (!agent.AgentAI)
		{
			return null;
		}
		return agent.AgentAI.GetCommand(agent, () => Targets);
	}

	private void OrderNewCommand(FightingAgent agent)
	{
		if (agent.Wgo.Data.HpComponent.Hp <= 0)
		{
			agent.PlayDying();
			return;
		}
		MobCommand newCommand = GetNewCommand(agent);
		if (newCommand != null)
		{
			agent.SetCommand(newCommand);
			busyAgents[agent.Wgo.Data.UniqueId] = agent;
		}
	}

	private void HandleAgentCommandCompleted(FightingAgent agent, MobCommand command)
	{
		busyAgents.Remove(agent.Wgo.Data.UniqueId);
		OrderNewCommand(agent);
	}

	private static void RemoveAgentMappingsFor(FightingAgent agent, Dictionary<SGuid, FightingAgent> dictionary)
	{
		if (dictionary.Count == 0)
		{
			return;
		}
		List<SGuid> list = null;
		foreach (KeyValuePair<SGuid, FightingAgent> item in dictionary)
		{
			if (!(item.Value != agent))
			{
				if (list == null)
				{
					list = new List<SGuid>();
				}
				list.Add(item.Key);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				dictionary.Remove(list[i]);
			}
		}
	}

	private void RetargetForThoseWhoAreGoingOnNewTargetAdded(ICombatEntity addedEntity)
	{
		foreach (FightingAgent value in busyAgents.Values)
		{
			if (value.MobCommand != null && value.MobCommand.commandType == MobCommand.CommandType.GoTo && (addedEntity.CombatEntityPosition - value.Wgo.CombatEntityPosition).magnitude < (value.MobCommand.Position - value.Wgo.CombatEntityPosition).magnitude)
			{
				value.StopCommandExecution();
				OrderNewCommand(value);
			}
		}
	}

	public WCCD_AgentsGroupBehaviourController OnSave()
	{
		return new WCCD_AgentsGroupBehaviourController
		{
			targetTeam = targetTeam
		};
	}

	public void OnLoad(WCCD_AgentsGroupBehaviourController data)
	{
		TargetTeam = data.targetTeam;
	}

	public void OnUnload()
	{
		TargetTeam = LazyConsts.Fighting.TeamType.Player;
	}
}
