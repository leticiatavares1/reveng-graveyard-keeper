using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set Wgo Damage Immunity", 0)]
[Category("Game/WGO")]
[Color("313c8f")]
public class Flow_SetWgoDamageImmunity : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool isPlayer;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<bool> hasDamageImmunity;

	public override string name => "Set " + (isPlayer ? "Player" : "Wgo") + " Damage Immunity";

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetDamageImmunityState);
		@out = AddFlowOutput("out".CapitalizeFirst());
		hasDamageImmunity = AddValueInput<bool>("hasDamageImmunity");
		if (!isPlayer)
		{
			base.RegisterPorts();
		}
	}

	private void SetDamageImmunityState(Flow flow)
	{
		if (isPlayer)
		{
			MainGame.PlayerData.hpComponent.IsImmuneToDamage = hasDamageImmunity.value;
		}
		else
		{
			WgoData wgoData = GetWgoData();
			if (wgoData != null)
			{
				wgoData.HpComponent.IsImmuneToDamage = hasDamageImmunity.value;
			}
		}
		@out.Call(flow);
	}
}
