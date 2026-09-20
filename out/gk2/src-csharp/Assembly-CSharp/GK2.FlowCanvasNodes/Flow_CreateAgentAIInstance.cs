using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Create Agent AI Instance", 0)]
[Category("Game/Fighting")]
[Description("Creates a runtime instance of a specific AgentAI type.")]
public class Flow_CreateAgentAIInstance : GKCustomFlowNode
{
	public enum AIType
	{
		Event_03_GoToDir
	}

	[GatherPortsCallback]
	public AIType selectedType;

	private ValueInput<Direction> direction;

	private ValueInput<float> distance;

	private ValueOutput<AgentAI> aiInstance;

	protected override void RegisterPorts()
	{
		if (selectedType == AIType.Event_03_GoToDir)
		{
			direction = AddValueInput<Direction>("Direction");
			distance = AddValueInput<float>("Distance");
		}
		aiInstance = AddValueOutput("AI Instance", CreateInstance);
	}

	private AgentAI CreateInstance()
	{
		if (selectedType == AIType.Event_03_GoToDir)
		{
			Event_03_GoToDir event_03_GoToDir = ScriptableObject.CreateInstance<Event_03_GoToDir>();
			event_03_GoToDir.Direction = direction.value.ConvertToVector3();
			event_03_GoToDir.Distance = distance.value;
			return event_03_GoToDir;
		}
		return null;
	}
}
