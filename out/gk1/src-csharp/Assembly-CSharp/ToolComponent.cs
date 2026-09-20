using DarkTonic.MasterAudio;
using UnityEngine;

public class ToolComponent : WorldGameObjectComponent
{
	private const int ACTION_DELAY = 2;

	private WorldGameObject _target_obj;

	private int _action_delay;

	private bool _target_state_changed;

	private bool _is_driven_by_anim_event;

	private bool _playing_animation;

	private bool _is_using_tool;

	private bool _tried_to_stop;

	private bool _was_using_tool;

	private float _action_start_time = -1f;

	private ItemDefinition.ItemType _current_tool;

	public bool has_no_target => _target_obj == null;

	public bool playing_animation => _playing_animation;

	public override void StartComponent()
	{
		base.StartComponent();
		base.components.animated_behaviour.on_item_loop += OnItemLoop;
	}

	private void OnItemLoop(ItemDefinition.ItemType item_type, bool flag)
	{
		bool key = LazyInput.GetKey(GameKey.Work);
		Debug.Log("OnItemLoop, work_holded = " + key);
		if (item_type != ItemDefinition.ItemType.Sword)
		{
			_playing_animation = false;
			StopUsingTool(key, stop_working_really: false);
		}
	}

	public bool UseTool(bool placed_on_dock_point)
	{
		if (_action_delay > 0)
		{
			_action_delay--;
			return _is_using_tool;
		}
		if (!_is_driven_by_anim_event || !_target_state_changed)
		{
			UseCurrentTool(placed_on_dock_point);
		}
		if (_is_using_tool && !_playing_animation && base.components.character.anim_state == CharAnimState.Tool)
		{
			_playing_animation = true;
		}
		return _is_using_tool;
	}

	public void TryStop()
	{
		_was_using_tool = false;
		if (_is_using_tool)
		{
			_tried_to_stop = true;
			Item equippedTool = base.wgo.GetEquippedTool();
			ItemDefinition.ItemType itemType = equippedTool?.definition.type ?? ItemDefinition.ItemType.None;
			if (equippedTool == null || itemType == ItemDefinition.ItemType.Hand)
			{
				StopUsingTool();
				base.components.character.SetAnimationState(CharAnimState.Idle);
			}
		}
	}

