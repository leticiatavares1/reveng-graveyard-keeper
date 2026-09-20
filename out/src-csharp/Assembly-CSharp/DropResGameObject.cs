using System;
using System.Collections.Generic;
using DG.Tweening;
using DLCRefugees;
using DarkTonic.MasterAudio;
using UnityEngine;

public class DropResGameObject : MonoBehaviour
{
	private const float K = 96f;

	private const float DROP_OFFSET = 28.800001f;

	private const float START_COL_RADIUS = 0.001f;

	private const float COL_RADIUS_STEP = 0.005f;

	private const float NOT_MERGING_DROPS_TIME = 3f;

	private const float MERGE_RADIUS = 1f;

	private const int POSSIBLE_MASK = 513;

	private const int WALLS_MASK = 1;

	[Range(0f, 1f)]
	public float position_randomization;

	public Item res = new Item();

	public SpriteRenderer sprite;

	public SpriteRenderer sprite_x2_item;

	public SpriteRenderer sprite_res_item;

	[Header("Logic setup")]
	[Range(0f, 5f)]
	public float speed;

	[Range(0f, 1f)]
	public float disable_body_dist;

	[Range(0f, 1f)]
	public float collect_delay;

	[Header("Bounce setup")]
	[Range(0f, 2f)]
	public float bounce_time;

	[Range(0f, 2f)]
	public float bounce_height;

	public DropResCurve[] anim_curves;

	[Header("Shadow setup")]
	public Transform shdw_cont_x2;

	public SpriteRenderer shdw_spr_x1;

	public SpriteRenderer shdw_spr_x2;

	[Range(0f, 2f)]
	public float shadow_size_k;

	[Range(0f, 2f)]
	public float shadow_alfa_k;

	[Space]
	public bool is_collected;

	private CircleCollider2D _collider;

	private Rigidbody2D _body;

	private WorldGameObject _target_obj;

	private Transform _target_char_tf;

	private Transform _tf;

	private Transform _sprite_tf;

	private Transform _shadow_tf;

	private SpriteRenderer _shadow_renderer;

	private DropResCurve _chosen_curve;

	private float _last_dist_to_target;

	private float _drop_time;

	private float _disable_body_dist_sqr;

	private float _col_radius;

	private float _try_do_merge_after;

	private Color _shadow_color = Color.white;

	private bool _in_player_radius;

	private bool _just_spawned;

	private bool _do_merge_done;

	private KickComponent _kick_component;

	public float dist_sqr_to_player;

	public static DropResGameObject currently_higlighted_obj;

	public GameObject go_small_drop;

	public GameObject go_x2_item;

	public Color c_hilighted = Color.yellow;

	public Color c_normal = Color.magenta;

	public bool small_collider_at_start = true;

	private DropResHint _linked_hint;

	private bool _performing_drop_and_fly;

	private Vector2 _dest_global_pos = Vector2.zero;

	private bool _flying_without_collider;

	[NonSerialized]
	public Transform object_transform;

	private bool _is_moving;

	private Vector2 _move_dir = Vector2.zero;

	[NonSerialized]
	public float dist_to_player = 9999f;

	public SpriteText stack_label;

	public string zone_id = "";

	[NonSerialized]
	public float dist = 999999f;

	public Vector3 pos => _tf.position;

	public bool has_target => _target_obj != null;

	public CircleCollider2D col => _collider ?? (_collider = GetComponent<CircleCollider2D>());

	public float bounce_curve_factor => _chosen_curve.duration_factor;

	public float collider_radius => _col_radius;

	public KickComponent kick_component
	{
		get
		{
			if (_kick_component == null)
			{
				_kick_component = new KickComponent();
				_kick_component.Init(null);
				_kick_component.SetDropResGameObject(this);
			}
			return _kick_component;
		}
	}

	public static void Drop(Vector3 pos, List<Item> res, Transform tf, Direction direction = Direction.None)
	{
		foreach (Item re in res)
		{
			Drop(pos, re, tf, direction);
		}
	}

