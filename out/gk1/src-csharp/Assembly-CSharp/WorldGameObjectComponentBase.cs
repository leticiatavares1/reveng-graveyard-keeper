using UnityEngine;

public abstract class WorldGameObjectComponentBase
{
	private bool _enabled;

	private Transform _tf;

	private WorldGameObject _wgo;

	private GameObject _go;

	private bool _cached_tf;

	private long _unique_id = -1L;

	private string _type_name = "";

	private bool _type_name_set;

	protected bool started;

	public bool enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			if (_enabled != value)
			{
				_enabled = value;
				if (_enabled)
				{
					OnEnabled();
				}
				else
				{
					OnDisabled();
				}
			}
		}
	}

	public ComponentsManager components => _wgo.components;

	public WorldGameObject wgo => _wgo;

	protected GameObject go => _go;

	public Transform tf
	{
		get
		{
			if (!_cached_tf)
			{
				_tf = _go.transform;
				_cached_tf = true;
			}
			return _tf;
		}
	}

	public virtual void Init(WorldGameObject wobj)
	{
		_cached_tf = _wgo != null && _tf != null;
		if (!_cached_tf)
		{
			_wgo = wobj;
			if (_wgo == null)
			{
				_cached_tf = false;
				_tf = null;
			}
			else
			{
				_go = wobj.gameObject;
				_cached_tf = true;
				_tf = _wgo.transform;
			}
			enabled = true;
		}
	}

	protected virtual void OnEnabled()
	{
	}

	protected virtual void OnDisabled()
	{
	}

	protected void ForceSetGameObjectLinks(WorldGameObject wgo, GameObject go = null)
	{
		_wgo = wgo;
		_go = ((go == null) ? wgo.gameObject : go);
	}

	public WorldGameObjectComponentBase()
	{
		_unique_id = UniqueID.GetUniqueID();
	}

	public long GetInstanceID()
	{
		return _unique_id;
	}

	public void SetTf(Transform tf)
	{
		_tf = tf;
		_cached_tf = true;
	}

	public string GetTypeName()
	{
		if (!_type_name_set)
		{
			_type_name_set = true;
			_type_name = GetType().Name;
		}
		return _type_name;
	}

	public void BeginProfilerSample()
	{
	}
}
