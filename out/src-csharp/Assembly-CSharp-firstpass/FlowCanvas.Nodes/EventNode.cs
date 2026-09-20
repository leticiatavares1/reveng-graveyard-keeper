using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Color("ff5c5c")]
[Category("Events")]
[ContextDefinedOutputs(new Type[] { typeof(Flow) })]
public abstract class EventNode : FlowNode
{
	public override string name => $"➥ {base.name.ToUpper()}";
}
[ContextDefinedOutputs(new Type[] { typeof(Wild) })]
public abstract class EventNode<T> : EventNode where T : Component
{
	public BBParameter<T> target;

	public override string name => string.Format("{0} [{1}]", base.name.ToUpper(), (target.isNull && !target.useBlackboard) ? "Self" : target.ToString());

	public override void OnGraphStarted()
	{
		ResolveSelf();
	}

	protected void ResolveSelf()
	{
		if (target.isNull && !target.useBlackboard)
		{
			target.value = base.graphAgent.GetComponent<T>();
		}
	}
}
