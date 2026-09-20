using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Always Enable Chunk On", 0)]
public class Flow_AlwaysEnableChunk : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetInputValuePort<bool>("Enable?").value)
			{
				return "Always Enable Chunk Off";
			}
			return base.name;
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<bool> in_alw_enable = AddValueInput<bool>("Enable?");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			try
			{
				worldGameObject.GetComponent<ChunkedGameObject>().always_active = in_alw_enable.value;
			}
			catch
			{
				Debug.LogError("Error in Flow_AlwayEnableChunk");
			}
			flow_out.Call(f);
		});
	}
}
