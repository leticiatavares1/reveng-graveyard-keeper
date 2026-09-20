using System;
using System.Collections.Generic;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Smart Res", 0)]
[Category("Game/Actions")]
public class Flow_SmartRes : GKCustomFlowNode
{
	[Serializable]
	private class Option
	{
		public SmartResType resType;

		public string id = string.Empty;

		public float value;
	}

	[SerializeField]
	private List<Option> options = new List<Option>();

	private ValueInput<SmartResType> resType;

	private ValueInput<string> paramName;

	private ValueInput<float> paramValue;

	private ValueOutput<SmartRes> smartRes;

	protected override void RegisterPorts()
	{
		smartRes = AddValueOutput("smartRes".CapitalizeFirst(), delegate
		{
			SmartRes smartRes = new SmartRes();
			if (options.Count > 0)
			{
				for (int i = 0; i < options.Count; i++)
				{
					switch (options[i].resType)
					{
					case SmartResType.GameRes:
						if (smartRes.gameRes == null)
						{
							smartRes.gameRes = new GameRes();
						}
						smartRes.gameRes.Add(options[i].id, options[i].value);
						break;
					case SmartResType.Item:
						if (smartRes.items == null)
						{
							smartRes.items = new List<ItemCount>();
						}
						smartRes.items.Add(new ItemCount(options[i].id, Mathf.RoundToInt(options[i].value)));
						break;
					}
				}
			}
			else
			{
				switch (resType.value)
				{
				case SmartResType.GameRes:
					smartRes.gameRes = new GameRes();
					smartRes.gameRes.Add(new GameResAtom(paramName.value, paramValue.value));
					break;
				case SmartResType.Item:
					smartRes.items = new List<ItemCount>();
					smartRes.items.Add(new ItemCount(paramName.value, Mathf.RoundToInt(paramValue.value)));
					break;
				}
			}
			return smartRes;
		});
	}

	private bool SyncLists()
	{
		return false;
	}
}
