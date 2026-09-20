using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Get GameResStr", 0)]
[Category("Game/GameRes")]
public class Flow_GetGameResStr : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool findByValue;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<WgoData> wgo;

	private ValueInput<string> gameResStrKey;

	private ValueInput<string> gameResStrValue;

	private ValueOutput<string> gameResStrKeyOut;

	private ValueOutput<string> gameResStrValueOut;

	private ValueOutput<bool> hasGameResStrOut;

	private string gameResStrKOut;

	private string gameResStrValOut;

	private bool hasGameRestStr;

	public override string name => "Get GameResStr From WgoData";

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), GetGameResStr);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (!findByValue)
		{
			gameResStrKey = AddValueInput<string>("gameResStrKey".CapitalizeFirst());
		}
		else
		{
			gameResStrValue = AddValueInput<string>("gameResStrValue".CapitalizeFirst());
		}
		hasGameResStrOut = AddValueOutput("Has", () => hasGameRestStr);
		gameResStrKeyOut = AddValueOutput("Key", () => gameResStrKOut);
		gameResStrValueOut = AddValueOutput("Value", () => gameResStrValOut);
	}

	private void GetGameResStr(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		if (wgoData != null)
		{
			if (!findByValue)
			{
				gameResStrKOut = gameResStrKey.value;
				hasGameRestStr = wgoData.GameResStr.Has(gameResStrKOut);
				gameResStrValOut = wgoData.GameResStr.Get(gameResStrKOut);
			}
			else
			{
				gameResStrValOut = gameResStrValue.value;
				gameResStrKOut = wgoData.GameResStr.GetKeyByValue(gameResStrValOut);
				hasGameRestStr = wgoData.GameResStr.Has(gameResStrKOut);
			}
		}
		else
		{
			Debug.LogError("Flow_AddGameRes: cannot get GameRes [" + gameResStrKey.value + "] from null WGO");
		}
		@out.Call(flow);
	}
}
