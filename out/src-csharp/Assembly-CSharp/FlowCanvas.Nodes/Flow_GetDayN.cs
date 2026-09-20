using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Get Day num", 0)]
[Category("Game Functions")]
[ContextDefinedOutputs(new Type[] { typeof(int) })]
public class Flow_GetDayN : MyFlowNode
{
	protected override void RegisterPorts()
	{
		AddValueOutput("day #", delegate
		{
			if (MainGame.me == null)
			{
				Debug.LogError("MainGame.me is null!");
				return -1;
			}
			if (MainGame.me.save == null)
			{
				Debug.LogError("MainGame.me.save is null!");
				return -1;
			}
			return MainGame.me.save.day;
		});
	}
}
