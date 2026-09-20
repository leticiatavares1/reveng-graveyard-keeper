using System.Collections.Generic;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Get Fighting Line WGOs", 0)]
[Category("Game/Fighting")]
[Description("Retrieves all WGO Data objects belonging to agents on a specific line.")]
[Color("f47dff")]
public class Flow_GetLineWgos : GKCustomFlowNode
{
	private ValueInput<int> lineId;

	private ValueOutput<List<WgoData>> wgos;

	private ValueOutput<AgentsGroupBehaviourController> outAgentsGroupBehaviourController;

	private AgentsGroupBehaviourController agentsGroupBehaviourController;

	protected override void RegisterPorts()
	{
		lineId = AddValueInput<int>("Line ID");
		wgos = AddValueOutput("WGOs", GetWgos);
		outAgentsGroupBehaviourController = AddValueOutput("Group Controller", () => agentsGroupBehaviourController);
	}

	private List<WgoData> GetWgos()
	{
		List<WgoData> list = new List<WgoData>();
		FightingLevel currentLevel = LazySingleton<FightingGameController>.Instance.CurrentLevel;
		if (currentLevel == null)
		{
			return list;
		}
		FightingLine fightingLine = currentLevel.GetFightingLine(lineId.value);
		if (fightingLine == null)
		{
			return list;
		}
		agentsGroupBehaviourController = fightingLine.GetComponent<AgentsGroupBehaviourController>();
		if (agentsGroupBehaviourController == null)
		{
			return list;
		}
		foreach (FightingAgent agent in agentsGroupBehaviourController.Agents)
		{
			if (agent != null && agent.Wgo != null)
			{
				list.Add(agent.Wgo.Data);
			}
		}
		return list;
	}
}
