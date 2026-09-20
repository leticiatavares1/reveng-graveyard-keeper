using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Enable Tutorial Mode", 0)]
[Category("Game/Tutorial")]
public class Flow_EnableTutorialMode : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool isEnable;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<List<WgoData>> wgoDataInput;

	public override string name
	{
		get
		{
			if (!isEnable)
			{
				return "Disable Tutorial Mode";
			}
			return "Enable Tutorial Mode";
		}
	}

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), CallMethod);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (isEnable)
		{
			wgoDataInput = AddValueInput<List<WgoData>>("wgos");
		}
	}

	protected virtual void CallMethod(Flow flow)
	{
		if (isEnable)
		{
			List<string> list = new List<string>();
			foreach (WgoData item in wgoDataInput.value)
			{
				if (item != null)
				{
					list.Add(item.UniqueId.Id);
				}
			}
			MainGame.PlayerData.SetTutorialModeState(isActive: true);
			MainGame.PlayerData.AddToTutorialModeExcludedList(list);
		}
		else
		{
			MainGame.PlayerData.SetTutorialModeState(isActive: false);
		}
		@out.Call(flow);
	}
}
