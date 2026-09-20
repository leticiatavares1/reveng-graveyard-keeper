using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("If Character is null, then Player")]
[Category("Game Actions")]
[Name("Add Phrase To Blacklist", 0)]
public class Flow_AddPhraseToBlacklist : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<bool>("remove").value)
			{
				return "<color=#FFFF50>Remove Phrase From Blacklist</color>";
			}
			return "<color=#30FF30>Add Phrase To Blacklist</color>";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<string> in_phrase = AddValueInput<string>("Phrase ID");
		ValueInput<bool> in_remove = AddValueInput<bool>("remove");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (!string.IsNullOrEmpty(in_phrase.value))
			{
				if (in_remove.value)
				{
					MainGame.me.save.black_list_of_phrases.Remove(in_phrase.value);
				}
				else
				{
					MainGame.me.save.AddPhraseToBlackList(in_phrase.value);
				}
			}
			else
			{
				Debug.LogError("Phrase ID is null or empty!");
			}
			flow_out.Call(f);
		});
	}
}
