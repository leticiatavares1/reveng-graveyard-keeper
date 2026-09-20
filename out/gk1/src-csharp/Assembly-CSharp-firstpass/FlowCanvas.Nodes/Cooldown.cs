using System;
using System.Collections;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[ContextDefinedOutputs(new Type[] { typeof(float) })]
[ContextDefinedInputs(new Type[] { typeof(float) })]
[Description("Filters OUT so that it can't be called very frequently")]
[Category("Flow Controllers/Filters")]
public class Cooldown : FlowControlNode
{
	private float current;

	private Coroutine coroutine;

	public override string name => base.name + string.Format(" [{0}]", current.ToString("0.0"));

	public override void OnGraphStarted()
	{
		current = 0f;
		coroutine = null;
	}

	public override void OnGraphStoped()
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			coroutine = null;
			current = 0f;
		}
	}

	protected override void RegisterPorts()
	{
		FlowOutput o = AddFlowOutput("Out");
		FlowOutput ready = AddFlowOutput("Ready");
		ValueInput<float> time = AddValueInput<float>("Time");
		AddValueOutput("Current", () => Mathf.Max(current, 0f));
		AddFlowInput("In", delegate(Flow f)
		{
			if (current <= 0f && coroutine == null)
			{
				current = time.value;
				coroutine = StartCoroutine(CountDown(ready, f));
				o.Call(f);
			}
		});
		AddFlowInput("Cancel", delegate
		{
			if (coroutine != null)
			{
				StopCoroutine(coroutine);
				coroutine = null;
				current = 0f;
			}
		});
	}

	private IEnumerator CountDown(FlowOutput ready, Flow f)
	{
		while (current > 0f)
		{
			while (base.graph.isPaused)
			{
				yield return null;
			}
			current -= Time.deltaTime;
			yield return null;
		}
		coroutine = null;
		ready.Call(f);
	}
}
