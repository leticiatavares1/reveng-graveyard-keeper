using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAnimationsOnColliderTrigger : MonoBehaviour
{
	[Serializable]
	public class TriggerAnimationDefinition
	{
		[SerializeField]
		private string trigger;

		private List<string> _trigger_names_array => TriggerAnimationsOnColliderTrigger._trigger_names_array;

		public void TrySetTrigger(Animator animator)
		{
			if (!string.IsNullOrEmpty(trigger))
			{
				animator.SetTrigger(trigger);
			}
		}

		public void TryResetTrigger(Animator animator)
		{
			if (!string.IsNullOrEmpty(trigger))
			{
				animator.ResetTrigger(trigger);
			}
		}
	}

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	[Space]
	private TriggerAnimationDefinition _on_enter;

	[SerializeField]
	private TriggerAnimationDefinition _on_exit;

	private static List<string> _trigger_names_array;

	private List<string> _state_names = new List<string>();

	private List<string> _trigger_names = new List<string>();

	private List<string> _param_names = new List<string>();

	private bool _is_player_inside;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (IsAnimatorSet())
		{
			WorldGameObject componentInParent = other.gameObject.GetComponentInParent<WorldGameObject>();
			if (!(componentInParent == null) && componentInParent.is_player && !_is_player_inside)
			{
				_is_player_inside = true;
				_on_enter.TrySetTrigger(_animator);
				_on_exit.TryResetTrigger(_animator);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (IsAnimatorSet())
		{
			WorldGameObject componentInParent = other.gameObject.GetComponentInParent<WorldGameObject>();
			if (!(componentInParent == null) && componentInParent.is_player && _is_player_inside)
			{
				_is_player_inside = false;
				_on_exit.TrySetTrigger(_animator);
				_on_enter.TryResetTrigger(_animator);
			}
		}
	}

	private void OnCustomInspectorGUI()
	{
		GJEditorAnimatorHelper.ScanAnimator(_animator.gameObject, ref _state_names, ref _trigger_names, ref _param_names);
		_trigger_names_array = _trigger_names;
	}

	private bool IsAnimatorSet()
	{
		if (_animator == null)
		{
			Debug.LogError("Animator not set!", this);
			return false;
		}
		return true;
	}
}
