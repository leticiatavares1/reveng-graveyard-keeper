using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Color("eed9a7")]
[Name("Find GD points", 0)]
[ContextDefinedOutputs(new Type[] { typeof(GameObject) })]
[Category("Game Functions")]
public class Flow_FindGDPoints : MyFlowNode
{
	public override string name
	{
		get
		{
			string text = base.name;
			if (!GetInputValuePort<string>("Name").isDefaultValue)
			{
				return text + " by name";
			}
			if (!GetInputValuePort<string>("GD tag").isDefaultValue)
			{
				return text + " by GD tag";
			}
			return text + "\nby ??????";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<string> par_name = AddValueInput<string>("Name");
		ValueInput<string> par_ctag = AddValueInput<string>("GD tag");
		AddValueOutput("List", "GD point", delegate
		{
			if (!par_name.isDefaultValue)
			{
				return WorldMap.GetGDPointsByName(par_name.value);
			}
			if (!par_ctag.isDefaultValue)
			{
				return WorldMap.GetGDPointsByGDTag(par_ctag.value);
			}
			Debug.LogError("Flow_FindGDPoint: no input parameters set");
			return (List<GameObject>)null;
		});
	}

	protected override void OnNodeInspectorGUI()
	{
		base.OnNodeInspectorGUI();
		MakeStringNullIfEmpty("Name");
		MakeStringNullIfEmpty("GD tag");
	}
}
