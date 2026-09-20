using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Functions")]
[Icon("Cube", false, "")]
[ContextDefinedOutputs(new Type[] { typeof(List<WorldGameObject>) })]
[Color("eed9a7")]
[Name("Find WGOs List", 0)]
public class Flow_FindWGOsList : MyFlowNode
{
	public override string name
	{
		get
		{
			string text = base.name;
			if (!GetInputValuePort<string>("wgo_obj_id").isDefaultValue)
			{
				return text + " by ObjID";
			}
			if (!GetInputValuePort<string>("custom_tag").isDefaultValue)
			{
				return text + " by tag";
			}
			return text + " by ??????";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<string> par_objid = AddValueInput<string>("WGO obj ID", "wgo_obj_id");
		ValueInput<string> par_ctag = AddValueInput<string>("Custom tag", "custom_tag");
		AddValueOutput("List<WGO>", delegate
		{
			if (!par_objid.isDefaultValue)
			{
				return WorldMap.GetWorldGameObjectsByObjId(par_objid.value);
			}
			if (!par_ctag.isDefaultValue)
			{
				return WorldMap.GetWorldGameObjectsByCustomTag(par_ctag.value);
			}
			Debug.LogError("Flow_FindWGOsList: no input parameters set");
			return (List<WorldGameObject>)null;
		});
	}

	protected override void OnNodeInspectorGUI()
	{
		base.OnNodeInspectorGUI();
		MakeStringNullIfEmpty("wgo_obj_id");
		MakeStringNullIfEmpty("custom_tag");
	}
}