	public static DropResGameObject DropAndFly(Vector3 pos, Item item, Transform parent, Vector2 dest_global_pos, bool fly_without_collider = false)
	{
		if (item == null || item.IsEmpty())
		{
			return null;
		}
		if (item.value > 1)
		{
			for (int i = 0; i < item.value; i++)
			{
				DropAndFly(pos, new Item(item)
				{
					value = 1
				}, parent, dest_global_pos, fly_without_collider);
			}
			return null;
		}
		if (item.is_tech_point)
		{
			Debug.LogError("Can't DropAndFly tech points");
			return null;
		}
		DropResGameObject dropResGameObject = Prefabs.me.drop_res_game_object.Copy(parent, activate: true, "Drop item " + item.id + ", v=" + item.value);
		dropResGameObject.transform.position = pos;
		DropsList.me.Add(dropResGameObject);
		WorldMap.OnNewDropItem(item);
		dropResGameObject.DoDrop(item, -1, do_bounce: false);
		_ = (Vector2)(MainGame.me.world_root.worldToLocalMatrix * dest_global_pos);
		dropResGameObject.SetFlyDestination(dest_global_pos, fly_without_collider);
		return dropResGameObject;
	}

	public static DropResGameObject Drop(Vector3 pos, Item item, Transform parent, Direction direction = Direction.None, float force_factor = 1f, int selected_curve = -1, bool check_walls = true, bool force_stacked_drop = false)
	{
		if (item == null || item.IsEmpty())
		{
			return null;
		}
		if (item.is_tech_point)
		{
			TechPointsDrop.Drop(pos, item);
			return null;
		}
		if (item.definition != null && item.definition.item_replace != null && MainGame.me.player.GetParamInt(item.definition.item_replace.player_flag) > 0)
		{
			item = new Item(item.definition.item_replace.replace_id, item.value);
		}
		if (item.worker_unique_id > 0)
		{
			Worker worker = item.worker;
			if (worker == null)
			{
				Debug.LogError("Wroker is null!");
			}
			else
			{
				item = worker.GetOnGroundItem();
			}
		}
		if (item.definition != null)
		{
			string run_script_after_drop = item.definition.run_script_after_drop;
			if (!string.IsNullOrEmpty(run_script_after_drop))
			{
				GS.RunFlowScript(run_script_after_drop);
			}
			if (item.definition.destroy_after_drop)
			{
				return null;
			}
		}
		if (item.value > 1)
		{
			if ((item.definition != null && item.value > 5 && item.definition.item_size == 1) || force_stacked_drop)
			{
				int num = item.value;
				DropResGameObject result = null;
				while (num > 0)
				{
					int num2 = item.value;
					if (item.definition.stack_count > 0 && num2 > item.definition.stack_count)
					{
						num2 = item.definition.stack_count;
					}
					num -= num2;
					result = DoDrop(pos, new Item(item)
					{
						value = num2
					}, parent, direction);
				}
				return result;
			}
			for (int i = 0; i < item.value; i++)
			{
				DoDrop(pos, new Item(item)
				{
					value = 1
				}, parent, direction);
			}
			return null;
		}
		return DoDrop(pos, item, parent, direction, force_factor, selected_curve, check_walls);
	}

