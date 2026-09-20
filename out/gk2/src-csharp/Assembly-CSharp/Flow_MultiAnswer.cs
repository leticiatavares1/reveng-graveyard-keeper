using System;
using System.Collections.Generic;
using System.Text;
using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;

[Name("Multi Answer", 0)]
[Category("Game/Dialogue")]
[ParadoxNotion.Design.Icon("Dialogue", false, "")]
[Color("32f08e")]
public class Flow_MultiAnswer : GKCustomFlowNode
{
	[Serializable]
	public class AnswerOption
	{
		public string id;

		public bool isLockedByDefault;

		public ValueInput<AnswerData> valueInput;

		public FlowOutput output;
	}

	[SerializeField]
	private List<AnswerOption> answerOptions = new List<AnswerOption>();

	[SerializeField]
	private bool isOverBlackout;

	public List<AnswerOption> AnswerOptions => answerOptions;

	public override int MinWidth => 200;

	public override string name
	{
		get
		{
			foreach (FlowOutput outputFlowPort in GetOutputFlowPorts())
			{
				if (!(outputFlowPort.name == "Out") && !outputFlowPort.isConnected)
				{
					return "Multi Answer \n<color=#FF2020>!!!EMPTY OUT!!!</color>";
				}
			}
			return "Multi Answer";
		}
	}

	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		int num = -1;
		SyncLists();
		foreach (AnswerOption answerOption in answerOptions)
		{
			num++;
			string text = answerOption.id;
			StringBuilder stringBuilder = new StringBuilder("□□□");
			if (false)
			{
				text = "<color=#4040FF>" + stringBuilder?.ToString() + "</color> " + text;
			}
			string iD = "out_" + num;
			text = (answerOption.isLockedByDefault ? ("[Ô]" + text) : text);
			answerOption.output = AddFlowOutput(text, iD);
			int num2 = num;
			answerOption.valueInput = AddValueInput<AnswerData>("<color=#A08030>#" + num2 + "</color>");
		}
		ValueInput<WgoData> wgoTalker = AddValueInput<WgoData>("Talker");
		AddFlowInput("In", delegate(Flow f)
		{
			List<AnswerVisualData> list = new List<AnswerVisualData>();
			SyncLists();
			for (int i = 0; i < answerOptions.Count; i++)
			{
				string id = answerOptions[i].id;
				AnswerData answerData = null;
				QuestDef value;
				if (answerOptions[i].valueInput.isConnected)
				{
					answerData = answerOptions[i].valueInput.value;
					if (answerData.customHideCondition)
					{
						continue;
					}
				}
				else if (GameBalance.Me.questDefByReqPhrase.TryGetValue(id, out value))
				{
					answerData = value.finishCheck.GetAnswerDataByReqs();
				}
				list.Add(new AnswerVisualData
				{
					answerData = answerData,
					hiddenByDefault = answerOptions[i].isLockedByDefault,
					id = id
				});
			}
			Transform bubblePoint = MainGame.PlayerController.BubblePoint;
			Bubble.ShowMultiAnswer(list, bubblePoint, WgoDataParamOrSelf(wgoTalker), delegate(string chosen)
			{
				GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.MultiAnswerSay, chosen);
				answerOptions.Find((AnswerOption x) => x.id == chosen).output.Call(f);
			}, delegate
			{
			}, isOverBlackout);
			flow_out.Call(f);
		});
	}

	private bool SyncLists()
	{
		return false;
	}
}
