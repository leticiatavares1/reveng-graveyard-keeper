using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[ContextDefinedOutputs(new Type[] { typeof(WorldGameObject) })]
[Name("Find WGO", 0)]
[Category("Game Functions")]
[Color("eed9a7")]
[Icon("Cube", false, "")]
public class Flow_FindWGO : MyFlowNode
{
	public override string name
	{
		get
		{
			string text = base.name;
			if (!GetInputValuePort<string>("obj_id").isDefaultValue || GetInputValuePort<string>("obj_id").isConnected)
			{
				return text + " by ObjID";
			}
			if (!GetInputValuePort<string>("custom_tag").isDefaultValue || GetInputValuePort<string>("custom_tag").isConnected)
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
		ValueInput<string> par_obj_id = AddValueInput<string>("Obj ID", "obj_id");
		ValueInput<string> par_ctag = AddValueInput<string>("Custom tag", "custom_tag");
		ValueInput<bool> par_ignore_error = AddValueInput<bool>("Ignore not found err", "ignore_error");
		AddValueOutput("WGO", delegate
		{
			if (par_obj_id.HasValue())
			{
				return WorldMap.GetWorldGameObjectByObjId(par_obj_id.value, par_ignore_error.value);
			}
			if (par_ctag.HasValue())
			{
				return WorldMap.GetWorldGameObjectByCustomTag(par_ctag.value, par_ignore_error.value);
			}
			Debug.LogError("Flow_FindWGO: no input parameters set");
			return (WorldGameObject)null;
		});
	}

	protected override void OnNodeInspectorGUI()
	{
		base.OnNodeInspectorGUI();
		MakeStringNullIfEmpty("obj_id");
		MakeStringNullIfEmpty("custom_tag");
	}
}