	private static DropResGameObject DoDrop(Vector3 pos, Item item, Transform parent, Direction direction = Direction.None, float force_factor = 1f, int selected_curve = -1, bool check_walls = true)
	{
		DropResGameObject dropResGameObject = Prefabs.me.drop_res_game_object.Copy(parent, activate: true, "Drop item " + item.id + ", v=" + item.value);
		dropResGameObject.transform.position = pos;
		dropResGameObject.DoDrop(item, selected_curve, direction != Direction.IgnoreDirection);
		DropsList.me.Add(dropResGameObject);
		WorldMap.OnNewDropItem(item);
		if (item.definition != null && item.definition.type == ItemDefinition.ItemType.Body)
		{
			dropResGameObject._linked_hint = DropResHint.Show(dropResGameObject, show_durability: true);
			Sounds.PlaySound("item_2h_drop", pos);
		}
		WorldZone zoneOfPoint = WorldZone.GetZoneOfPoint(pos);
		dropResGameObject.zone_id = ((zoneOfPoint == null) ? string.Empty : zoneOfPoint.id);
		if (dropResGameObject.res != null)
		{
			dropResGameObject.res.drop_zone_id = dropResGameObject.zone_id;
		}
		KickComponent kickComponent = dropResGameObject.kick_component;
		if (kickComponent == null)
		{
			return dropResGameObject;
		}
		if (item.definition != null && item.definition.is_big)
		{
			kickComponent.active = false;
		}
		if (direction == Direction.None)
		{
			Vector3 vector = dropResGameObject.RandPos();
			bool flag = false;
			if (check_walls)
			{
				flag = IsOverlapingSomething(pos + vector, Vector3.zero, 0.01f);
				if (flag)
				{
					int num = 5;
					while (flag && num > 0)
					{
						num--;
						vector = dropResGameObject.RandPos();
						flag = IsOverlapingSomething(pos + vector, Vector3.zero, 0.01f);
					}
				}
			}
			if (!flag)
			{
				dropResGameObject.transform.position = pos + vector;
			}
			kickComponent.Kick(vector, dropResGameObject.bounce_curve_factor, dropResGameObject.OnKickedDropStoped);
			return dropResGameObject;
		}
		Vector3 vector2 = direction.ToVec3() * 28.800001f;
		if (check_walls)
		{
			bool flag2 = IsOverlapingSomething(pos, vector2, dropResGameObject.collider_radius);
			if (flag2)
			{
				int num2 = 3;
				while (flag2 && num2 > 0)
				{
					direction = direction.ClockwiseDir();
					vector2 = direction.ToVec3() * 28.800001f;
					flag2 = IsOverlapingSomething(pos, vector2, dropResGameObject.collider_radius);
					num2--;
				}
				if (flag2)
				{
					direction = direction.ClockwiseDir().Opposite();
					vector2 = direction.ToVec3() * 28.800001f;
				}
			}
		}
		dropResGameObject.transform.position += vector2;
		if (direction == Direction.IgnoreDirection)
		{
			dropResGameObject._chosen_curve = null;
		}
		else
		{
			kickComponent.Kick(vector2, force_factor * dropResGameObject.bounce_curve_factor, dropResGameObject.OnKickedDropStoped);
		}
		return dropResGameObject;
	}

	private void OnKickedDropStoped()
	{
		if (!has_target && col.isTrigger)
		{
			col.isTrigger = false;
		}
	}

	private Vector3 RandPos()
	{
		return new Vector3(UnityEngine.Random.Range(-96f, 96f), UnityEngine.Random.Range(-96f, 96f)) * position_randomization;
	}

	private static bool IsOverlapingSomething(Vector3 pos, Vector3 dir, float radius)
	{
		return Physics2D.OverlapCircleAll(pos + dir / 2f, radius, 1).Length != 0;
	}

