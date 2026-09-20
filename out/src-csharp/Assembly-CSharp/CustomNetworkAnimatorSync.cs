using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomNetworkAnimatorSync : MonoBehaviour
{
	private Animator _animator;

	private bool _animator_inited;

	[NonSerialized]
	public SerializableAnimatorParameters stored_state = new SerializableAnimatorParameters();

	private const string DEBUG_ANIM_NAME = "worker_zombie";

	private Dictionary<string, bool> _states = new Dictionary<string, bool>();

	private const bool isLocalPlayer = true;

	private const bool isServer = true;

	public AnimatorControllerParameter[] parameters => _animator.parameters;

	public static CustomNetworkAnimatorSync InitAnimator(GameObject go)
	{
		CustomNetworkAnimatorSync obj = go.GetComponentInChildren<CustomNetworkAnimatorSync>() ?? go.AddComponent<CustomNetworkAnimatorSync>();
		obj.Init(go);
		return obj;
	}

	private void Init(GameObject go)
	{
		_animator = go.GetComponentInChildren<Animator>();
		if (_animator == null)
		{
			_animator = go.AddComponent<Animator>();
			_animator.enabled = false;
			Debug.LogError("Animator not found at game object: " + go.name, go);
		}
		_animator_inited = true;
	}

	public void SetInteger(string param, int v)
	{
		DoSetInteger(param, v);
		if (CustomNetworkManager.is_running)
		{
			RpcSetInteger(param, v);
		}
	}

	public void SetBool(string param, bool v)
	{
		DoSetBool(param, v);
		if (CustomNetworkManager.is_running)
		{
			RpcSetBool(param, v);
		}
	}

	public void SetFloat(string param, float v)
	{
		DoSetFloat(param, v);
		if (CustomNetworkManager.is_running)
		{
			RpcSetFloat(param, v);
		}
	}

	public void SetTrigger(string param)
	{
		DoSetTrigger(param);
		if (CustomNetworkManager.is_running)
		{
			RpcSetTrigger(param);
		}
	}

	public void ResetTrigger(string param)
	{
		DoResetTrigger(param);
		if (CustomNetworkManager.is_running)
		{
			RpcResetTrigger(param);
		}
	}

	public int GetInteger(string param)
	{
		return _animator.GetInteger(param);
	}

	public bool GetBool(string param)
	{
		return _animator.GetBool(param);
	}

	public bool ParamExists(string param_name)
	{
		if (_animator == null)
		{
			return false;
		}
		if (!_animator.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (_states.ContainsKey(param_name))
		{
			return _states[param_name];
		}
		if (!_animator.gameObject.activeSelf)
		{
			return false;
		}
		bool flag = false;
		AnimatorControllerParameter[] array = _animator.parameters;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name == param_name)
			{
				flag = true;
				break;
			}
		}
		_states.Add(param_name, flag);
		return flag;
	}

	private void CmdSetInteger(string param, int v)
	{
		DoSetInteger(param, v);
	}

	private void CmdSetBool(string param, bool v)
	{
		DoSetBool(param, v);
	}

	private void CmdSetFloat(string param, float v)
	{
		DoSetFloat(param, v);
	}

	private void CmdSetTrigger(string trigger)
	{
		DoSetTrigger(trigger);
	}

	private void CmdResetTrigger(string trigger)
	{
		DoResetTrigger(trigger);
	}

	private void RpcSetInteger(string param, int v)
	{
	}

	private void RpcSetBool(string param, bool v)
	{
	}

	private void RpcSetFloat(string param, float v)
	{
	}

	private void RpcSetTrigger(string trigger)
	{
	}

	private void RpcResetTrigger(string trigger)
	{
	}

	private void DoSetInteger(string param, int v)
	{
		stored_state.Set(param, v);
		if (ParamExists(param))
		{
			_animator.SetInteger(param, v);
		}
	}

	private void DoSetBool(string param, bool v)
	{
		stored_state.Set(param, v);
		if (ParamExists(param))
		{
			_animator.SetBool(param, v);
		}
	}

	private void DoSetFloat(string param, float v)
	{
		stored_state.Set(param, v);
		try
		{
			if (ParamExists(param))
			{
				_animator.SetFloat(param, v);
			}
		}
		catch (Exception)
		{
			Debug.LogError("Animator exception at object: " + base.gameObject.name, base.gameObject);
			throw;
		}
	}

	private void DoSetTrigger(string trigger)
	{
		if (ParamExists(trigger))
		{
			_animator.SetTrigger(trigger);
		}
	}

	private void DoResetTrigger(string trigger)
	{
		if (ParamExists(trigger))
		{
			_animator.ResetTrigger(trigger);
		}
	}

	public void DeserializeFromSavedState(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			stored_state = new SerializableAnimatorParameters();
			return;
		}
		stored_state = JsonUtility.FromJson<SerializableAnimatorParameters>(json);
		if (base.gameObject.activeInHierarchy)
		{
			OnEnable();
		}
	}

	private void DeserializeState(SerializableAnimatorParameters state)
	{
		foreach (SerializableAnimatorParameters.SerializableAnimatorParameter par in state.pars)
		{
			switch ((AnimatorControllerParameterType)par.param_type)
			{
			case AnimatorControllerParameterType.Float:
				_animator.SetFloat(par.param_name, par.float_v);
				break;
			case AnimatorControllerParameterType.Int:
				_animator.SetInteger(par.param_name, par.int_v);
				break;
			case AnimatorControllerParameterType.Bool:
				_animator.SetBool(par.param_name, par.bool_v);
				break;
			case AnimatorControllerParameterType.Trigger:
				if (par.trigger_v)
				{
					_animator.SetTrigger(par.param_name);
					par.trigger_v = false;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public void OnEnable()
	{
		if (stored_state == null)
		{
			return;
		}
		if (!_animator_inited || _animator == null)
		{
			WorldGameObject componentInParent = GetComponentInParent<WorldGameObject>();
			GameObject gameObject = null;
			if (componentInParent != null)
			{
				gameObject = componentInParent.gameObject;
			}
			else
			{
				WorldSimpleObject componentInParent2 = GetComponentInParent<WorldSimpleObject>();
				if (componentInParent2 != null)
				{
					gameObject = componentInParent2.gameObject;
				}
			}
			if (gameObject == null)
			{
				Debug.LogError("Couldn't init animator, " + base.name, this);
				return;
			}
			Init(gameObject);
		}
		DeserializeState(stored_state);
	}
}
