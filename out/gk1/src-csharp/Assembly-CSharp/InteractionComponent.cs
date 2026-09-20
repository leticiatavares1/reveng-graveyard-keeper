using System.Collections.Generic;
using UnityEngine;

public class InteractionComponent : WorldGameObjectComponent
{
	private const int OBJS_MASK = 66305;

	private const int DROPS_MASK = 16384;

	private const int OBJS_CHECK_FREQUENCY = 1;

	private const int DROPS_CHECK_FREQUENCY = 1;

	[HideInInspector]
	public DropResGameObject nearest_drop;

	private Vector3 rotation;

	private List<WorldGameObject> _collisions = new List<WorldGameObject>();

	private bool _is_initialized;

	private bool _nearest_has_action;

	private bool _nearest_has_interaction;

	private float _interaction_start_time;

	private Transform _collider_tf;

	private WorldGameObject _target_obj;

	private BoxCollider2D _collider;

	private DropsList _drops_list;

	private WorldGameObject _nearest;

	public static InteractionComponent last_interacted;

	public WorldGameObject nearest
	{
		get
		{
			return _nearest;
		}
		set
		{
			_nearest = value;
			_nearest_has_action = _nearest != null && _nearest.obj_def.tool_actions.action_tools.Count > 0;
			_nearest_has_interaction = _nearest != null && _nearest.obj_def.interaction_type != ObjectDefinition.InteractionType.None;
		}
	}

	public ObjectDefinition nearest_definition
	{
		get
		{
			if (!(_nearest != null))
			{
				return null;
			}
			return _nearest.obj_def;
		}
	}

	public bool nearest_has_action => _nearest_has_action;

	public bool nearest_has_interaction => _nearest_has_interaction;

	public bool has_no_target => _target_obj == null;