	private void DoDrop(Item drop_item, int selected_curve = -1, bool do_bounce = true)
	{
		res = drop_item;
		ItemDefinition dataOrNull = GameBalance.me.GetDataOrNull<ItemDefinition>(drop_item.id);
		bool flag = false;
		if (dataOrNull != null)
		{
			flag = dataOrNull.item_size >= 2;
		}
		sprite.gameObject.SetActive(value: true);
		go_small_drop.SetActive(!flag);
		go_x2_item.SetActive(flag);
		GetComponent<CircleCollider2D>().enabled = !flag;
		GetComponent<CapsuleCollider2D>().enabled = flag;
		if (stack_label != null)
		{
			stack_label.SetActive(active: false);
		}
		if (flag)
		{
			string text = drop_item.GetIcon();
			if (text.Contains("body"))
			{
				text = "i_body";
			}
			sprite_x2_item.sprite = EasySpritesCollection.GetSprite(text);
			object_transform = sprite_x2_item.transform;
		}
		else
		{
			sprite.sprite = EasySpritesCollection.GetSprite("d_" + drop_item.id, not_found_is_valid: true);
			if (sprite_res_item != null)
			{
				if (sprite.sprite == null)
				{
					sprite_res_item.gameObject.SetActive(value: true);
					sprite.gameObject.SetActive(value: false);
					string sprite_name = ((dataOrNull == null) ? ("i_" + drop_item.id) : dataOrNull.GetIcon());
					sprite_res_item.sprite = EasySpritesCollection.GetSprite(sprite_name);
				}
				else
				{
					sprite_res_item.gameObject.SetActive(value: false);
				}
			}
			object_transform = sprite_res_item.transform;
			if (drop_item.value > 1 && stack_label != null)
			{
				stack_label.SetActive(active: true);
				stack_label.SetText(drop_item.value.ToString() ?? "");
			}
		}
		is_collected = false;
		_tf = base.transform;
		_sprite_tf = (flag ? sprite_x2_item.transform : sprite.transform);
		_shadow_tf = (flag ? shdw_cont_x2 : shdw_spr_x1.transform);
		_shadow_renderer = (flag ? shdw_spr_x2 : shdw_spr_x1);
		_disable_body_dist_sqr = disable_body_dist * disable_body_dist;
		if (selected_curve >= 0)
		{
			_chosen_curve = anim_curves[selected_curve];
		}
		else
		{
			_chosen_curve = ((anim_curves.Length == 0) ? null : anim_curves[UnityEngine.Random.Range(0, anim_curves.Length)]);
		}
		if (_chosen_curve != null)
		{
			col.isTrigger = true;
			_col_radius = col.radius;
			if (small_collider_at_start)
			{
				col.radius = 0.001f;
			}
			_just_spawned = true;
			_body = GetComponent<Rigidbody2D>();
			ChangeKickableState(now_kickable: true);
			if (do_bounce)
			{
				EasyTimer.Add(collect_delay, StopBounce);
				PlayBounce();
				_drop_time = 0f;
			}
			_do_merge_done = false;
			_try_do_merge_after = 3f;
		}
	}

	public void RedrawStackCounter()
	{
		if (!(stack_label == null))
		{
			stack_label.SetActive(res.value > 1);
			stack_label.SetText(res.value.ToString() ?? "");
		}
	}

	public void UpdateMe()
	{
		collect_delay -= Time.deltaTime;
		if (_is_moving && !is_collected)
		{
			_tf.position += (Vector3)_move_dir * Time.deltaTime;
		}
		if (!_do_merge_done)
		{
			_try_do_merge_after -= Time.deltaTime;
			if (_try_do_merge_after < 0f)
			{
				DoTryMerging();
			}
		}
		if (!_performing_drop_and_fly && (!has_target || is_collected || collect_delay > 0f))
		{
			PlayBounce();
		}
		else if (is_collected)
		{
			_target_obj = null;
			_tf.DOComplete();
			_sprite_tf.DOComplete();
		}
		else
		{
			PerformDropMovementLogics();
		}
	}

	private void PerformDropMovementLogics()
	{
		Vector3 vector = (_performing_drop_and_fly ? ((Vector3)_dest_global_pos) : _target_char_tf.position) - _tf.position;
		vector.z = 0f;
		dist = vector.magnitude;
		bool flag = res != null && res.definition != null && res.definition.item_size >= 2;
		if (!_performing_drop_and_fly && flag)
		{
			return;
		}
		if (dist < 270f || _performing_drop_and_fly)
		{
			float num = 1f;
			if (_target_obj != null && _target_obj.is_player)
			{
				num = (dist + 0.2f) * 0.01f * 1.3f;
			}
			_tf.position += vector.normalized * speed * Time.deltaTime * 96f * num;
		}
		if (_performing_drop_and_fly)
		{
			_last_dist_to_target = _tf.position.DistSqrTo(_dest_global_pos, 96f);
			if (_last_dist_to_target < 0.1f)
			{
				_performing_drop_and_fly = false;
				if (_flying_without_collider)
				{
					col.enabled = true;
					_flying_without_collider = false;
				}
				ChangeKickableState(now_kickable: true);
				SetDropCollectingState(for_collect: false);
			}
		}
		else
		{
			_last_dist_to_target = _target_char_tf.position.DistSqrTo(_tf.position, 96f);
			dist_to_player = _last_dist_to_target;
		}
	}

