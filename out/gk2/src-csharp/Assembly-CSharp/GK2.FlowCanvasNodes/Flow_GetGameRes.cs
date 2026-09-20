using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Get GameRes", 0)]
[Category("Game/GameRes")]
public class Flow_GetGameRes : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool getFromPlayer;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<WgoData> wgo;

	private ValueInput<string> gameResName;

	private ValueOutput<string> gameResNameOut;

	private ValueOutput<float> gameResValueOut;

	private float gameResValOut;

	public override string name => "Get GameRes From " + (getFromPlayer ? "Player" : "WgoData");

	protected override void RegisterPorts()
	{
		if (!getFromPlayer)
		{
			base.RegisterPorts();
		}
		@in = AddFlowInput("in".CapitalizeFirst(), ChangeGameRes);
		@out = AddFlowOutput("out".CapitalizeFirst());
		gameResName = AddValueInput<string>("gameResName".CapitalizeFirst());
		gameResValueOut = AddValueOutput("Value", () => gameResValOut);
		gameResNameOut = AddValueOutput("paramName", () => gameResName.value);
	}

	private void ChangeGameRes(Flow flow)
	{
		if (getFromPlayer)
		{
			gameResValOut = base.PlayerData.GetRes(gameResName.value);
		}
		else
		{
			WgoData wgoData = GetWgoData();
			if (wgoData != null)
			{
				gameResValOut = wgoData.GetGameRes(gameResName.value);
			}
			else
			{
				Debug.LogError("Flow_AddGameRes: cannot get GameRes [" + gameResName.value + "] from null WGO");
			}
		}
		@out.Call(flow);
	}
}
