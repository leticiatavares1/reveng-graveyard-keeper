using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("From IList To List", 0)]
[Category("Game Actions")]
[Description("If WGO is null, then self")]
public class Flow_FromIListToList : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<IList<string>> in_IList = AddValueInput<IList<string>>("IList");
		List<string> _out_list = null;
		FlowOutput flow_out = AddFlowOutput("Out");
		AddValueOutput("List", () => _out_list);
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_IList.value == null)
			{
				Debug.LogError("Flow_FromIListToList error: IList is null!");
				_out_list = new List<string>();
			}
			else
			{
				_out_list = new List<string>(in_IList.value);
			}
			flow_out.Call(f);
		});
	}
}