	public void FixedUpdateMe(float delta_time)
	{
		if (is_collected)
		{
			return;
		}
		if (col.radius < _col_radius)
		{
			if (col.radius.EqualsTo(_col_radius, 0.005f))
			{
				col.radius = _col_radius;
			}
			else
			{
				col.radius += 0.005f;
			}
		}
		kick_component.FixedUpdateComponent(delta_time);
		if (col.isTrigger && _just_spawned && !(Physics2D.OverlapCircle(_tf.position, col.radius, 513) != null) && !has_target)
		{
			col.isTrigger = false;
			_just_spawned = false;
		}
	}

	public void ProcessDropCollectorRangeCheck(WorldGameObject collector_wgo, Vector3 char_global_pos)
	{
		if (!_performing_drop_and_fly)
		{
			float num = char_global_pos.DistSqrTo(_tf.position, 96f);
			if (collector_wgo.is_player)
			{
				dist_sqr_to_player = num;
			}
			if (!(num > 3.2399998f) && collector_wgo.CanCollectDrop(this) > 0 && !has_target && !(collect_delay > 0f))
			{
				ChangeKickableState(now_kickable: false);
				_target_obj = collector_wgo;
				_target_char_tf = collector_wgo.transform;
				_last_dist_to_target = num;
				_just_spawned = false;
				SetDropCollectingState(for_collect: true);
			}
		}
	}

	public void UnsuccessfullPickup(WorldGameObject obj)
	{
		if (has_target && !(_target_obj != obj))
		{
			_target_obj = null;
			_target_char_tf = null;
			SetDropCollectingState(for_collect: false);
			ChangeKickableState(now_kickable: true);
		}
	}

	private void SetFlyDestination(Vector2 dest_global_pos, bool fly_without_collider = false)
	{
		ChangeKickableState(now_kickable: false);
		_target_obj = null;
		_just_spawned = false;
		_performing_drop_and_fly = true;
		_dest_global_pos = dest_global_pos;
		SetDropCollectingState(for_collect: true);
		_flying_without_collider = fly_without_collider;
		if (fly_without_collider)
		{
			col.enabled = false;
		}
	}

	private void ChangeKickableState(bool now_kickable)
	{
		if (now_kickable)
		{
			kick_component.enabled = true;
			kick_component.StartComponent();
		}
		else
		{
			kick_component.enabled = false;
		}
		kick_component.OnEnableStateChanged();
	}

	private void SetDropCollectingState(bool for_collect)
	{
		_body.bodyType = (for_collect ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic);
		col.isTrigger = for_collect;
	}

	private void PlayBounce()
	{
		_drop_time += Time.deltaTime;
		if (!(_drop_time > bounce_time) && _chosen_curve != null)
		{
			float time = _drop_time / bounce_time;
			float num = _chosen_curve.curve.Evaluate(time);
			_sprite_tf.localPosition = Vector3.up * num * bounce_height;
			_shadow_tf.localScale = (1f + num * shadow_size_k) * Vector3.one;
			_shadow_color.a = 1f - num * shadow_alfa_k * 2f;
			_shadow_renderer.color = _shadow_color;
		}
	}

	public void StopBounce()
	{
		if (_chosen_curve != null && !(sprite == null))
		{
			_chosen_curve = null;
			if (!_sprite_tf.localPosition.magnitude.EqualsTo(0f, 0.0001f))
			{
				_sprite_tf.DOLocalMove(Vector3.zero, 0.2f);
			}
		}
	}

