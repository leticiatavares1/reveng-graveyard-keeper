using System.Collections;
using System.Collections.Generic;
using NodeCanvas;
using ParadoxNotion.Services;
using UnityEngine;

namespace FlowCanvas.Nodes;

public abstract class LatentActionNodeBase : SimplexNode
{
	public enum InvocationMode
	{
		QueueCalls,
		FilterCalls
	}

	public InvocationMode invocationMode;

	private FlowOutput outFlow;

	private FlowOutput doing;

	private FlowOutput done;

	private Queue<IEnumerator> enumeratorQueue = new Queue<IEnumerator>();

	private Queue<Flow> flowQueue = new Queue<Flow>();

	private Coroutine currentCoroutine;

	private bool graphPaused;

	public override string name
	{
		get
		{
			if (enumeratorQueue.Count <= 0)
			{
				return base.name;
			}
			return $"{base.name} [{enumeratorQueue.Count.ToString()}]";
		}
	}

	public virtual bool exposeRoutineControls => true;

	public sealed override void OnGraphStarted()
	{
		enumeratorQueue = new Queue<IEnumerator>();
		flowQueue = new Queue<Flow>();
		currentCoroutine = null;
	}

	public sealed override void OnGraphStoped()
	{
		Break();
	}

	public sealed override void OnGraphPaused()
	{
		graphPaused = true;
	}

	public sealed override void OnGraphUnpaused()
	{
		graphPaused = false;
	}

	protected void Begin(IEnumerator enumerator, Flow f)
	{
		if (exposeRoutineControls && invocationMode == InvocationMode.QueueCalls && !enumeratorQueue.Contains(enumerator))
		{
			enumeratorQueue.Enqueue(enumerator);
			flowQueue.Enqueue(f);
		}
		if (currentCoroutine == null)
		{
			currentCoroutine = MonoManager.current.StartCoroutine(InternalCoroutine(enumerator, f));
		}
	}

	protected void Break()
	{
		if (currentCoroutine != null)
		{
			MonoManager.current.StopCoroutine(currentCoroutine);
			enumeratorQueue = new Queue<IEnumerator>();
			flowQueue = new Queue<Flow>();
			currentCoroutine = null;
			done.parent.SetStatus(Status.Resting);
			OnBreak();
			done.Call(default(Flow));
		}
	}

	private IEnumerator InternalCoroutine(IEnumerator enumerator, Flow f)
	{
		FlowNode parentNode = done.parent;
		parentNode.SetStatus(Status.Running);
		if (outFlow != null)
		{
			outFlow.Call(f);
		}
		f.Break = Break;
		while (enumerator.MoveNext())
		{
			while (graphPaused)
			{
				yield return null;
			}
			if (doing != null)
			{
				doing.Call(f);
			}
			yield return enumerator.Current;
		}
		f.Break = null;
		parentNode.SetStatus(Status.Resting);
		done.Call(f);
		currentCoroutine = null;
		if (enumeratorQueue.Count > 0)
		{
			enumeratorQueue.Dequeue();
			flowQueue.Dequeue();
			if (enumeratorQueue.Count > 0)
			{
				Begin(enumeratorQueue.Peek(), flowQueue.Peek());
			}
		}
	}

	protected override void OnRegisterPorts(FlowNode node)
	{
		if (exposeRoutineControls)
		{
			outFlow = node.AddFlowOutput("Start", "Out");
			doing = node.AddFlowOutput("Update", "Doing");
		}
		done = node.AddFlowOutput("Finish", "Done");
		OnRegisterDerivedPorts(node);
		if (exposeRoutineControls)
		{
			node.AddFlowInput("Break", delegate
			{
				Break();
			});
		}
	}

	protected abstract void OnRegisterDerivedPorts(FlowNode node);

	public virtual void OnBreak()
	{
	}
}
