using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Destroy Agent AI Instance", 0)]
[Category("Game/Fighting")]
[Description("Explicitly destroys a runtime-created AgentAI instance to free memory.")]
public class Flow_DestroyAgentAIInstance : GKCustomFlowNode
{
	private ValueInput<AgentAI> aiInstance;

	private FlowOutput outFlow;

	protected override void RegisterPorts()
	{
		aiInstance = AddValueInput<AgentAI>("AI Instance");
		AddFlowInput("In", delegate(Flow f)
		{
			Execute();
			f.Call(outFlow);
		});
		outFlow = AddFlowOutput("Out");
	}

	private void Execute()
	{
		if (!(aiInstance.value == null))
		{
			Object.Destroy(aiInstance.value);
		}
	}
}
