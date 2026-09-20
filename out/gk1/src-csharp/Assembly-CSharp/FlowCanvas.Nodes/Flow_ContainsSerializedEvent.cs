using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Contains Serialized Event", 0)]
[Category("Game Actions")]
public class Flow_ContainsSerializedEvent : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<string> event_id = AddValueInput<string>("Event Id");
		bool contains = false;
		ValueInput<WorldGameObject> wgo = AddValueInput<WorldGameObject>("WGO");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(wgo);
			if (worldGameObject != null)
			{
				contains = worldGameObject.ContainsSerializedEvent(event_id.value);
				if (contains)
				{
					Debug.Log("Already has " + event_id.value + " Event");
				}
			}
			flow_out.Call(f);
		});
		AddValueOutput("Contains", () => contains);
	}
}
