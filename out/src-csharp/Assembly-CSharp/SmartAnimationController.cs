using System;
using System.Collections.Generic;
using UnityEngine;

public class SmartAnimationController : MonoBehaviour
{
	public enum ObjectType
	{
		Character,
		Object
	}

	public enum AnimationDirectionLimit
	{
		Any,
		Down,
		Left,
		Right,
		Up
	}

	[Serializable]
	public struct TriggerGroup
	{
		public List<TriggerDefine> triggers;
	}

	[Serializable]
	public struct TriggerDefine
	{
		public string trigger;

		private List<string> _trigger_names_array => SmartAnimationController._trigger_names_array;
	}

	[Serializable]
	public class Animation
	{
		public string id;

		public string trigger;

		public AnimationDirectionLimit dir;

		public bool executed_with_trigger = true;

		public int min_period = 10;

		public int est_period = 30;

		[NonSerialized]
		public float cur_period;

		private List<string> _trigger_names_array => SmartAnimationController._trigger_names_array;

		private bool NoPeriods()
		{
			return executed_with_trigger;
		}
	}

	public ObjectType object_type;

	private string _object_type_message = "";

	[Space(10f)]
	public List<Animation> animations = new List<Animation>();

	private List<string> _state_names = new List<string>();

	private List<string> _trigger_names = new List<string>();

	private List<string> _param_names = new List<string>();

	private float _t;

	private const float REROLL_PERIOD = 1f;

	private Animator _cached_animator;

	private WorldGameObject _cached_wgo;

	public List<SmartAnimationController> linked_animators = new List<SmartAnimationController>();

	[NonSerialized]
	private static List<string> _trigger_names_array;

	[Header("Roll triggers on start")]
	[Space(10f)]
	public List<TriggerGroup> trigger_groups = new List<TriggerGroup>();

	private bool Editor_ValidateObjectType(ObjectType object_type)
	{
		_object_type_message = "";
		switch (object_type)
		{
		case ObjectType.Character:
			_object_type_message = "Character - animated object which can switch animations and don't roll triggers during cutscenes.";
			break;
		case ObjectType.Object:
			_object_type_message = "Object - non-character object, which animations don't depend on cutscenes, walking phase, etc.";
			break;
		}
		return true;
	}

	private void OnCustomInspectorGUI()
	{
		GJEditorAnimatorHelper.ScanAnimator(base.gameObject, ref _state_names, ref _trigger_names, ref _param_names);
		_trigger_names_array = _trigger_names;
	}

	public void Start()
	{
		foreach (TriggerGroup trigger_group in trigger_groups)
		{
			if (trigger_group.triggers != null && trigger_group.triggers.Count != 0)
			{
				int num = Mathf.FloorToInt(UnityEngine.Random.Range(0f, (float)trigger_group.triggers.Count - 0.001f));
				if (num >= trigger_group.triggers.Count)
				{
					num--;
				}
				CheckComponentsCache();
				_cached_animator.SetTrigger(trigger_group.triggers[num].trigger);
			}
		}
	}

	public void Update()
	{
		_t += Time.deltaTime;
		if (_t < 1f)
		{
			return;
		}
		_t = 1f;
		foreach (Animation animation in animations)
		{
			if (animation.executed_with_trigger)
			{
				continue;
			}
			animation.cur_period += _t;
			if (!(animation.cur_period < (float)animation.min_period))
			{
				float num = 1f / (float)animation.est_period;
				if (UnityEngine.Random.Range(0f, 1f) < num && (object_type != 0 || MainGame.me.player_char.control_enabled) && IsAnimationCanBeTriggered(animation))
				{
					animation.cur_period = 0f;
					_cached_animator.SetTrigger(animation.trigger);
				}
			}
		}
		_t = 0f;
	}

	private bool IsAnimationCanBeTriggered(Animation a)
	{
		CheckComponentsCache();
		if (object_type == ObjectType.Object)
		{
			return true;
		}
		BaseCharacterComponent character = _cached_wgo.components.character;
		if (character == null)
		{
			Debug.LogError("BaseCharacterComponent not found", this);
			return false;
		}
		if (character.anim_state != 0 && character.anim_state != CharAnimState.Disabled)
		{
			return false;
		}
		if (a.dir == AnimationDirectionLimit.Any)
		{
			return true;
		}
		Direction direction = character.direction.ToDirection();
		return a.dir switch
		{
			AnimationDirectionLimit.Down => direction == Direction.Down, 
			AnimationDirectionLimit.Left => direction == Direction.Left, 
			AnimationDirectionLimit.Right => direction == Direction.Right, 
			AnimationDirectionLimit.Up => direction == Direction.Up, 
			_ => true, 
		};
	}

	public void TriggerAimation(string smart_trigger_id)
	{
		CheckComponentsCache();
		bool flag = false;
		foreach (Animation animation in animations)
		{
			if (animation.id == smart_trigger_id)
			{
				flag = true;
				if (IsAnimationCanBeTriggered(animation))
				{
					animation.cur_period = 0f;
					_cached_animator.SetTrigger(animation.trigger);
				}
			}
		}
		if (!flag)
		{
			Debug.LogWarning("Smart trigger not found = " + smart_trigger_id, this);
		}
	}

	private void CheckComponentsCache()
	{
		if (!Application.isPlaying || _cached_animator == null)
		{
			_cached_animator = GetComponentInChildren<Animator>();
		}
		if (object_type == ObjectType.Character && (!Application.isPlaying || _cached_wgo == null))
		{
			_cached_wgo = GetComponentsInParent<WorldGameObject>(includeInactive: true)[0];
		}
	}

	public void TriggerOtherAnimation(int index, string triggers)
	{
		if (index >= linked_animators.Count)
		{
			Debug.LogError("TriggerOtherAnimation: Can't trigger animator #" + index + " because there's only " + linked_animators.Count + " in the list");
		}
		else
		{
			string[] array = triggers.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string smart_trigger_id in array)
			{
				linked_animators[index].TriggerAimation(smart_trigger_id);
			}
		}
	}

	public void TriggerOtherAnimation0(string smart_trigger)
	{
		TriggerOtherAnimation(0, smart_trigger);
	}

	public void TriggerOtherAnimation1(string smart_trigger)
	{
		TriggerOtherAnimation(1, smart_trigger);
	}

	public void TriggerOtherAnimation2(string smart_trigger)
	{
		TriggerOtherAnimation(2, smart_trigger);
	}

	public void TriggerOtherAnimation3(string smart_trigger)
	{
		TriggerOtherAnimation(3, smart_trigger);
	}

	public void TriggerOtherAnimation4(string smart_trigger)
	{
		TriggerOtherAnimation(4, smart_trigger);
	}
}
