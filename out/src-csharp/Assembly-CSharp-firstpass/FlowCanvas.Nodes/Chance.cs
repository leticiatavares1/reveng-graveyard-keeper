using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Filter the Flow based on a chance of 0 to 1 for 0% - 100%")]
[Category("Flow Controllers/Filters")]
[ContextDefinedInputs(new Type[] { typeof(float) })]
public class Chance : FlowControlNode
{
	protected override void RegisterPorts()
	{
		FlowOutput o = AddFlowOutput("Out");
		ValueInput<float> c = AddValueInput<float>("Percentage");
		AddFlowInput("In", delegate(Flow f)
		{
			if (UnityEngine.Random.Range(0f, 1f) <= c.value)
			{
				o.Call(f);
			}
		});
	}
}
