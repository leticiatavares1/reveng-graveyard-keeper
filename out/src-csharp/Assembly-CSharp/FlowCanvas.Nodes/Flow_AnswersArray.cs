using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Answers List", 0)]
[Category("Game Actions")]
public class Flow_AnswersArray : MyFlowNode
{
	public int count;

	private ValueOutput<List<AnswerData>> datasList;

	private List<ValueInput<AnswerData>> ins = new List<ValueInput<AnswerData>>();

	protected override void RegisterPorts()
	{
		datasList = AddValueOutput("Datas", GetData);
		for (int i = 0; i < count; i++)
		{
			List<ValueInput<AnswerData>> list = ins;
			int num = i;
			list.Add(AddValueInput<AnswerData>("<color=#A08030>#" + num + "</color>"));
		}
	}

	private List<AnswerData> GetData()
	{
		List<AnswerData> list = new List<AnswerData>();
		for (int i = 0; i < ins.Count; i++)
		{
			list.Add(ins[i].value);
		}
		return list;
	}

	protected override void OnNodeInspectorGUI()
	{
		base.OnNodeInspectorGUI();
		if (GUILayout.Button("Refresh"))
		{
			GatherPorts();
		}
	}
}
