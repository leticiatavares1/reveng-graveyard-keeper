using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Set Agent AI", 0)]
[Category("Game/Fighting")]
[Description("Sets a specific AgentAI instance to a single WGO.")]
[Color("f47dff")]
public class Flow_SetAgentAI : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool setNull;

	private ValueInput<WgoData> wgoData;

	private ValueInput<AgentAI> aiInstance;

	private FlowOutput outFlow;

	public override string name
	{
		get
		{
			if (!setNull)
			{
				return "Set Agent AI";
			}
			return "Set Agent AI Null";
		}
	}

	protected override void RegisterPorts()
	{
		wgoData = AddValueInput<WgoData>("WGO");
		if (!setNull)
		{
			aiInstance = AddValueInput<AgentAI>("AI Instance");
		}
		AddFlowInput("In", delegate(Flow f)
		{
			Execute();
			f.Call(outFlow);
		});
		outFlow = AddFlowOutput("Out");
	}

	private void Execute()
	{
		if (wgoData.value == null || (!setNull && aiInstance.value == null))
		{
			return;
		}
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(wgoData.value.UniqueId);
		if (!(wgoViewGlobal == null))
		{
			FightingAgent componentInChildren = wgoViewGlobal.GetComponentInChildren<FightingAgent>();
			if (componentInChildren == null)
			{
				Debug.LogError("[Flow_SetAgentAI]: WGO " + wgoData.value.id + " does not have a FightingAgent component.");
				return;
			}
			componentInChildren.SetAgentAI(setNull ? null : aiInstance.value);
			componentInChildren.StopCommandExecution();
		}
	}
}
