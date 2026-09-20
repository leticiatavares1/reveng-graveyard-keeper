using System;
using LinqTools;
using NodeCanvas.Framework;
using ParadoxNotion;
using UnityEngine;

namespace NodeCanvas.StateMachines;

public abstract class FSMState : Node, IState
{
	public enum TransitionEvaluationMode
	{
		CheckContinuously,
		CheckAfterStateFinished,
		CheckManually
	}

	[SerializeField]
	private TransitionEvaluationMode _transitionEvaluation;

	private float _elapsedTime;

	private bool hasInit;

	public override int maxInConnections => -1;

	public override int maxOutConnections => -1;

	public sealed override Type outConnectionType => typeof(FSMConnection);

	public override bool allowAsPrime => true;

	public sealed override Alignment2x2 commentsAlignment => Alignment2x2.Bottom;

	public sealed override Alignment2x2 iconAlignment => Alignment2x2.Default;

	public TransitionEvaluationMode transitionEvaluation
	{
		get
		{
			return _transitionEvaluation;
		}
		set
		{
			_transitionEvaluation = value;
		}
	}

	public float elapsedTime
	{
		get
		{
			return _elapsedTime;
		}
		private set
		{
			_elapsedTime = value;
		}
	}

	public FSM FSM => (FSM)base.graph;

	public FSMConnection[] GetTransitions()
	{
		return base.outConnections.Cast<FSMConnection>().ToArray();
	}

	public void Finish()
	{
		Finish(inSuccess: true);
	}

	public void Finish(bool inSuccess)
	{
		base.status = (inSuccess ? Status.Success : Status.Failure);
	}

	public sealed override void OnGraphStarted()
	{
	}

	public sealed override void OnGraphStoped()
	{
		base.status = Status.Resting;
	}

	public sealed override void OnGraphPaused()
	{
		if (base.status == Status.Running)
		{
			OnPause();
		}
	}

	protected sealed override Status OnExecute(Component agent, IBlackboard bb)
	{
		if (!hasInit)
		{
			hasInit = true;
			OnInit();
		}
		if (base.status == Status.Resting || base.status == Status.Running)
		{
			base.status = Status.Running;
			for (int i = 0; i < base.outConnections.Count; i++)
			{
				if (((FSMConnection)base.outConnections[i]).condition != null)
				{
					((FSMConnection)base.outConnections[i]).condition.Enable(agent, bb);
				}
			}
			OnEnter();
		}
		return base.status;
	}

	public void Update()
	{
		elapsedTime += Time.deltaTime;
		if (transitionEvaluation == TransitionEvaluationMode.CheckContinuously)
		{
			CheckTransitions();
		}
		else if (transitionEvaluation == TransitionEvaluationMode.CheckAfterStateFinished && base.status != Status.Running)
		{
			CheckTransitions();
		}
		if (base.status == Status.Running)
		{
			OnUpdate();
		}
	}

	public bool CheckTransitions()
	{
		for (int i = 0; i < base.outConnections.Count; i++)
		{
			FSMConnection fSMConnection = (FSMConnection)base.outConnections[i];
			ConditionTask condition = fSMConnection.condition;
			if (fSMConnection.isActive)
			{
				if ((condition != null && condition.CheckCondition(base.graphAgent, base.graphBlackboard)) || (condition == null && base.status != Status.Running))
				{
					FSM.EnterState((FSMState)fSMConnection.targetNode);
					fSMConnection.status = Status.Success;
					return true;
				}
				fSMConnection.status = Status.Failure;
			}
		}
		return false;
	}

	protected sealed override void OnReset()
	{
		base.status = Status.Resting;
		elapsedTime = 0f;
		for (int i = 0; i < base.outConnections.Count; i++)
		{
			if (((FSMConnection)base.outConnections[i]).condition != null)
			{
				((FSMConnection)base.outConnections[i]).condition.Disable();
			}
		}
		OnExit();
	}

	protected virtual void OnInit()
	{
	}

	protected virtual void OnEnter()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnExit()
	{
	}

	protected virtual void OnPause()
	{
	}
}
