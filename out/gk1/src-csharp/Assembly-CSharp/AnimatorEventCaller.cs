using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimatorEventCaller : MonoBehaviour
{
	public enum AnimatorParamType
	{
		Integer,
		Trigger,
		Bool,
		Float
	}

	[Serializable]
	public class AnimatorEventCallerAtom
	{
		public Animator animator;

		public AnimatorParamType anim_type;

		public string anim_param_name;

		public int anim_int;

		public bool anim_bool;

		public float anim_float;
	}

	public List<AnimatorEventCallerAtom> event_caller_atoms = new List<AnimatorEventCallerAtom>();

	public void SetTrigger_1()
	{
		if (event_caller_atoms == null || event_caller_atoms.Count < 1)
		{
			Debug.LogError("SetTrigger_1 error!");
		}
		else
		{
			CallTrigger(event_caller_atoms[0]);
		}
	}

	public void SetTrigger_2()
	{
		if (event_caller_atoms == null || event_caller_atoms.Count < 1)
		{
			Debug.LogError("SetTrigger_2 error!");
		}
		else
		{
			CallTrigger(event_caller_atoms[1]);
		}
	}

	public void SetTrigger_3()
	{
		if (event_caller_atoms == null || event_caller_atoms.Count < 1)
		{
			Debug.LogError("SetTrigger_3 error!");
		}
		else
		{
			CallTrigger(event_caller_atoms[2]);
		}
	}

	public void SetTrigger_4()
	{
		if (event_caller_atoms == null || event_caller_atoms.Count < 1)
		{
			Debug.LogError("SetTrigger_4 error!");
		}
		else
		{
			CallTrigger(event_caller_atoms[3]);
		}
	}

	public void SetTrigger_5()
	{
		if (event_caller_atoms == null || event_caller_atoms.Count < 1)
		{
			Debug.LogError("SetTrigger_5 error!");
		}
		else
		{
			CallTrigger(event_caller_atoms[4]);
		}
	}

	public void SetTrigger_6()
	{
		if (event_caller_atoms == null || event_caller_atoms.Count < 1)
		{
			Debug.LogError("SetTrigger_6 error!");
		}
		else
		{
			CallTrigger(event_caller_atoms[5]);
		}
	}

	public void SetTrigger_7()
	{
		if (event_caller_atoms == null || event_caller_atoms.Count < 1)
		{
			Debug.LogError("SetTrigger_7 error!");
		}
		else
		{
			CallTrigger(event_caller_atoms[6]);
		}
	}

	public void SetTrigger_8()
	{
		if (event_caller_atoms == null || event_caller_atoms.Count < 1)
		{
			Debug.LogError("SetTrigger_8 error!");
		}
		else
		{
			CallTrigger(event_caller_atoms[7]);
		}
	}

	private void CallTrigger(AnimatorEventCallerAtom caller_atom)
	{
		if (caller_atom.animator == null)
		{
			Debug.LogError("CallTrigger error: animator is null!");
			return;
		}
		switch (caller_atom.anim_type)
		{
		case AnimatorParamType.Integer:
			caller_atom.animator.SetInteger(caller_atom.anim_param_name, caller_atom.anim_int);
			break;
		case AnimatorParamType.Trigger:
			caller_atom.animator.SetTrigger(caller_atom.anim_param_name);
			break;
		case AnimatorParamType.Bool:
			caller_atom.animator.SetBool(caller_atom.anim_param_name, caller_atom.anim_bool);
			break;
		case AnimatorParamType.Float:
			caller_atom.animator.SetFloat(caller_atom.anim_param_name, caller_atom.anim_float);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
