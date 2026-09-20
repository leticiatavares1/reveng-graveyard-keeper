using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[ContextDefinedInputs(new Type[] { typeof(GameObject) })]
[Description("Branch the Flow based on the tag of a GameObject value")]
[Category("Flow Controllers/Switchers")]
public class SwitchTag : FlowControlNode
{
	[SerializeField]
	private string[] _tagNames;

	protected override void RegisterPorts()
	{
		ValueInput<GameObject> go = AddValueInput<GameObject>("Value");
		List<FlowOutput> outs = new List<FlowOutput>();
		for (int i = 0; i < _tagNames.Length; i++)
		{
			outs.Add(AddFlowOutput(_tagNames[i], i.ToString()));
		}
		AddFlowInput("In", delegate(Flow f)
		{
			for (int j = 0; j < _tagNames.Length; j++)
			{
				if (_tagNames[j] == go.value.tag)
				{
					outs[j].Call(f);
				}
			}
		});
	}
}
