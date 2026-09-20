using System;
using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace NodeCanvas;

[AddComponentMenu("NodeCanvas/Action List")]
public class ActionListPlayer : MonoBehaviour, ITaskSystem, ISerializationCallbackReceiver
{
	[SerializeField]
	private string _serializedList;

	[SerializeField]
	private List<UnityEngine.Object> _objectReferences;

	[NonSerialized]
	private ActionList _actionList;

	[SerializeField]
	private Blackboard _blackboard;

	public ActionList actionList => _actionList;

	Component ITaskSystem.agent => this;

	public IBlackboard blackboard
	{
		get
		{
			return _blackboard;
		}
		set
		{
			if (_blackboard != value)
			{
				_blackboard = (Blackboard)value;
				SendTaskOwnerDefaults();
			}
		}
	}

	public float elapsedTime => actionList.elapsedTime;

	UnityEngine.Object ITaskSystem.contextObject => this;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		_actionList = JSONSerializer.Deserialize<ActionList>(_serializedList, _objectReferences);
		if (_actionList == null)
		{
			_actionList = (ActionList)Task.Create(typeof(ActionList), this);
		}
	}

	public static ActionListPlayer Create()
	{
		return new GameObject("ActionList").AddComponent<ActionListPlayer>();
	}

	public void SendTaskOwnerDefaults()
	{
		actionList.SetOwnerSystem(this);
		foreach (ActionTask action in actionList.actions)
		{
			action.SetOwnerSystem(this);
		}
	}

	void ITaskSystem.SendEvent(EventData eventData)
	{
		Debug.LogWarning("Sending events to action lists has no effect");
	}

	void ITaskSystem.RecordUndo(string name)
	{
	}

	private void Awake()
	{
		SendTaskOwnerDefaults();
	}

	[ContextMenu("Play")]
	public void Play()
	{
		Play(this, blackboard, null);
	}

	public void Play(Action<bool> OnFinish)
	{
		Play(this, blackboard, OnFinish);
	}

	public void Play(Component agent, IBlackboard blackboard, Action<bool> OnFinish)
	{
		if (Application.isPlaying)
		{
			actionList.ExecuteAction(agent, blackboard, OnFinish);
		}
	}

	public Status ExecuteAction()
	{
		return actionList.ExecuteAction(this, blackboard);
	}

	public Status ExecuteAction(Component agent)
	{
		return actionList.ExecuteAction(agent, blackboard);
	}
}