	public override void StartComponent()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		base.StartComponent();
		BoxCollider2D[] componentsInChildren = base.wgo.GetComponentsInChildren<BoxCollider2D>();
		foreach (BoxCollider2D boxCollider2D in componentsInChildren)
		{
			if (boxCollider2D.gameObject.layer == 8)
			{
				_collider = boxCollider2D;
				break;
			}
		}
		if (_collider != null)
		{
			_collider_tf = _collider.transform;
			_drops_list = DropsList.me;
			_is_initialized = true;
		}
		nearest = null;
		_collisions.Clear();
	}

	public void StopInteraction()
	{
		_interaction_start_time = 0f;
	}

	public bool Interact(bool interaction_start)
	{
		last_interacted = this;
		if (_interaction_start_time <= 0f)
		{
			_interaction_start_time = Time.time;
		}
		_target_obj = FindObjectForInteraction();
		if (_target_obj == null)
		{
			return false;
		}
		Debug.Log("Interact on = " + base.wgo.name + ", found target = " + _target_obj.name);
		MainGame.me.save.quests.CheckKeyQuests("interact_" + _target_obj.obj_id);
		Stats.DesignEvent("Interact:" + _target_obj.obj_id);
		_target_obj.Interact(base.wgo, interaction_start, Time.time - _interaction_start_time);
		return true;
	}

	public void FailAnimationEventAction()
	{
		if (_target_obj != null)
		{
			_target_obj.DoAnimAction();
		}
	}

	public override bool HasUpdate()
	{
		return true;
	}

	public override void UpdateComponent(float delta_time)
	{
		if (!_is_initialized)
		{
			return;
		}
		rotation.z = (int)base.components.character.anim_direction * 90;
		_collider_tf.eulerAngles = rotation;
		FindNearestInteractionObj();
		FindNearestDrop();
		_collisions.RemoveUnityNulls();
		_drops_list.SetHighlighted(nearest_drop);
		if (_collisions.Count == 0)
		{
			if (nearest != null)
			{
				nearest.UnprepareForInteraction();
			}
			nearest = null;
		}
		else
		{
			UpdateInteractionNearest();
		}
	}

	private void UpdateInteractionNearest()
	{
		WorldGameObject worldGameObject = FindCurrentInteractionNearest();
		if (worldGameObject == nearest || worldGameObject == null)
		{
			return;
		}
		nearest = worldGameObject;
		foreach (WorldGameObject collision in _collisions)
		{
			if (collision != nearest)
			{
				collision.UnprepareForInteraction();
			}
		}
		if (base.wgo.is_player)
		{
			base.components.character.wgo_hilighted_for_work = null;
			if (base.wgo.components.character.average_step > 0.01f && LazyInput.GetDirection().magnitude > 0f)
			{
				nearest = null;
			}
			else
			{
				nearest.PrepareForInteraction(base.components.character);
			}
		}
	}

	public void UpdateNearestHint()
	{
		if (nearest == null || !base.wgo.is_player || !MainGame.game_started)
		{
			return;
		}
		nearest.UnprepareForInteraction();
		nearest.PrepareForInteraction(base.components.character);
		InteractionBubbleGUI bubble = InteractionBubbleGUI.GetBubble(nearest.unique_id);
		if (bubble != null)
		{
			bubble.widget.alpha = 1f;
			TweenAlpha component = bubble.GetComponent<TweenAlpha>();
			if (component != null)
			{
				component.DestroyComponent();
			}
		}
	}

	private WorldGameObject FindCurrentInteractionNearest()
	{
		if (_collisions.Count <= 1)
		{
			if (_collisions.Count != 0)
			{
				return _collisions[0];
			}
			return null;
		}
		return GetGameObject(base.tf.localPosition, base.components.character.anim_direction);
	}

	public void OnObjectEnter(WorldGameObject collided_object)
	{
		if (!(collided_object == null) && !_collisions.Contains(collided_object))
		{
			_collisions.Add(collided_object);
		}
	}

	private void OnObjectExit(WorldGameObject collided_object)
	{
		if (collided_object == null)
		{
			_collisions.RemoveUnityNulls();
		}
		else if (_collisions.Contains(collided_object))
		{
			collided_object.UnprepareForInteraction();
			_collisions.Remove(collided_object);
		}
	}

	public WorldGameObject GetGameObject(Vector3 position, Direction direction)
	{
		if (_collisions.Count == 0)
		{
			return null;
		}
		int index = 0;
		float current = direction.ToVec().Atan2();
		float num = float.MaxValue;
		for (int i = 0; i < _collisions.Count; i++)
		{
			Vector2 v = (Vector2)_collisions[i].tf.position - base.wgo.pos;
			float target = v.Atan2();
			float num2 = Mathf.Abs(Mathf.DeltaAngle(current, target));
			float magnitude = v.magnitude;
			num2 += magnitude / 96f / 2f;
			if (i == 0 || num2 < num)
			{
				num = num2;
				index = i;
			}
		}
		return _collisions[index];
	}

	private void FindNearestInteractionObj()
	{
		if (Time.frameCount % 1 != 0)
		{
			return;
		}
		List<WorldGameObject> list = new List<WorldGameObject>();
		Collider2D[] array = Physics2D.OverlapAreaAll(_collider.bounds.min, _collider.bounds.max, 66305);
		bool flag = false;
		Collider2D[] array2 = array;
		foreach (Collider2D collider2D in array2)
		{
			if (collider2D.GetComponent<SkipInteraction>() != null)
			{
				continue;
			}
			WorldObjectPart component = collider2D.GetComponent<WorldObjectPart>();
			WorldGameObject worldGameObject = ((component == null) ? collider2D.GetComponentInParent<WorldGameObject>() : component.parent);
			if (worldGameObject == null)
			{
				continue;
			}
			ObjectDefinition obj_def = worldGameObject.obj_def;
			if (obj_def == null || obj_def.IsNotInteractive(worldGameObject))
			{
				continue;
			}
			if (collider2D.isTrigger && !flag)
			{
				if (list.Count > 0)
				{
					list.Clear();
				}
				flag = true;
			}
			if (!flag || collider2D.isTrigger)
			{
				list.Add(worldGameObject);
			}
		}
		for (int j = 0; j < _collisions.Count; j++)
		{
			if (!list.Contains(_collisions[j]))
			{
				OnObjectExit(_collisions[j]);
				j--;
			}
		}
		foreach (WorldGameObject item in list)
		{
			if (!_collisions.Contains(item))
			{
				OnObjectEnter(item);
			}
		}
	}

	private void FindNearestDrop()
	{
		if (Time.frameCount % 1 != 0)
		{
			return;
		}
		Collider2D[] array = Physics2D.OverlapAreaAll(_collider.bounds.min, _collider.bounds.max, 16384);
		nearest_drop = null;
		if (array.Length == 0)
		{
			return;
		}
		float num = float.MaxValue;
		Collider2D[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			DropResGameObject component = array2[i].GetComponent<DropResGameObject>();
			if (!(component == null) && !component.res.is_tech_point && component.res.definition != null && !component.res.definition.is_small && !(component.dist_sqr_to_player > num))
			{
				nearest_drop = component;
				num = component.dist_sqr_to_player;
			}
		}
	}

	public Item GetWorkToolTypeForNearest()
	{
		if (!(nearest == null))
		{
			return GetWorkToolTypeForObj(nearest);
		}
		return null;
	}

	public Item GetWorkToolTypeForObj(WorldGameObject wobj)
	{
		ObjectDefinition obj_def = wobj.obj_def;
		if (!MainGame.me.save.IsWorkAvailible(obj_def))
		{
			return null;
		}
		List<Item> list = new List<Item>();
		foreach (ItemDefinition.ItemType action_tool in obj_def.tool_actions.action_tools)
		{
			Item equippedTool = base.wgo.GetEquippedTool(action_tool);
			if (equippedTool != null)
			{
				list.Add(equippedTool);
			}
		}
		if (list.Count != 0 && MainGame.me.save.IsWorkAvailible(obj_def))
		{
			return list[0];
		}
		return null;
	}

	public bool CanWorkWith(WorldGameObject wobj)
	{
		return GetWorkToolTypeForObj(wobj) != null;
	}

	public override void UpdateEnableState(ObjectDefinition.ObjType obj_type)
	{
		base.enabled = base.wgo.is_player;
	}
}
