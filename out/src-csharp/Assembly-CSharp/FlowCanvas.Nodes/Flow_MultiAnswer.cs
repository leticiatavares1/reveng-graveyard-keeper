using System.Collections.Generic;
using System.Text;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Color("00FF00")]
[Icon("Dialogue", false, "")]
[Name("Multi answer", 0)]
[Category("Game Actions")]
public class Flow_MultiAnswer : MyFlowNode
{
	public List<string> answers = new List<string>();

	public override string name
	{
		get
		{
			foreach (Port outputPort in GetOutputPorts())
			{
				if (!(outputPort.name == "Out") && !outputPort.isConnected)
				{
					return base.name + "\n<color=#FF2020>!!!EMPTY OUT!!!</color>";
				}
			}
			return base.name;
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		List<FlowOutput> outs = new List<FlowOutput>();
		List<ValueInput<AnswerData>> ins = new List<ValueInput<AnswerData>>();
		int num = -1;
		SyncLists();
		foreach (string answer in answers)
		{
			num++;
			string text = answer;
			StringBuilder stringBuilder = new StringBuilder("□□□");
			if (false)
			{
				text = "<color=#4040FF>" + stringBuilder?.ToString() + "</color> " + text;
			}
			string iD = "out_" + num;
			outs.Add(AddFlowOutput(text, iD));
			List<ValueInput<AnswerData>> list = ins;
			int num2 = num;
			list.Add(AddValueInput<AnswerData>("<color=#A08030>#" + num2 + "</color>"));
		}
		ValueInput<WorldGameObject> par_wgo_talker = AddValueInput<WorldGameObject>("Talker");
		AddFlowInput("In", delegate(Flow f)
		{
			List<AnswerVisualData> list2 = new List<AnswerVisualData>();
			SyncLists();
			for (int i = 0; i < answers.Count; i++)
			{
				MultipleAnswerVisualData vis_data = new MultipleAnswerVisualData
				{
					id = answers[i]
				};
				if (ins[i].value is MultipleAnswerData)
				{
					MultipleAnswerData multipleAnswerData = (MultipleAnswerData)ins[i].value;
					if (multipleAnswerData != null)
					{
						vis_data.answer_visual_datas = new List<AnswerVisualData>();
						multipleAnswerData.FillVisualData(ref vis_data, base.wgo);
					}
					list2.Add(vis_data);
				}
				else
				{
					AnswerVisualData vis_data2 = new AnswerVisualData
					{
						id = answers[i]
					};
					ins[i].value?.FillVisualData(ref vis_data2, base.wgo);
					list2.Add(vis_data2);
				}
			}
			WorldGameObject talker = (par_wgo_talker.HasValue() ? par_wgo_talker.value : MainGame.me.player);
			MainGame.me.player.ShowMultianswer(list2, delegate(string chosen)
			{
				outs[answers.IndexOf(chosen)].Call(f);
			}, null, null, talker);
			flow_out.Call(f);
		});
	}

	private bool SyncLists()
	{
		return false;
	}
}
