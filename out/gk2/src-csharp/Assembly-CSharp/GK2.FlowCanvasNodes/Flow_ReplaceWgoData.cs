using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Replace WGO Data", 0)]
[Category("Game/Wgo")]
[Color("f47dff")]
public class Flow_ReplaceWgoData : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool transferDataToNewWgo;

	protected FlowInput @in;

	protected FlowOutput @out;

	protected ValueInput<string> newWgoId;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), ReplaceWGOData);
		@out = AddFlowOutput("out".CapitalizeFirst());
		newWgoId = AddValueInput<string>("newWgoId");
	}

	protected virtual void ReplaceWGOData(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		if (wgoData == null)
		{
			Debug.LogError(string.Format("[{0}]: not found wgoData: {1}", "GKCustomFlowNode", wgoId));
		}
		else if (transferDataToNewWgo)
		{
			MainGame.Instance.GameSave.WorldData.ChangeWgoData(wgoData, newWgoId.value);
		}
		else
		{
			MainGame.Instance.GameSave.WorldData.ReplaceWgoData(wgoData, newWgoId.value);
		}
		@out.Call(flow);
	}
}
