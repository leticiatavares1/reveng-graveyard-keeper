using System.Collections.Generic;
using LinqTools;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.StateMachines;

[Name("FSM", 0)]
[Category("Nested")]
[Description("Execute a nested FSM OnEnter and Stop that FSM OnExit. This state is Finished when the nested FSM is finished as well")]
public class NestedFSMState : FSMState, IGraphAssignable
{
	[SerializeField]
	protected BBParameter<FSM> _nestedFSM;

	private Dictionary<FSM, FSM> instances = new Dictionary<FSM, FSM>();

	private FSM currentInstance;

	public FSM nestedFSM
	{
		get
		{
			return _nestedFSM.value;
		}
		set
		{
			_nestedFSM.value = value;
		}
	}

	Graph IGraphAssignable.nestedGraph
	{
		get
		{
			return nestedFSM;
		}
		set
		{
			nestedFSM = (FSM)value;
		}
	}

	Graph[] IGraphAssignable.GetInstances()
	{
		return instances.Values.ToArray();
	}

	protected override void OnEnter()
	{
		if (nestedFSM == null)
		{
			Finish(inSuccess: false);
			return;
		}
		currentInstance = CheckInstance();
		currentInstance.StartGraph(base.graphAgent, base.graphBlackboard, autoUpdate: false, Finish);
	}

	protected override void OnUpdate()
	{
		currentInstance.UpdateGraph();
	}

	protected override void OnExit()
	{
		if (currentInstance != null && (currentInstance.isRunning || currentInstance.isPaused))
		{
			currentInstance.Stop();
		}
	}

	protected override void OnPause()
	{
		if (currentInstance != null)
		{
			currentInstance.Pause();
		}
	}

	private FSM CheckInstance()
	{
		if (nestedFSM == currentInstance)
		{
			return currentInstance;
		}
		FSM value = null;
		if (!instances.TryGetValue(nestedFSM, out value))
		{
			value = Graph.Clone(nestedFSM);
			instances[nestedFSM] = value;
		}
		value.agent = base.graphAgent;
		value.blackboard = base.graphBlackboard;
		nestedFSM = value;
		return value;
	}
}
