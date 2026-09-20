using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Check Phrase In BlackList", 0)]
[Category("Game/Dialogue")]
[Color("70f1ff")]
public class Flow_CheckMultiplePhrasesInBlackList : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @true;

	private FlowOutput @false;

	private List<ValueInput<string>> phrasesList = new List<ValueInput<string>>();

	[GatherPortsCallback]
	public int number = 1;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Check);
		@true = AddFlowOutput("true".CapitalizeFirst());
		@false = AddFlowOutput("false".CapitalizeFirst());
		for (int i = 0; i < number; i++)
		{
			phrasesList.Add(AddValueInput<string>($"Phrase {i + 1}"));
		}
	}

	private void Check(Flow flow)
	{
		foreach (ValueInput<string> phrases in phrasesList)
		{
			string item = (string)phrases;
			if (!MainGame.Instance.GameSave.knowledgeSystem.blackListPhrases.Contains(item))
			{
				@false.Call(flow);
				return;
			}
		}
		@true.Call(flow);
	}
}