	public void SetInteractionHilight(bool interaction)
	{
		if ((bool)sprite_x2_item)
		{
			sprite_x2_item.color = (interaction ? c_hilighted : c_normal);
		}
		if (interaction)
		{
			currently_higlighted_obj = this;
		}
		else if (currently_higlighted_obj == this)
		{
			currently_higlighted_obj = null;
		}
	}

	public bool CanPickupWithInteraction(BaseCharacterComponent inventory_owner)
	{
		if (is_collected)
		{
			return false;
		}
		if (res.definition.item_size == 2)
		{
			return true;
		}
		return inventory_owner.wgo.data.CanAddItem(res);
	}

	private bool TryPickupWithInteraction(BaseCharacterComponent inventory_owner)
	{
		if (!inventory_owner.wgo.data.CanAddItem(res))
		{
			return false;
		}
		CollectDrop(inventory_owner.wgo);
		return true;
	}

	public void CollectDrop(WorldGameObject player)
	{
		Debug.Log("<color=yellow>Collect drop</color> " + res);
		is_collected = true;
		if (res.is_tech_point)
		{
			MasterAudio.PlaySound("pickup", 1f, null, 0f, "pickup1");
			MainGame.me.player.AddToParams(res.id, res.value);
		}
		else
		{
			WorldMap.OnDropItemRemoved(res);
			if (res.id != "refugee_happiness_item")
			{
				player.AddToInventory(res);
			}
			if (res.definition.item_size == 1)
			{
				string variationName = "pickup1";
				string id = res.definition.id;
				if (id != null && id == "coins")
				{
					variationName = "pickup_coin";
				}
				MasterAudio.PlaySound("pickup", 1f, null, 0f, variationName);
				player.TryEquipPickupedDrop(res);
			}
		}
		DestroyLinkedHint();
		if (res.id == "refugee_happiness_item")
		{
			WorldMap.GetWorldGameObjectByCustomTag("refugee_camp_progress_obj")?.AddToInventory(res);
			RefugeesCampEngine.instance.UpdateRefugeeCampValues(0f, RefugeesCampEngine.UpdateHappinessItemsMode.ItemUpdatesGameRes);
		}
		DropCollectGUI.OnDropCollected(res);
	}

	public void DestroyLinkedHint()
	{
		if (_linked_hint != null)
		{
			_linked_hint.DestroyMe();
		}
	}

	public void MakeObjectMove(Vector2 dir)
	{
		_is_moving = true;
		_move_dir = dir;
	}

	private void DoTryMerging()
	{
		if (is_collected || _is_moving)
		{
			return;
		}
		_do_merge_done = true;
		if (res?.definition == null || res.definition.stack_count == 1 || res.definition.is_big || res.value >= res.definition.stack_count)
		{
			return;
		}
		Collider2D[] array = Physics2D.OverlapCircleAll(_tf.position, 96f, 16384);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == col)
			{
				continue;
			}
			DropResGameObject component = array[i].GetComponent<DropResGameObject>();
			if (component == null || component == this || component.is_collected || component._is_moving || component.res == null || component.res.IsEmpty() || component.res.id != res.id)
			{
				continue;
			}
			if (res.value > component.res.value)
			{
				int num = res.definition.stack_count - res.value;
				if (component.res.value > num)
				{
					res.value += num;
					component.res.value -= num;
				}
				else
				{
					res.value += component.res.value;
					WorldMap.OnDropItemRemoved(component.res);
					component.is_collected = true;
					component.DestroyLinkedHint();
				}
			}
			else
			{
				int num2 = component.res.definition.stack_count - component.res.value;
				if (res.value <= num2)
				{
					component.res.value += res.value;
					component.RedrawStackCounter();
					WorldMap.OnDropItemRemoved(res);
					is_collected = true;
					DestroyLinkedHint();
					break;
				}
				component.res.value += num2;
				res.value -= num2;
			}
			RedrawStackCounter();
			component.RedrawStackCounter();
		}
	}
}
