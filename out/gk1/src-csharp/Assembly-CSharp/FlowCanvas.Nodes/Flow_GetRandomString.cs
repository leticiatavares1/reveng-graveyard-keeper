using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Get random string")]
[Name("Get random string", 0)]
[Category("Game Actions")]
public class Flow_GetRandomString : MyFlowNode
{
	public List<string> strings = new List<string>();

	public string random_string = string.Empty;

	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("out");
		AddValueOutput("random string", () => random_string);
		ValueInput<string> in_exeption = AddValueInput<string>("exeption");
		AddFlowInput("In", delegate(Flow f)
		{
			bool flag = false;
			int num = -1;
			if (!string.IsNullOrEmpty(in_exeption.value))
			{
				for (int i = 0; i < strings.Count; i++)
				{
					if (strings[i] == in_exeption.value)
					{
						flag = true;
						num = i;
						break;
					}
				}
			}
			int num2 = UnityEngine.Random.Range(0, strings.Count - (flag ? 1 : 0));
			if (num >= 0 && num2 >= num && num2 < strings.Count - 1)
			{
				num2++;
			}
			random_string = strings[num2];
			flow_out.Call(f);
		});
	}
}
