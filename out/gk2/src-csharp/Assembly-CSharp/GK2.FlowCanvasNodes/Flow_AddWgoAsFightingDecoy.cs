using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Add Wgo As Fighting Decoy", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_AddWgoAsFightingDecoy : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool toAdd;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<LazyConsts.Fighting.TargetAttackPriority> customPriority;

	public override string name
	{
		get
		{
			if (!toAdd)
			{
				return "Remove Wgo From Fighting Decoy";
			}
			return "Add Wgo As Fighting Decoy";
		}
	}

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), DoDecoyManipulationLogic);
		@out = AddFlowOutput("out".CapitalizeFirst());
		customPriority = AddValueInput<LazyConsts.Fighting.TargetAttackPriority>("customPriority");
		customPriority.SetDefaultAndSerializedValue(LazyConsts.Fighting.TargetAttackPriority.High);
	}

	private void DoDecoyManipulationLogic(Flow flow)
	{
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(GetWgoData()?.UniqueId);
		if (wgoViewGlobal != null)
		{
			if (toAdd)
			{
				LazySingleton<FightingGameController>.Instance.AddWgoAsCustomDecoy(wgoViewGlobal, setAsAgent: false, customPriority.value);
			}
			else
			{
				LazySingleton<FightingGameController>.Instance.RemoveWgFromCustomDecoy(wgoViewGlobal);
			}
		}
		@out.Call(flow);
	}
}
