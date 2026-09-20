using System;
using System.Collections.Generic;
using UnityEngine;

public class SSM
{
	private List<SSMState> states = new List<SSMState>();

	private Dictionary<Type, SSMState> statesByType = new Dictionary<Type, SSMState>();

	private readonly SSMState defaultState;

	private SSMState prevFStateBase;

	private SSMState curState;

	public SSMState CurState => curState;

	public SSM(SSMState defaultState)
	{
		this.defaultState = defaultState;
		AddState(defaultState);
	}

	public void AddState(SSMState state)
	{
		if (statesByType.TryAdd(state.GetType(), state))
		{
			states.Add(state);
		}
		else
		{
			Debug.LogError($"Can't add state: {state.GetType()}");
		}
	}

	public void ForceEnterState<T>() where T : SSMState
	{
		if (statesByType.TryGetValue(typeof(T), out var value))
		{
			curState = value;
			HandleActionChanged();
		}
	}

	public void CustomUpdate()
	{
		SSMState sSMState = null;
		foreach (SSMState state in states)
		{
			if (state != curState && state.CanEnter)
			{
				sSMState = state;
				break;
			}
		}
		if (curState == null || !curState.IsActive || sSMState != null)
		{
			curState = sSMState ?? defaultState;
		}
		if (!HandleActionChanged() && curState != null && curState.IsActive)
		{
			curState.Update();
		}
	}

	public void CustomFixedUpdate()
	{
		if (!HandleActionChanged() && curState != null && curState.IsActive)
		{
			curState.FixedUpdate();
		}
	}

	private bool HandleActionChanged()
	{
		bool num = prevFStateBase != curState;
		if (num)
		{
			prevFStateBase?.OnExit();
			if (prevFStateBase != null)
			{
				Debug.Log($"SSM.ExitState: {prevFStateBase.GetType()}");
			}
			prevFStateBase = curState;
			curState.OnEnter();
			Debug.Log($"SSM.EnterState: {curState.GetType()}");
		}
		return num;
	}
}
