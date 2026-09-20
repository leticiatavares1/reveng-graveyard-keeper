using System;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D;

public abstract class BasePC2D : MonoBehaviour
{
	public ProCamera2D ProCamera2D;

	protected Func<Vector3, float> Vector3H;

	protected Func<Vector3, float> Vector3V;

	protected Func<Vector3, float> Vector3D;

	protected Func<float, float, Vector3> VectorHV;

	protected Func<float, float, float, Vector3> VectorHVD;

	protected Transform _transform;

	private bool _enabled;

	protected virtual void Awake()
	{
		_transform = base.transform;
		if (ProCamera2D == null && Camera.main != null)
		{
			ProCamera2D = Camera.main.GetComponent<ProCamera2D>();
		}
		else if (ProCamera2D == null)
		{
			ProCamera2D = UnityEngine.Object.FindObjectOfType(typeof(ProCamera2D)) as ProCamera2D;
		}
		if (ProCamera2D == null)
		{
			Debug.LogWarning(GetType().Name + ": ProCamera2D not found! Please add the ProCamera2D.cs component to your main camera.");
			return;
		}
		if (base.enabled)
		{
			Enable();
		}
		ResetAxisFunctions();
	}

	protected virtual void OnEnable()
	{
		Enable();
	}

	protected virtual void OnDisable()
	{
		Disable();
	}

	protected virtual void OnDestroy()
	{
		Disable();
	}

	public virtual void OnReset()
	{
	}

	private void Enable()
	{
		if (!_enabled && !(ProCamera2D == null))
		{
			_enabled = true;
			ProCamera2D proCamera2D = ProCamera2D;
			proCamera2D.OnReset = (Action)Delegate.Combine(proCamera2D.OnReset, new Action(OnReset));
		}
	}

	private void Disable()
	{
		if (ProCamera2D != null && _enabled)
		{
			_enabled = false;
			ProCamera2D proCamera2D = ProCamera2D;
			proCamera2D.OnReset = (Action)Delegate.Remove(proCamera2D.OnReset, new Action(OnReset));
		}
	}

	private void ResetAxisFunctions()
	{
		if (Vector3H != null)
		{
			return;
		}
		switch (ProCamera2D.Axis)
		{
		case MovementAxis.XY:
			Vector3H = (Vector3 vector) => vector.x;
			Vector3V = (Vector3 vector) => vector.y;
			Vector3D = (Vector3 vector) => vector.z;
			VectorHV = (float h, float v) => new Vector3(h, v, 0f);
			VectorHVD = (float h, float v, float d) => new Vector3(h, v, d);
			break;
		case MovementAxis.XZ:
			Vector3H = (Vector3 vector) => vector.x;
			Vector3V = (Vector3 vector) => vector.z;
			Vector3D = (Vector3 vector) => vector.y;
			VectorHV = (float h, float v) => new Vector3(h, 0f, v);
			VectorHVD = (float h, float v, float d) => new Vector3(h, d, v);
			break;
		case MovementAxis.YZ:
			Vector3H = (Vector3 vector) => vector.z;
			Vector3V = (Vector3 vector) => vector.y;
			Vector3D = (Vector3 vector) => vector.x;
			VectorHV = (float h, float v) => new Vector3(0f, v, h);
			VectorHVD = (float h, float v, float d) => new Vector3(d, v, h);
			break;
		}
	}
}
