using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorldGameObjectComponent : WorldGameObjectComponentBase, IComparable<WorldGameObjectComponent>
{
	[NonSerialized]
	protected int _update_every_frame = 1;

	[NonSerialized]
	public bool destroyed;

	private float _update_delay;

	private float _left_update_delay;

	private static Dictionary<Type, object> _lists = new Dictionary<Type, object>();

	private Rigidbody2D _body;

	private bool _body_cached;

	private CustomNetworkAnimatorSync _animator;

	private bool _animator_cached;

	public Rigidbody2D body
	{
		get
		{
			if (_body_cached)
			{
				return _body;
			}
			_body_cached = true;
			_body = base.go.GetComponent<Rigidbody2D>();
			return _body;
		}
	}

	public virtual void StartComponent()
	{
		_update_delay = ((_update_every_frame <= 1) ? 0f : ((float)_update_every_frame / 60f));
		started = true;
	}

	public virtual void LateUpdateComponent()
	{
	}

	public virtual void UpdateComponent(float delta_time)
	{
	}

	public virtual void FixedUpdateComponent(float delta_time)
	{
	}

	public virtual bool HasFixedUpdate()
	{
		return false;
	}

	public virtual bool HasUpdate()
	{
		return false;
	}

	public virtual bool HasLateUpdate()
	{
		return false;
	}

	public virtual bool DoAction(WorldGameObject other_obj, float delta_time, bool for_gratitude_points = false)
	{
		return false;
	}

	public virtual bool Interact(WorldGameObject other_obj, float delta_time)
	{
		return false;
	}

	public virtual void PrepareForInteraction(BaseCharacterComponent for_whom)
	{
	}

	public virtual void UnprepareForInteraction()
	{
	}

	protected bool DelayedUpdate(float delta_time)
	{
		if (_update_every_frame <= 1)
		{
			return false;
		}
		_left_update_delay -= delta_time;
		if (_left_update_delay > 0f)
		{
			return true;
		}
		_left_update_delay = _update_delay;
		return false;
	}

	public int CompareTo(WorldGameObjectComponent other)
	{
		if (GetExecutionOrder() == other.GetExecutionOrder())
		{
			return 0;
		}
		if (GetExecutionOrder() <= other.GetExecutionOrder())
		{
			return -1;
		}
		return 1;
	}

	public virtual void InitComponent()
	{
	}

	protected virtual int GetExecutionOrder()
	{
		return 0;
	}

	protected WorldGameObject FindObjectForInteraction()
	{
		return base.components.interaction.nearest;
	}

	public void RedrawCurrentInteractiveHint()
	{
		if (MainGame.game_started)
		{
			Debug.Log("RedrawCurrentInteractiveHint");
			WorldGameObject worldGameObject = FindObjectForInteraction();
			if (worldGameObject != null)
			{
				worldGameObject.RedrawBubble();
			}
		}
	}

	public virtual void OnEnable()
	{
	}

	public virtual void OnDisable()
	{
	}

	public virtual void UpdateEnableState(ObjectDefinition.ObjType obj_type)
	{
		base.enabled = true;
	}

	public virtual void RefreshComponentBubbleData(bool show_interaction_buttons)
	{
	}
}
