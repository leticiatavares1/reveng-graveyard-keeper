using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;

[Name("GJ Trigger Animation Roll Enable", 0)]
[Category("Game Actions")]
public class Flow_GJAnimRandomTriggerEnable : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<GameObject> game_object_in;

	private ValueInput<bool> enable_flag_in;

	public override string name => "GJ Trigger Animation Roll " + (enable_flag_in.value ? "Enable" : "Disable");

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", SetActive);
		@out = AddFlowOutput("Out");
		game_object_in = AddValueInput<GameObject>("Game Object");
		enable_flag_in = AddValueInput<bool>("Is Active");
	}

	private void SetActive(Flow flow)
	{
		if (game_object_in.value != null)
		{
			GJAnimRandomTrigger componentInChildren = game_object_in.value.GetComponentInChildren<GJAnimRandomTrigger>();
			if (componentInChildren != null)
			{
				componentInChildren.SetRollActive(enable_flag_in.value);
			}
			else
			{
				Debug.LogError("GJAnimRandomTrigger component not found on object" + game_object_in.value);
			}
		}
		else
		{
			Debug.LogError("GameObject is null");
		}
		@out.Call(flow);
	}
}
