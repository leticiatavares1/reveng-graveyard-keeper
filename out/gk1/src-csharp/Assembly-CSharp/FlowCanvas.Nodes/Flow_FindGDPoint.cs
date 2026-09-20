using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Find GD point", 0)]
[Category("Game Functions")]
[Color("eed9a7")]
[ContextDefinedOutputs(new Type[] { typeof(GameObject) })]
public class Flow_FindGDPoint : MyFlowNode
{
	public override string name
	{
		get
		{
			string text = base.name;
			if (!GetInputValuePort<string>("Name").isDefaultValue || GetInputValuePort<string>("Name").isConnected)
			{
				return text + " by name";
			}
			if (!GetInputValuePort<string>("GD tag").isDefaultValue || GetInputValuePort<string>("GD tag").isConnected)
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
		ValueInput<bool> par_ignore_errors = AddValueInput<bool>("Not found is OK", "Ignore not\nfound error");
		AddValueOutput("GD point", delegate
		{
			if (par_name.HasValue())
			{
				GDPoint gDPointByName = WorldMap.GetGDPointByName(par_name.value, !par_ignore_errors.value);
				if (!(gDPointByName == null))
				{
					return gDPointByName.gameObject;
				}
				return (GameObject)null;
			}
			if (par_ctag.HasValue())
			{
				GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag(par_ctag.value, !par_ignore_errors.value);
				if (!(gDPointByGDTag == null))
				{
					return gDPointByGDTag.gameObject;
				}
				return (GameObject)null;
			}
			Debug.LogError("Flow_FindGDPoint: no input parameters set");
			return (GameObject)null;
		});
	}

	protected override void OnNodeInspectorGUI()
	{
		base.OnNodeInspectorGUI();
		MakeStringNullIfEmpty("Name");
		MakeStringNullIfEmpty("GD tag");
	}
}
