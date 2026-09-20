using System.Collections.Generic;
using LinqTools;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Icon("BT", false, "")]
[Category("Nested")]
[Name("SubTree", 0)]
[Description("SubTree Node can be assigned an entire Sub BehaviorTree. The root node of that behaviour will be considered child node of this node and will return whatever it returns.\nThe target SubTree can also be set by using a Blackboard variable as normal.")]
public class SubTree : BTNode, IGraphAssignable
{
	[SerializeField]
	private BBParameter<BehaviourTree> _subTree;

	private Dictionary<BehaviourTree, BehaviourTree> instances = new Dictionary<BehaviourTree, BehaviourTree>();

	private BehaviourTree currentInstance;

	public override string name => base.name.ToUpper();

	public BehaviourTree subTree
	{
		get
		{
			return _subTree.value;
		}
		set
		{
			_subTree.value = value;
		}
	}

	Graph IGraphAssignable.nestedGraph
	{
		get
		{
			return subTree;
		}
		set
		{
			subTree = (BehaviourTree)value;
		}
	}

	Graph[] IGraphAssignable.GetInstances()
	{
		return instances.Values.ToArray();
	}

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (subTree == null || subTree.primeNode == null)
		{
			return Status.Failure;
		}
		if (base.status == Status.Resting)
		{
			currentInstance = CheckInstance();
		}
		return currentInstance.Tick(agent, blackboard);
	}

	protected override void OnReset()
	{
		if (currentInstance != null && currentInstance.primeNode != null)
		{
			currentInstance.primeNode.Reset();
		}
	}

	public override void OnGraphStoped()
	{
		if (currentInstance != null)
		{
			for (int i = 0; i < currentInstance.allNodes.Count; i++)
			{
				currentInstance.allNodes[i].OnGraphStoped();
			}
		}
	}

	public override void OnGraphPaused()
	{
		if (currentInstance != null)
		{
			for (int i = 0; i < currentInstance.allNodes.Count; i++)
			{
				currentInstance.allNodes[i].OnGraphPaused();
			}
		}
	}

	private BehaviourTree CheckInstance()
	{
		if (subTree == currentInstance)
		{
			return currentInstance;
		}
		BehaviourTree value = null;
		if (!instances.TryGetValue(subTree, out value))
		{
			value = Graph.Clone(subTree);
			instances[subTree] = value;
			for (int i = 0; i < value.allNodes.Count; i++)
			{
				value.allNodes[i].OnGraphStarted();
			}
		}
		value.agent = base.graphAgent;
		value.blackboard = base.graphBlackboard;
		value.UpdateReferences();
		subTree = value;
		return value;
	}
}
