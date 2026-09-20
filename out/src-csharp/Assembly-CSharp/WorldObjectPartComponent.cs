using System;
using UnityEngine;

public class WorldObjectPartComponent : MonoBehaviour
{
	private WorldGameObject _obj;

	private WorldObjectPart _part;

	private ComponentsManager _components;

	private CustomNetworkAnimatorSync _animator;

	private BaseCharacterComponent _character;

	private bool _cached;

	public WorldGameObject wgo
	{
		get
		{
			if (!_cached)
			{
				Cache();
			}
			return _obj;
		}
	}

	public WorldObjectPart part
	{
		get
		{
			if (!_cached)
			{
				Cache();
			}
			return _part;
		}
	}

	public ComponentsManager components
	{
		get
		{
			if (!_cached)
			{
				Cache();
			}
			return _components;
		}
	}

	public CustomNetworkAnimatorSync animator
	{
		get
		{
			if (!_cached)
			{
				Cache();
			}
			return _animator;
		}
	}

	public BaseCharacterComponent character
	{
		get
		{
			if (!_cached)
			{
				Cache();
			}
			return _character;
		}
	}

	private void Cache()
	{
		_part = GetComponent<WorldObjectPart>();
		_obj = _part.parent;
		if (_obj == null)
		{
			_cached = false;
			WorldGameObject componentInParent = GetComponentInParent<WorldGameObject>();
			Debug.Log("Part has no WGO, is null = " + (componentInParent == null), this);
			throw new Exception();
		}
		_components = _obj.components;
		_animator = _components.animator;
		_character = _components.character;
		_cached = true;
	}

	public virtual void StartComponent()
	{
		Cache();
	}

	public virtual void UpdateComponent(float delta_time)
	{
	}

	public void ForceRecache()
	{
		_cached = false;
	}
}
