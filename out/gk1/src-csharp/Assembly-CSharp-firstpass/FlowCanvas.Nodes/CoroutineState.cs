using System.Collections;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Flow Controllers/Repeaters")]
[Name("Coroutine", 0)]
[Description("Start a Coroutine that will repeat until Break is signaled")]
public class CoroutineState : FlowControlNode
{
	private bool active;

	private Coroutine coroutine;

	public override void OnGraphStoped()
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			active = false;
		}
	}

	protected override void RegisterPorts()
	{
		FlowOutput fStarted = AddFlowOutput("Start");
		FlowOutput fUpdate = AddFlowOutput("Update");
		FlowOutput fFinish = AddFlowOutput("Finish");
		AddFlowInput("Start", delegate(Flow f)
		{
			if (!active)
			{
				active = true;
				coroutine = StartCoroutine(DoRepeat(fStarted, fUpdate, fFinish, f));
			}
		});
		AddFlowInput("Break", delegate
		{
			active = false;
		});
	}

	private IEnumerator DoRepeat(FlowOutput fStarted, FlowOutput fUpdate, FlowOutput fFinish, Flow f)
	{
		f.Break = delegate
		{
			active = false;
		};
		fStarted.Call(f);
		while (active)
		{
			while (base.graph.isPaused)
			{
				yield return null;
			}
			fUpdate.Call(f);
			yield return null;
		}
		f.Break = null;
		fFinish.Call(f);
	}
}
