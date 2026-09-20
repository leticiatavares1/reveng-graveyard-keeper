using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set Player Armor", 0)]
[Category("Game/Player")]
[Color("313c8f")]
public class Flow_SetPlayerArmorState : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool isEnabled;

	[GatherPortsCallback]
	[ShowIf("isEnabled", 0)]
	public bool withArmor;

	[GatherPortsCallback]
	[ShowIf("isEnabled", 1)]
	public bool withSword;

	[GatherPortsCallback]
	[ShowIf("isEnabled", 1)]
	public bool withHelmet = true;

	private FlowInput @in;

	private FlowOutput @out;

	public override string name
	{
		get
		{
			if (!isEnabled)
			{
				return "Unequip Sword" + (withArmor ? " and Armor" : "");
			}
			return "Equip Armor" + (withHelmet ? "" : " (no helmet)") + (withSword ? " and Sword" : "");
		}
	}

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetArmorState);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void SetArmorState(Flow flow)
	{
		if (isEnabled)
		{
			PlayerController playerController = MainGame.PlayerController;
			bool flag = withHelmet;
			playerController.SetArmorView(isActive: true, null, flag);
			if (withSword)
			{
				MainGame.PlayerController.AttackComponent.EquipWeapon(ItemType.Sword);
			}
		}
		else
		{
			MainGame.PlayerController.AttackComponent.UnequipWeapon();
			if (withArmor)
			{
				MainGame.PlayerController.SetArmorView(isActive: false);
			}
		}
		@out.Call(flow);
	}
}
