using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Set GameResStr", 0)]
[Category("Game/GameRes")]
[Color("FFFFFF")]
public class Flow_SetGameResStr : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool remove;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<WgoData> wgo;

	private ValueInput<string> gameResStrKey;

	private ValueInput<string> gameResStrValue;

	private ValueOutput<string> gameResStrKeyOut;

	public override string name => (remove ? "Remove" : "Set") + " GameResStr" + (remove ? " From" : " To") + " WgoData";

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), ChangeGameResStr);
		@out = AddFlowOutput("out".CapitalizeFirst());
		gameResStrKey = AddValueInput<string>("gameResStrKey".CapitalizeFirst());
		if (!remove)
		{
			gameResStrValue = AddValueInput<string>("gameResStrValue".CapitalizeFirst());
		}
		gameResStrKeyOut = AddValueOutput("key", () => gameResStrKey.value);
	}

	private void ChangeGameResStr(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		if (wgoData != null)
		{
			if (!remove)
			{
				wgoData.GameResStr.Set(gameResStrKey.value, gameResStrValue.value);
			}
			else
			{
				wgoData.GameResStr.Remove(gameResStrKey.value);
			}
		}
		else
		{
			Debug.LogError("Flow_AddGameRes: Tried to " + (remove ? "remove" : "set") + " GameResStr " + (remove ? "from" : "to") + " null WGO");
		}
		@out.Call(flow);
	}
}
