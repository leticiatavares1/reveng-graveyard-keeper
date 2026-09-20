using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Tutorial Arrow", 0)]
[Category("Game")]
public class Flow_TutorialArrow : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool attach;

	[GatherPortsCallback]
	[InspectorName("Use Custom Tag for Wgo Data link")]
	public bool findByCustomTag;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> wgoId;

	private ValueInput<WgoData> wgoDataInput;

	public override string name
	{
		get
		{
			if (!attach)
			{
				return "UnAttach Tutorial Arrow";
			}
			return "Attach Tutorial Arrow";
		}
	}

	protected override void RegisterPorts()
	{
		if (findByCustomTag)
		{
			wgoId = AddValueInput<string>("WgoData");
		}
		else
		{
			wgoDataInput = AddValueInput<WgoData>("WgoData");
		}
		@in = AddFlowInput("in".CapitalizeFirst(), Action);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void Action(Flow flow)
	{
		if (attach)
		{
			LazySingleton<UITutorialArrow>.Instance.Attach(GetWgoData());
		}
		else
		{
			LazySingleton<UITutorialArrow>.Instance.UnAttach();
		}
		@out.Call(flow);
	}

	private WgoData GetWgoData()
	{
		if (!findByCustomTag)
		{
			return WgoDataParamOrSelf(wgoDataInput);
		}
		return MainGame.Instance.GameSave.worldData.GetWgoDataByCustomTag(wgoId.value);
	}
}
