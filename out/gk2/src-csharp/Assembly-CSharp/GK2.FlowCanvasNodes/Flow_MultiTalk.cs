using System.Collections.Generic;
using FlowCanvas;
using LinqTools;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Multi Talk", 0)]
[Category("Game/Dialogue")]
[Color("40addb")]
[ParadoxNotion.Design.Icon("Dialogue", false, "")]
public sealed class Flow_MultiTalk : GKMultiElementFlowNode<TalkElement>
{
	private FlowInput flowInput;

	private FlowOutput flowOutput;

	private FlowOutput flowOnFinished;

	protected override void RegisterPorts()
	{
		RegisterWgoIdsPorts();
		flowInput = AddFlowInput("In", DoTalk);
		flowOutput = AddFlowOutput("Out");
		flowOnFinished = AddFlowOutput("On Finished");
	}

	private void DoTalk(Flow flow)
	{
		DoTalkIteration(flow, 0);
		flowOutput.Call(flow);
	}

	private void DoTalkIteration(Flow flow, int index)
	{
		if (index >= elements.value.Length)
		{
			flowOnFinished.Call(flow);
			return;
		}
		TalkElement el = elements.value[index];
		bool flag = el.wgoId == "[Player]";
		WgoData npcWgoData = null;
		if (!flag)
		{
			npcWgoData = ((el.wgoId == "[Self]") ? base.SelfWgoData : ((el.wgoId == "[Wisp]") ? MainGame.PlayerController.WispController.GetWispWgoData() : MainGame.Instance.GameSave.worldData.GetWgoData(el.wgoId)));
		}
		PhraseData data = default(PhraseData);
		data.text = el.text;
		data.isPlayer = flag;
		data.npcWgoData = npcWgoData;
		data.onFinished = delegate
		{
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.SpeechSaid, el.text);
			DoTalkIteration(flow, index + 1);
		};
		data.cornerPosition = el.forceCornerPosition;
		data.speechType = SpeechBubbleType.Talk;
		Bubble.Talk(data);
	}

	protected override void SeparateNode(int i)
	{
		Flow_MultiTalk flow_MultiTalk = base.flowGraph.AddNode<Flow_MultiTalk>(base.position + Vector2.right * 200f);
		string[] value = wgosIds.value;
		foreach (string wGOId in value)
		{
			flow_MultiTalk.AddNewWgoIfAbsent(wGOId);
		}
		List<TalkElement> list = elements.value.ToList();
		for (int k = i + 1; k < elements.value.Length; k++)
		{
			flow_MultiTalk.AddNewElement(elements.value[k]);
			list.RemoveAt(i + 1);
		}
		elements.serializedValue = list.ToArray();
		RemoveUnusedWGOs();
		flow_MultiTalk.RemoveUnusedWGOs();
	}
}