	private void UseCurrentTool(bool placed_on_dock_point)
	{
		WorldGameObject worldGameObject = FindObjectForInteraction();
		if (worldGameObject != null)
		{
			if (worldGameObject.CheckIfDisabledInTutorial())
			{
				return;
			}
			if (worldGameObject.is_removing)
			{
				MainGame.me.build_mode_logics.ProcessRemovingCraft(worldGameObject, Time.deltaTime);
				_is_driven_by_anim_event = false;
				base.components.character.SetAnimationState(CharAnimState.Tool, ItemDefinition.ItemType.Hand);
				return;
			}
			if (worldGameObject.playing_disappearing_anim)
			{
				return;
			}
			CraftComponent craft = worldGameObject.components.craft;
			if (craft.enabled && !craft.IsCraftQueueEmpty() && !craft.is_crafting)
			{
				craft.TryStartCraftFromQueue(base.wgo.is_player);
				if (!craft.is_crafting && base.wgo.is_player)
				{
					base.components.character.ShowCustomNeedBubble("not_enough_resources");
				}
			}
			if (craft.enabled && (!craft.is_crafting || (craft.current_craft.is_auto && !craft.current_craft.hidden)))
			{
				return;
			}
		}
		_current_tool = base.wgo.GetEquippedToolType();
		if (worldGameObject != null && _current_tool == ItemDefinition.ItemType.None)
		{
			base.components.character.ShowNeededToolBubble(!MainGame.me.save.IsWorkAvailible(worldGameObject.obj_def));
			base.components.character.SetAnimationState(CharAnimState.Idle);
			StopUsingTool(work_holded: true);
			return;
		}
		if (_target_obj != null && _target_obj.CheckDisabledInteractions())
		{
			_target_obj = null;
			return;
		}
		if (_target_obj == null || _target_state_changed)
		{
			if (_tried_to_stop)
			{
				_target_state_changed = false;
				return;
			}
			_target_obj = FindObjectForInteraction();
			if (_target_obj == null)
			{
				if (placed_on_dock_point || !_was_using_tool)
				{
					_is_using_tool = false;
				}
				return;
			}
			if (_target_obj.CheckDisabledInteractions())
			{
				_target_obj = null;
				return;
			}
			_is_using_tool = true;
			_target_state_changed = false;
			Debug.Log("Set action start time, prev = " + _action_start_time);
			if (_action_start_time <= 0f)
			{
				_action_start_time = Time.time;
			}
			_is_driven_by_anim_event = ToolTypeDefinition.Get(_current_tool)?.driven_by_anim_event ?? false;
			base.components.character.SetAnimationState(CharAnimState.Tool, _current_tool);
		}
		PlayerComponent player = base.components.character.player;
		if (!player.IsEnoughEnergyToWork())
		{
			player.ShowNeedEnergyBubble();
			base.components.character.SetAnimationState(CharAnimState.Idle);
			StopUsingTool(work_holded: true);
			return;
		}
		if (_target_obj != null && !string.IsNullOrEmpty(worldGameObject.obj_def.work_sfx))
		{
			Sounds.PlaySound(worldGameObject.obj_def.work_sfx);
		}
		if (!_is_driven_by_anim_event && _target_obj != null)
		{
			_target_state_changed = _target_obj.DoAction(base.wgo);
			bool flag = _target_obj != null && _target_obj.components != null && _target_obj.components.craft != null && _target_obj.components.craft.is_crafting;
			_was_using_tool = true;
			if (_target_state_changed && !flag)
			{
				_action_delay = 2;
				_playing_animation = false;
				StopUsingTool(work_holded: false, stop_working_really: true, complete_work: true);
			}
		}
	}

	public void AnimationEventAction()
	{
		if (_is_driven_by_anim_event && !(_target_obj == null))
		{
			_was_using_tool = true;
			_target_state_changed = _target_obj.DoAction(base.wgo, Time.time - _action_start_time);
			Debug.Log("Set action start time #2, prev = " + _action_start_time);
			_action_start_time = Time.time;
			if (_target_state_changed)
			{
				_action_delay = 2;
			}
		}
	}

	public void FailAnimationEventAction()
	{
		if (_target_obj != null)
		{
			_target_obj.DoAnimAction();
		}
	}

	public void StopUsingTool(bool work_holded = false, bool stop_working_really = true, bool complete_work = false)
	{
		Debug.Log("StopUsingTool, work_holded = " + work_holded + ", stop_working_really = " + stop_working_really);
		if (stop_working_really)
		{
			base.wgo.components.character.dont_work_anymore = true;
		}
		_is_using_tool = false;
		if (_target_obj != null)
		{
			if (!string.IsNullOrEmpty(_target_obj.obj_def.work_sfx))
			{
				MasterAudio.StopAllOfSound(_target_obj.obj_def.work_sfx);
				if (complete_work && !string.IsNullOrEmpty(_target_obj.obj_def.work_end_sfx))
				{
					Sounds.PlaySound(_target_obj.obj_def.work_end_sfx);
				}
			}
			_target_obj.OnWorkFinished();
		}
		_target_obj = null;
		_tried_to_stop = (_target_state_changed = (_is_driven_by_anim_event = (_playing_animation = false)));
		if (!work_holded)
		{
			Debug.Log("Set action start time = -1, prev = " + _action_start_time);
			_action_start_time = -1f;
		}
		_current_tool = ItemDefinition.ItemType.None;
	}

	protected override int GetExecutionOrder()
	{
		return 4;
	}

	public override void UpdateEnableState(ObjectDefinition.ObjType obj_type)
	{
		base.enabled = base.wgo.is_player;
	}

	public void ResetLastActionTime()
	{
		_action_start_time = -1f;
	}
}
