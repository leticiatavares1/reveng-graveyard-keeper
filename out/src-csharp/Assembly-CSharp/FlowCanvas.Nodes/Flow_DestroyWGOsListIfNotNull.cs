using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Destroy WGOs List If Not Null", 0)]
[Category("Game Actions")]
[Icon("Cross", false, "")]
[Description("Destroy WGOs List If Not Null")]
public class Flow_DestroyWGOsListIfNotNull : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<List<WorldGameObject>> par_wgo = AddValueInput<List<WorldGameObject>>("WGOs List");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			List<WorldGameObject> value = par_wgo.value;
			if (value == null)
			{
				flow_out.Call(f);
			}
			else
			{
				foreach (WorldGameObject item in value)
				{
					if (!(item == null))
					{
						item.DestroyMe();
					}
				}
				flow_out.Call(f);
			}
		});
	}
}
