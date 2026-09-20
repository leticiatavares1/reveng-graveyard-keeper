using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Add WGO To Sim Group", 0)]
[Category("Game/Wgo")]
[Color("f47dff")]
public class Flow_AddWgoToSimGroup : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool remove;

	[GatherPortsCallback]
	[ShowIf("remove", 0)]
	public bool findGroupById;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> groupId;

	public override string name => (remove ? "Remove" : "Add") + " WGO" + (remove ? " From" : " To") + " Sim Group" + ((!remove && !findGroupById) ? " From Balance" : "");

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), SetInteractableState);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (findGroupById)
		{
			groupId = AddValueInput<string>("groupId?");
		}
	}

	private void SetInteractableState(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		if (wgoData != null)
		{
			NPCLifeSimulator npcLifeSimulator = MainGame.Instance.npcLifeSimulator;
			if (remove)
			{
				npcLifeSimulator.RemoveWgoFromGroup(wgoData);
			}
			else if (findGroupById)
			{
				npcLifeSimulator.AddWgoToGroup(wgoData, groupId.value);
			}
			else
			{
				npcLifeSimulator.AddWgoToGroupFromBalance(wgoData);
			}
		}
		@out.Call(flow);
	}
}
