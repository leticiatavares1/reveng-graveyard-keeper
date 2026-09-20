using System;
using System.Collections;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Once called, will continuously call 'Do' while the input boolean condition is true. Once condition becomes or is false, 'Done' is called")]
[Category("Flow Controllers/Repeaters")]
[Name("While True", 0)]
[ContextDefinedInputs(new Type[] { typeof(bool) })]
public class While : FlowControlNode
{
	private Coroutine coroutine;

	public override void OnGraphStarted()
	{
		coroutine = null;
	}

	public override void OnGraphStoped()
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			coroutine = null;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<bool> c = AddValueInput<bool>("Condition");
		FlowOutput fCurrent = AddFlowOutput("Do");
		FlowOutput fFinish = AddFlowOutput("Done");
		AddFlowInput("In", delegate(Flow f)
		{
			if (coroutine == null)
			{
				coroutine = StartCoroutine(DoWhile(fCurrent, fFinish, f, c));
			}
		});
	}

	private IEnumerator DoWhile(FlowOutput fCurrent, FlowOutput fFinish, Flow f, ValueInput<bool> condition)
	{
		f.Break = delegate
		{
			coroutine = null;
		};
		while (coroutine != null && condition.value)
		{
			while (base.graph.isPaused)
			{
				yield return null;
			}
			fCurrent.Call(f);
			yield return null;
		}
		coroutine = null;
		f.Break = null;
		fFinish.Call(f);
	}
}
