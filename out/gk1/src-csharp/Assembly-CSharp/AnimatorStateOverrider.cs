using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class AnimatorStateOverrider : MonoBehaviour
{
	[SerializeField]
	public List<AnimatorStateOverriderAtom> parameters_list = new List<AnimatorStateOverriderAtom>();

	private Animator _source_animator;

	private List<Animator> _destination_animators = new List<Animator>();

	public void Awake()
	{
		_source_animator = GetComponent<Animator>();
		foreach (Animator item in GetComponentsInChildren<Animator>(includeInactive: true).ToList())
		{
			if (item != _source_animator)
			{
				_destination_animators.Add(item);
			}
		}
	}

	public void Update()
	{
		if (_source_animator != null && _destination_animators.Count != 0)
		{
			foreach (AnimatorStateOverriderAtom item in parameters_list)
			{
				string source_state_name = item.source_state_name;
				string destination_state_name = item.destination_state_name;
				switch (item.animator_source_state_type)
				{
				case AnimatorStateOverriderAtom.AnimatorStates.BOOL:
				{
					bool @bool = _source_animator.GetBool(source_state_name);
					foreach (Animator destination_animator in _destination_animators)
					{
						if (destination_animator != null)
						{
							destination_animator.SetBool(destination_state_name, @bool);
						}
					}
					break;
				}
				case AnimatorStateOverriderAtom.AnimatorStates.FLOAT:
				{
					float @float = _source_animator.GetFloat(source_state_name);
					foreach (Animator destination_animator2 in _destination_animators)
					{
						if (destination_animator2 != null)
						{
							destination_animator2.SetFloat(destination_state_name, @float);
						}
					}
					break;
				}
				case AnimatorStateOverriderAtom.AnimatorStates.INT:
				{
					int integer = _source_animator.GetInteger(source_state_name);
					foreach (Animator destination_animator3 in _destination_animators)
					{
						if (destination_animator3 != null)
						{
							destination_animator3.SetInteger(destination_state_name, integer);
						}
					}
					break;
				}
				}
			}
			return;
		}
		Debug.LogError("AnimatorStateOverrider:Update, some problems with source or(and) destination animators: source is null(" + _source_animator == "), destinationanimtaors count(" + _destination_animators.Count + ")");
	}
}
