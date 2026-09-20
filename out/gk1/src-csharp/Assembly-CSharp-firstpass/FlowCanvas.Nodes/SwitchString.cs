using System;
using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Flow Controllers/Switchers")]
[Description("Branch the Flow based on a string value. The Default output is called if there is no other matching output same as the input value")]
[ContextDefinedInputs(new Type[] { typeof(string) })]
public class SwitchString : FlowControlNode
{
	public List<string> comparisonOutputs = new List<string>();

	protected override void RegisterPorts()
	{
		ValueInput<string> name = AddValueInput<string>("Value");
		List<FlowOutput> outs = new List<FlowOutput>();
		for (int i = 0; i < comparisonOutputs.Count; i++)
		{
			outs.Add(AddFlowOutput($"\"{comparisonOutputs[i]}\"", i.ToString()));
		}
		FlowOutput def = AddFlowOutput("Default");
		AddFlowInput("In", delegate(Flow f)
		{
			string value = name.value;
			if (value == null)
			{
				def.Call(f);
			}
			else
			{
				bool flag = false;
				for (int j = 0; j < comparisonOutputs.Count; j++)
				{
					if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(comparisonOutputs[j]))
					{
						outs[j].Call(f);
						flag = true;
					}
					else if (comparisonOutputs[j].Trim().ToLower() == value.Trim().ToLower())
					{
						outs[j].Call(f);
						flag = true;
					}
				}
				if (!flag)
				{
					def.Call(f);
				}
			}
		});
	}
}
