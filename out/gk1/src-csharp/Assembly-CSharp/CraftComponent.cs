using System;
using System.Collections.Generic;
using UnityEngine;

public class CraftComponent : WorldGameObjectComponent
{
	[Serializable]
	public class CraftQueueItem
	{
		public string id = "";

		public int n;

		public bool infinite;

		public bool is_gratitude_points_craft;

		private CraftDefinition _craft;

		public CraftDefinition craft
		{
			get
			{
				if (_craft == null)
				{
					_craft = GameBalance.me.GetData<CraftDefinition>(id);
				}
				return _craft;
			}
		}
	}

	private const float DROP_OFFSET = 48f;

	public CraftDefinition current_craft;

	public List<CraftDefinition> crafts = new List<CraftDefinition>();

	[Space]
	public bool is_crafting;

	protected WorldGameObject other_obj;

	protected MultiInventory used_multi_inventory;

	private Item _current_item;

	private Item _dur_item;

	private List<Item> _cur_craft_items_used = new List<Item>();

	private CraftDefinition.MultiqualityCraftResult _multiquality_craft_result;

	private string _multiquality_craft_item_id;

	public int craft_amount = 1;

	private const float AUTO_CRAFT_RECALC_PERIOD = 0.5f;

	private static HashSet<CraftComponent> _all_crafts = new HashSet<CraftComponent>();

	private static bool _all_crafts_iterating = false;

	private static List<CraftComponent> _crafts_to_add;

	private static List<CraftComponent> _crafts_to_del;

	private int _cur_last_craft_slot;

	private string _last_craft_id = "";

	private string _last_craft_id_2 = "";

	private float _time_worker_tried_to_start_craft;

	public const float TIME_BETWEEN_TRIES_TO_START_CRAFT = 1f;

	private bool _worker_is_paused;

	private float _time_worker_tried_to_continue_craft;

	public const float TIME_BETWEEN_TRIES_TO_CONTINUE_CRAFT = 1f;

	public bool is_gratitude_points_spent_for_craft;

	public List<CraftQueueItem> craft_queue = new List<CraftQueueItem>();

	public Item current_item => _current_item;

	public bool worker_is_paused => _worker_is_paused;

	public int craft_index => crafts.IndexOf(current_craft);

	public string last_craft_id
	{
		get
		{
			if (_cur_last_craft_slot == 1)
			{
				return _last_craft_id_2;
			}
			return _last_craft_id;
		}
	}

	public bool has_visible_crafts
	{
		get
		{
			foreach (CraftDefinition craft in crafts)
			{
				if (!craft.hidden)
				{
					return true;
				}
			}
			return false;
		}
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		if (!_all_crafts.Contains(this))
		{
			if (_all_crafts_iterating)
			{
				_crafts_to_add.Add(this);
			}
			else
			{
				_all_crafts.Add(this);
			}
		}
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		if (_all_crafts.Contains(this))
		{
			if (_all_crafts_iterating)
			{
				_crafts_to_del.Add(this);
			}
			else
			{
				_all_crafts.Remove(this);
			}
		}
	}

	public override void StartComponent()
	{
		if (Application.isPlaying && !started)
		{
			base.StartComponent();
			FillCraftsList();
			started = true;
		}
	}

	public void FillCraftsList()
	{
		crafts.Clear();
		crafts.AddRange(GameBalance.me.GetCraftsForObject(base.wgo.obj_id));
	}

	public WorldGameObject GetOtherObj()
	{
		return other_obj;
	}

	public override bool DoAction(WorldGameObject other_obj, float delta_time, bool for_gratitude_points = false)
	{
		if (!is_crafting || (other_obj.is_player && (current_craft.is_auto || current_craft.hidden)))
		{
			return false;
		}
		this.other_obj = other_obj;
		used_multi_inventory = this.other_obj.GetMultiInventory();
		float k;
		if (!for_gratitude_points)
		{
			if (this.other_obj.is_player)
			{
				if (!GetCraftCoeffForPlayer(out k))
				{
					return false;
				}
			}
			else if (base.wgo == this.other_obj)
			{
				k = 1f;
			}
			else if (this.other_obj.IsWorker())
			{
				k = this.other_obj.data.GetParam("working_k");
				ItemDefinition.ItemType itemType = ItemDefinition.ItemType.None;
				float num = 0f;
				ToolActions tool_actions = base.wgo.obj_def.tool_actions;
				for (int i = 0; i < tool_actions.action_tools.Count; i++)
				{
					if (tool_actions.action_k[i] > num)
					{
						num = tool_actions.action_k[i];
						itemType = tool_actions.action_tools[i];
					}
				}
				if (itemType == ItemDefinition.ItemType.None)
				{
					itemType = ItemDefinition.ItemType.Hand;
				}
				this.other_obj.components.character.SetWorkerToolNum(itemType);
			}
			else
			{
				k = 1f;
			}
		}
		else
		{
			k = 0.125f;
		}
		if (!for_gratitude_points)
		{
			if (other_obj.is_player)
			{
				if (!TrySpendPlayerEnergy(other_obj, delta_time))
				{
					return false;
				}
				SpendPlayerSanity(other_obj, delta_time);
				base.wgo.OnWorkAction();
			}
			else if (base.wgo.has_linked_worker)
			{
				base.wgo.OnWorkAction();
			}
		}
		if ((double)current_craft.craft_time.EvaluateFloat(base.wgo, MainGame.me.player) <= 0.001)
		{
			base.wgo.progress = 1f;
		}
		else
		{
			base.wgo.progress += k * delta_time / current_craft.craft_time.EvaluateFloat(base.wgo, MainGame.me.player);
		}
		if (!string.IsNullOrEmpty(current_craft.game_res_to_mirror_name) && current_craft.game_res_to_mirror_max > 0f)
		{
			base.wgo.SetParam(current_craft.game_res_to_mirror_name, base.wgo.progress * current_craft.game_res_to_mirror_max);
		}
		if (base.wgo.progress < 1f)
		{
			FastRedrawWhileInProgress();
			return false;
		}
		FinishCurrentCraft();
		return true;
	}

	private void FinishCurrentCraft()
	{
		if (current_craft.craft_type == CraftDefinition.CraftType.Survey && current_craft.sub_type != CraftDefinition.CraftSubType.SurveySciencePoints)
		{
			ShowSurveyCompleteWindow(current_craft);
		}
		else
		{
			ProcessFinishedCraft();
		}
	}

	private void ProcessFinishedCraft()
	{
		base.wgo.OnBeganObjectModifications();
		if (current_craft.flag != 1)
		{
			List<Item> cant_insert = new List<Item>();
			if (current_craft.IsBodyPartExtractionCraft() && _current_item != null)
			{
				List<Item> list = ResModificator.ProcessItemsListBeforeDrop(current_craft.output, base.wgo, MainGame.me.player, _current_item);
				list.Add(_current_item);
				if (!base.wgo.is_current_craft_gratitude)
				{
					base.wgo.DropItems(list);
				}
				else
				{
					DistributeDropsFromSoulsCraft(list);
				}
			}
			else if (current_craft.IsBodyPartInsertionCraft() && _current_item != null)
			{
				((base.wgo.data.inventory.Count > 0) ? base.wgo.data.inventory[0] : null)?.AddItem(_current_item);
			}
			else if (other_obj.is_player || current_craft.is_auto)
			{
				if (current_craft.IsMultiqualityOutput() && _multiquality_craft_result != null)
				{
					cant_insert = DropMultiqualityOutput(do_not_really_drop: false);
				}
				else
				{
					cant_insert = ResModificator.ProcessItemsListBeforeDrop(current_craft.output, base.wgo, other_obj);
					if (base.wgo.obj_id == "refugee_camp_cooking_table" || base.wgo.obj_id == "refugee_camp_cooking_table_2" || base.wgo.obj_id == "refugee_camp_hive")
					{
						WorldMap.GetWorldGameObjectByCustomTag("refugee_camp_depot").AddToInventory(cant_insert);
					}
					else if (base.wgo.obj_id == "refugee_camp_well")
					{
						WorldMap.GetWorldGameObjectByCustomTag("refugee_camp_well").AddToInventory(cant_insert);
					}
					else
					{
						if (base.wgo.obj_id == "tavern_kitchen" || base.wgo.obj_id == "tavern_oven")
						{
							WorldGameObject worldGameObjectByObjId = WorldMap.GetWorldGameObjectByObjId("npc_tavern_barman");
							if (worldGameObjectByObjId == null)
							{
								Debug.LogError("Can not put tavern_kitchen output to barmen: not found barmen WGO! Call Bulat. #2");
							}
							else
							{
								int count = cant_insert.Count;
								worldGameObjectByObjId.TryPutToInventory(cant_insert, out cant_insert);
								if (count > cant_insert.Count)
								{
									base.wgo.SetParam("do_roll_anim", 1f);
								}
							}
						}
						if (base.wgo.is_current_craft_gratitude)
						{
							DistributeDropsFromSoulsCraft(cant_insert);
						}
						else
						{
							base.wgo.DropItems(cant_insert);
						}
					}
				}
			}
			else if (HasLinkedWorker() || base.wgo.is_current_craft_gratitude)
			{
				bool flag = true;
				if (current_craft.IsMultiqualityOutput() && _multiquality_craft_result != null)
				{
					cant_insert = DropMultiqualityOutput(do_not_really_drop: true);
					flag = false;
				}
				else
				{
					cant_insert = ResModificator.ProcessItemsListBeforeDrop(current_craft.output, base.wgo, MainGame.me.player);
				}
				List<Item> list2 = new List<Item>();
				for (int i = 0; i < cant_insert.Count; i++)
				{
					if (cant_insert[i].is_tech_point)
					{
						if (base.wgo.is_current_craft_gratitude)
						{
							list2.Add(cant_insert[i]);
						}
						cant_insert.RemoveAt(i);
						i--;
					}
				}
				if (flag && !base.wgo.CanPutToAllPossibleInventories(cant_insert, out var can_not_put))
				{
					if (!base.wgo.is_current_craft_gratitude)
					{
						base.wgo.progress = 0.999999f;
						SetWorkerPausedMode(paused: true);
						_time_worker_tried_to_continue_craft = 1f;
						base.wgo.linked_worker.components.character.SetNoWorkerTool();
						return;
					}
					base.wgo.PutToAllPossibleInventories(can_not_put, out var cant_insert2);
					if (cant_insert2 != null && cant_insert2.Count > 0)
					{
						base.wgo.progress = 0.999999f;
						SetWorkerPausedMode(paused: true);
						_time_worker_tried_to_continue_craft = 1f;
						return;
					}
					base.wgo.DropItems(list2);
				}
				else
				{
					base.wgo.PutToAllPossibleInventories(cant_insert, out can_not_put);
					if (can_not_put != null && can_not_put.Count > 0)
					{
						base.wgo.DropItems(can_not_put);
					}
					base.wgo.DropItems(list2);
				}
			}
			else
			{
				other_obj.AddToInventory(ResModificator.ProcessItemsListBeforeDrop(current_craft.output, base.wgo, other_obj));
			}
			if (base.wgo.is_current_craft_gratitude && worker_is_paused)
			{
				SetWorkerPausedMode(paused: false);
			}
			base.wgo.AddToParams(current_craft.output_res_wgo);
			foreach (Item item2 in current_craft.output_to_wgo)
			{
				if (item2.value > 0)
				{
					base.wgo.AddToInventory(item2);
					continue;
				}
				if (item2.definition.has_durability && item2.value == -1)
				{
					foreach (Item item3 in cant_insert)
					{
						if (item3.id == item2.id)
						{
							Item lastItem = base.wgo.data.GetLastItem(item2.id);
							item3.durability = lastItem.durability;
							break;
						}
					}
				}
				base.wgo.data.RemoveItem(item2.id, Mathf.Abs(item2.value));
			}
			if ((current_craft.craft_type == CraftDefinition.CraftType.Survey && current_craft.sub_type != CraftDefinition.CraftSubType.SurveySciencePoints) || current_craft.takes_item_durability)
			{
				if (_current_item == null)
				{
					if (current_craft.craft_type == CraftDefinition.CraftType.Survey)
					{
						base.wgo.DropItem(new Item(string.IsNullOrEmpty(_multiquality_craft_item_id) ? current_craft.needs[0].id : _multiquality_craft_item_id, 1));
					}
					if (string.IsNullOrEmpty(_multiquality_craft_item_id) || (current_craft.craft_type == CraftDefinition.CraftType.None && _dur_item != null))
					{
						if (_dur_item != null)
						{
							DropDurItem();
						}
						else
						{
							Debug.LogError("Can't give back a craft item because it is null");
						}
					}
				}
				else if (_dur_item == null)
				{
					if (_current_item.GetParamInt("taken_from_player_inventory") == 1 || current_craft.craft_type == CraftDefinition.CraftType.Survey)
					{
						base.wgo.DropItem(_current_item, Direction.ToPlayer);
					}
					else
					{
						base.wgo.PutToAllPossibleInventories(new List<Item> { _current_item }, out var cant_insert3);
						if (cant_insert3 != null && cant_insert3.Count > 0)
						{
							base.wgo.DropItem(cant_insert3[0]);
						}
					}
				}
				else
				{
					DropDurItem();
				}
			}
			foreach (SmartExpression out_items_expression in current_craft.out_items_expressions)
			{
				out_items_expression.Evaluate(cant_insert);
			}
		}
		if (!current_craft.set_out_wgo_params_on_start)
		{
			base.wgo.SetParam(current_craft.output_set_res_wgo);
		}
		_ = other_obj.is_player;
		if (HasLinkedWorker())
		{
			base.wgo.linked_worker.components.character.SetNoWorkerTool();
		}
		Item item = ((base.wgo.data.inventory.Count > 0) ? base.wgo.data.inventory[0] : null);
		if (item != null)
		{
			item.AddToParams(current_craft.itempars_add);
			item.SetParam(current_craft.itempars_set);
			item.AddNotFoldedItemsWithoutCheck(ResModificator.ProcessItemsListBeforeDrop(current_craft.item_output, base.wgo, MainGame.me.player));
		}
		if (!string.IsNullOrEmpty(current_craft.end_script))
		{
			if (current_craft.end_script.StartsWith("g:"))
			{
				GS.RunFlowScript(current_craft.end_script.Substring(2));
			}
			else if (!current_craft.end_script.StartsWith(":") && current_craft.end_script.Contains(":"))
			{
				CustomFlowScript customFlowScript = GS.RunFlowScript(current_craft.end_script.Split(':')[0]);
				customFlowScript.StartBehaviour();
				if (current_craft.end_script.Split(':').Length > 2)
				{
					customFlowScript.FireEvent(current_craft.end_script.Split(':')[1], current_craft.end_script.Split(':')[2]);
				}
				else
				{
					customFlowScript.FireEvent(current_craft.end_script.Split(':')[1]);
				}
			}
			else
			{
				base.wgo.AttachFlowScript(current_craft.end_script);
			}
		}
		if (!string.IsNullOrEmpty(current_craft.end_event))
		{
			base.wgo.FireEvent(current_craft.end_event);
		}
		if (!string.IsNullOrEmpty(current_craft.change_wgo))
		{
			if (current_craft.use_variations)
			{
				base.wgo.ReplaceWithObject(current_craft.change_wgo, current_craft.puff_when_replaced, current_craft.variation_index);
			}
			else
			{
				base.wgo.ReplaceWithObject(current_craft.change_wgo, current_craft.puff_when_replaced);
			}
		}
		if (!string.IsNullOrEmpty(current_craft.craft_after_finish))
		{
			string craft_to_start_name = current_craft.craft_after_finish;
			GJTimer.AddTimer(0.1f, delegate
			{
				CraftDefinition dataOrNull = GameBalance.me.GetDataOrNull<CraftDefinition>(craft_to_start_name);
				if (dataOrNull == null)
				{
					Debug.LogError("Can not start craft: craft [" + craft_to_start_name + "] is null!");
				}
				else if (!base.wgo.components.craft.Craft(dataOrNull))
				{
					Debug.LogError("Failed to start craft!");
				}
			});
		}
		MainGame.me.save.OnFinishedCraft(current_craft);
		if (!string.IsNullOrEmpty(base.wgo.obj_def.anim_on_craft_finish))
		{
			base.wgo.TriggerSmartAnimation(base.wgo.obj_def.anim_on_craft_finish);
		}
		End();
		base.wgo.Redraw();
	}

	private void DropDurItem()
	{
		if (_dur_item.definition.dont_break_on_zero_dur || _dur_item.durability_state != 0)
		{
			if (_dur_item.GetParamInt("taken_from_player_inventory") == 1 || current_craft.craft_type == CraftDefinition.CraftType.Survey)
			{
				base.wgo.DropItem(_dur_item, Direction.ToPlayer);
			}
			else
			{
				List<Item> cant_insert = new List<Item>();
				cant_insert.Add(_dur_item);
				base.wgo.PutToAllPossibleInventories(cant_insert, out cant_insert);
				if (cant_insert != null && cant_insert.Count > 0)
				{
					base.wgo.DropItem(cant_insert[0]);
				}
			}
		}
		_dur_item = null;
	}

	private List<Item> DropMultiqualityOutput(bool do_not_really_drop)
	{
		int num = 0;
		List<Item> list = new List<Item>();
		float[] quality_probabilities = _multiquality_craft_result.quality_probabilities;
		for (int num2 = quality_probabilities.Length - 1; num2 >= 0; num2--)
		{
			if (UnityEngine.Random.value < quality_probabilities[num2])
			{
				num = num2;
				break;
			}
		}
		foreach (Item item2 in current_craft.output)
		{
			bool flag = false;
			if (item2.is_multiquality)
			{
				foreach (string multiquality_item in item2.multiquality_items)
				{
					if (Mathf.Abs(GameBalance.me.GetData<ItemDefinition>(multiquality_item).quality - (float)num - 1f) < 0.01f)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				if (!do_not_really_drop)
				{
					base.wgo.DropItem(item2);
				}
				list.Add(item2);
				continue;
			}
			Item item = new Item(item2.multiquality_items[num], item2.value);
			list.Add(item);
			if (do_not_really_drop)
			{
				continue;
			}
			if (base.wgo.obj_id == "tavern_kitchen" || base.wgo.obj_id == "tavern_oven")
			{
				WorldGameObject worldGameObjectByObjId = WorldMap.GetWorldGameObjectByObjId("npc_tavern_barman");
				if (worldGameObjectByObjId == null)
				{
					Debug.LogError("Can not put tavern_kitchen output to barmen: not found barmen WGO! Call Bulat. #3");
					base.wgo.DropItem(item);
					continue;
				}
				worldGameObjectByObjId.TryPutToInventory(new List<Item> { item }, out var cant_insert);
				if (cant_insert != null && cant_insert.Count > 0)
				{
					base.wgo.DropItems(cant_insert);
				}
				else
				{
					base.wgo.SetParam("do_roll_anim", 1f);
				}
			}
			else if (base.wgo.obj_id == "refugee_camp_cooking_table" || base.wgo.obj_id == "refugee_camp_cooking_table_2" || base.wgo.obj_id == "refugee_camp_hive")
			{
				WorldMap.GetWorldGameObjectByCustomTag("refugee_camp_depot").AddToInventory(item);
			}
			else if (base.wgo.obj_id == "refugee_camp_well")
			{
				WorldMap.GetWorldGameObjectByCustomTag("refugee_camp_well").AddToInventory(item);
			}
			else
			{
				base.wgo.DropItem(item);
			}
		}
		return list;
	}

	private void FastRedrawWhileInProgress()
	{
		base.wgo.custom_drawers.FastRedraw();
	}

	public Vector3 GetDropBos(WorldGameObject target_obj = null)
	{
		if (target_obj == null)
		{
			target_obj = MainGame.me.player;
		}
		return target_obj.tf.position + target_obj.components.character.anim_direction.ClockwiseDir().ToVec3() * 48f;
	}

	public override bool Interact(WorldGameObject other_obj, float delta_time)
	{
		if (!CanInteractCraft())
		{
			return false;
		}
		this.other_obj = other_obj;
		used_multi_inventory = other_obj.GetMultiInventory();
		GUIElements.me.OpenCraftGUI(base.wgo);
		return false;
	}

	public bool CanInteractCraft()
	{
		if (!base.wgo.obj_def.can_insert_zombie && is_crafting)
		{
			return false;
		}
		if (base.wgo.is_removing)
		{
			return false;
		}
		if (has_visible_crafts)
		{
			return base.wgo.obj_def.interaction_type == ObjectDefinition.InteractionType.Craft;
		}
		return false;
	}

	public bool CraftAsPlayer(CraftDefinition craft, Item try_use_particular_item = null, List<string> multiquality_ids = null, List<Item> override_needs = null, bool ignore_crafts_list = false, int amount = 1, WorldGameObject other_obj_override = null)
	{
		other_obj = ((other_obj_override != null) ? other_obj_override : MainGame.me.player);
		if (craft == null)
		{
			_last_craft_id = "";
			_cur_last_craft_slot = 0;
		}
		else
		{
			if (_cur_last_craft_slot == 1)
			{
				_last_craft_id_2 = craft.id;
			}
			else
			{
				_last_craft_id = craft.id;
			}
			_cur_last_craft_slot = craft.store_last_craft_slot;
		}
		if (!IsCraftQueueEmpty() || is_crafting)
		{
			return true;
		}
		return Craft(craft, try_use_particular_item, multiquality_ids, override_needs, ignore_crafts_list, amount);
	}

	public void StartRemovalCraft(CraftDefinition craft)
	{
		if (base.wgo.has_linked_worker)
		{
			base.wgo.DropItem(base.wgo.linked_worker.worker.GetOnGroundItem());
			WorldMap.RemoveZombieWorkerToStock(base.wgo.linked_worker);
		}
		if (is_crafting && current_craft != null)
		{
			Item item = new Item
			{
				inventory_size = 100
			};
			for (int i = 0; i < craft_amount; i++)
			{
				item.AddItems(current_craft.needs, return_false_if_cannot_add_all: false);
			}
			base.wgo.DropItems(item.inventory);
			craft_amount = 0;
			if (current_craft.output_to_wgo_on_start.Count > 0)
			{
				base.wgo.data.RemoveItems(current_craft.output_to_wgo_on_start);
			}
		}
		base.enabled = true;
		is_crafting = true;
		craft_queue = new List<CraftQueueItem>();
		current_craft = craft;
		_multiquality_craft_item_id = null;
		if (!crafts.Contains(craft))
		{
			crafts.Add(craft);
		}
		base.wgo.progress = 0f;
		base.wgo.is_current_craft_gratitude = false;
		base.wgo.RedrawBubble();
	}

	public virtual void EnqueueCraft(CraftDefinition craft, List<string> multiquality_ids, int amount, bool can_use_player_inventory = false)
	{
		if (craft == null)
		{
			Debug.LogError("Trying to enqueue a null craft");
			return;
		}
		Debug.Log($"EnqueueCraft {craft.id}, amount={amount}");
		bool flag = true;
		foreach (CraftQueueItem item in craft_queue)
		{
			if (!(item.id == craft.id))
			{
				continue;
			}
			if (GlobalCraftControlGUI.is_global_control_active)
			{
				if (item.is_gratitude_points_craft)
				{
					flag = false;
				}
			}
			else if (!item.is_gratitude_points_craft)
			{
				flag = false;
			}
			if (!flag)
			{
				item.n += amount;
				break;
			}
		}
		if (flag)
		{
			craft_queue.Add(new CraftQueueItem
			{
				id = craft.id,
				n = amount,
				is_gratitude_points_craft = GlobalCraftControlGUI.is_global_control_active
			});
			if (GlobalCraftControlGUI.is_global_control_active)
			{
				TryStartCraftFromQueue(can_use_player_inventory: false, start_by_player: false);
				RefreshComponentBubbleData(show_interaction_buttons: false);
			}
		}
		TryStartCraftFromQueue(can_use_player_inventory, !GlobalCraftControlGUI.is_global_control_active);
	}

	public bool HasGratitudeCraftInQueue()
	{
		for (int i = 0; i < craft_queue.Count; i++)
		{
			if (craft_queue[i].is_gratitude_points_craft)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCraftQueueEmpty()
	{
		if (craft_queue == null || craft_queue.Count == 0)
		{
			return true;
		}
		for (int num = craft_queue.Count - 1; num >= 0; num--)
		{
			if (craft_queue[num]?.craft == null)
			{
				craft_queue.RemoveAt(num);
			}
		}
		return false;
	}

	private CraftQueueItem CanStartCraftFromQueue(bool use_player_inventory = false, bool start_by_player = true)
	{
		if (IsCraftQueueEmpty())
		{
			return null;
		}
		MultiInventory multiInventory = ((!(GlobalCraftControlGUI.is_global_control_active && use_player_inventory)) ? base.wgo.GetMultiInventory(null, "", use_player_inventory ? MultiInventory.PlayerMultiInventory.IncludePlayer : MultiInventory.PlayerMultiInventory.ExcludePlayer) : ((!WorldZone.GetZoneOfObject(base.wgo).IsPlayerInZone()) ? base.wgo.GetMultiInventory(null, "", MultiInventory.PlayerMultiInventory.ExcludePlayer) : base.wgo.GetMultiInventory(null, "", use_player_inventory ? MultiInventory.PlayerMultiInventory.IncludePlayer : MultiInventory.PlayerMultiInventory.ExcludePlayer)));
		foreach (CraftQueueItem item in craft_queue)
		{
			if (multiInventory.IsEnoughItems(item.craft.needs) && base.wgo.data.IsEnoughItems(item.craft.needs_from_wgo) && item.craft.condition.EvaluateBoolean(base.wgo, MainGame.me.player) && (start_by_player || !item.is_gratitude_points_craft || CanSpendPlayerGratitudePoints(item.craft.gratitude_points_craft_cost?.EvaluateFloat() ?? 0f)))
			{
				return item;
			}
		}
		return null;
	}

	public void TryStartCraftFromQueue(bool can_use_player_inventory = false, bool start_by_player = true)
	{
		int num = 0;
		for (int i = 0; i < craft_queue.Count; i++)
		{
			if (craft_queue[i].n == 0)
			{
				craft_queue.RemoveAt(i--);
			}
		}
		while (!is_crafting)
		{
			CraftQueueItem craftQueueItem = CanStartCraftFromQueue(can_use_player_inventory, start_by_player);
			if (craftQueueItem == null)
			{
				break;
			}
			if (!craftQueueItem.infinite && --craftQueueItem.n == 0)
			{
				craft_queue.Remove(craftQueueItem);
			}
			CraftReally(craftQueueItem.craft, null, null, null, ignore_crafts_list: false, 1, craftQueueItem.is_gratitude_points_craft, can_use_player_inventory, start_by_player);
			if (string.IsNullOrEmpty(craftQueueItem.craft.craft_after_finish) && craftQueueItem.craft.craft_time_is_zero)
			{
				if (++num > 500)
				{
					Debug.LogError("TryStartCraftFromQueue iterator is too big, wgo: " + base.wgo.name + ", craft: " + craftQueueItem.id, base.wgo);
					break;
				}
				continue;
			}
			break;
		}
	}

	public virtual bool Craft(CraftDefinition craft, Item try_use_particular_item = null, List<string> multiquality_ids = null, List<Item> override_needs = null, bool ignore_crafts_list = false, int amount = 1)
	{
		Debug.Log("Craft " + craft?.id);
		if (crafts.IndexOf(craft) < 0 && !ignore_crafts_list)
		{
			Debug.LogError("craft " + craft?.id + " not found in the wgo's crafts inventory", base.wgo);
			string text = "";
			foreach (CraftDefinition craft2 in crafts)
			{
				text = text.ConcatWithSeparator(craft2.id, ", ");
			}
			Debug.LogError("Available crafts: " + (string.IsNullOrEmpty(text) ? "[none]" : text));
			return false;
		}
		if (craft_queue == null)
		{
			craft_queue = new List<CraftQueueItem>();
		}
		return CraftReally(craft, try_use_particular_item, multiquality_ids, override_needs, ignore_crafts_list, amount, GlobalCraftControlGUI.is_global_control_active, use_player_inv: false, GlobalCraftControlGUI.is_global_control_active);
	}

	protected bool CraftReally(CraftDefinition craft, Item try_use_particular_item = null, List<string> multiquality_ids = null, List<Item> override_needs = null, bool ignore_crafts_list = false, int amount = 1, bool for_gratitude_points = false, bool use_player_inv = false, bool start_by_player = true)
	{
		_current_item = null;
		_cur_craft_items_used = new List<Item>();
		if (other_obj == null)
		{
			other_obj = MainGame.me.player;
		}
		used_multi_inventory = (other_obj.is_player ? other_obj.GetMultiInventoryForInteraction() : other_obj.GetMultiInventory());
		if (for_gratitude_points && !other_obj.is_player && base.wgo != null)
		{
			if (WorldZone.GetZoneOfObject(base.wgo).IsPlayerInZone())
			{
				used_multi_inventory = base.wgo.GetMultiInventory(null, "", use_player_inv ? MultiInventory.PlayerMultiInventory.IncludePlayer : MultiInventory.PlayerMultiInventory.ExcludePlayer);
			}
			else
			{
				used_multi_inventory = base.wgo.GetMultiInventory(null, "", MultiInventory.PlayerMultiInventory.ExcludePlayer);
			}
		}
		if (craft.item_needs.Count > 0)
		{
			if (base.wgo.data.inventory.Count <= 0)
			{
				Debug.LogError("Can not start craft " + craft.id + ": not found any item in workbench inventory.");
				return false;
			}
			Item item = base.wgo.data.inventory[0];
			foreach (Item item_need in craft.item_needs)
			{
				if (item.GetItemsCount(item_need.id) < item_need.value)
				{
					Debug.LogError("Can not start craft " + craft.id + ": not found item " + item_need.id + " in first item inventory.");
					return false;
				}
			}
			if (!craft.item_needs_leave)
			{
				item.RemoveItems(new List<Item>(craft.item_needs));
			}
		}
		if (for_gratitude_points && !start_by_player)
		{
			int num = 0;
			if (craft.gratitude_points_craft_cost != null)
			{
				num = Mathf.RoundToInt(craft.gratitude_points_craft_cost.EvaluateFloat(MainGame.me.player) * (1f - base.wgo.progress));
			}
			TrySpendPlayerGratitudePoints(num);
		}
		List<Item> list = override_needs ?? new List<Item>(craft.needs);
		craft_amount = amount;
		if (amount > 1)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = new Item(list[i].id, list[i].value * amount);
			}
		}
		if (try_use_particular_item != null && craft.IsBodyPartInsertionCraft())
		{
			_current_item = try_use_particular_item;
		}
		else if (try_use_particular_item != null && try_use_particular_item.definition.stack_count == 1)
		{
			if (craft.IsBodyPartExtractionCraft())
			{
				_current_item = try_use_particular_item;
			}
			else if (list.Count > 0 && list[0].id == try_use_particular_item.id)
			{
				int itemsCount = MainGame.me.player.data.GetItemsCount(try_use_particular_item.id, count_secondary_inventory: true);
				if (used_multi_inventory.TryRemoveSpecificItemNoCheck(try_use_particular_item))
				{
					_current_item = try_use_particular_item;
					_current_item.SetParam("taken_from_player_inventory", (itemsCount - MainGame.me.player.data.GetItemsCount(try_use_particular_item.id, count_secondary_inventory: true) > 0) ? 1 : 0);
					list.RemoveAt(0);
					_cur_craft_items_used.Add(_current_item);
				}
			}
		}
		if (craft.takes_item_durability && _dur_item == null)
		{
			if (craft.needs.Count <= craft.dur_needs_item_index)
			{
				Debug.LogError("Can't take durability of 'needs' because it is empty, dur_needs_item_index = " + craft.dur_needs_item_index + ", needs.Count = " + craft.needs.Count);
				return false;
			}
			Item item2 = craft.needs[craft.dur_needs_item_index];
			string id = item2.id;
			if (item2.definition == null && multiquality_ids != null && craft.dur_needs_item_index < multiquality_ids.Count)
			{
				item2 = new Item(multiquality_ids[craft.dur_needs_item_index], item2.value);
			}
			if (item2.definition != null && item2.definition.stack_count != 1)
			{
				Debug.LogError("Balance error: takes_1st_item_durability can be used only with stack_count = 1, item_id = " + item2.id);
				return false;
			}
			_dur_item = used_multi_inventory.GetItem(item2.id, Item.ItemFindLogics.WithLowestDurability);
			if (_dur_item == null)
			{
				Debug.LogError("Can't find item in multi-inventory, id = " + item2.id);
				return false;
			}
			int itemsCount2 = MainGame.me.player.data.GetItemsCount(_dur_item.id, count_secondary_inventory: true);
			used_multi_inventory.TryRemoveSpecificItemNoCheck(_dur_item);
			_dur_item.SetParam("taken_from_player_inventory", (itemsCount2 - MainGame.me.player.data.GetItemsCount(_dur_item.id, count_secondary_inventory: true) > 0) ? 1 : 0);
			_cur_craft_items_used.Add(_dur_item);
			Item.RemoveItemWithIDFromTheList(list, id, dont_remove_but_set_zero: true);
		}
		if (craft.takes_item_durability)
		{
			_dur_item.durability -= craft.dur_needs_item;
		}
		if (list.Count > 0)
		{
			int num2 = 0;
			foreach (Item item3 in list)
			{
				if (multiquality_ids != null && num2 < multiquality_ids.Count && !string.IsNullOrEmpty(multiquality_ids[num2]))
				{
					_cur_craft_items_used.Add(new Item(multiquality_ids[num2], item3.value));
				}
				else
				{
					_cur_craft_items_used.Add(item3);
				}
				num2++;
			}
			List<Item> list2 = new List<Item>();
			if (craft.IsBodyPartInsertionCraft() && _current_item != null)
			{
				List<Item> items = new List<Item>
				{
					new Item(_current_item)
				};
				if (used_multi_inventory.RemoveItems(items, MultiInventory.DestinationType.AllFromFirst, multiquality_ids))
				{
				}
			}
			else
			{
				if (!used_multi_inventory.RemoveItems(list, MultiInventory.DestinationType.AllFromFirst, multiquality_ids, list2))
				{
					_cur_craft_items_used.Clear();
					return false;
				}
				if (list2.Count > 0)
				{
					_cur_craft_items_used = list2;
					if (craft.takes_item_durability && _dur_item != null)
					{
						_cur_craft_items_used.Add(_dur_item);
					}
				}
			}
		}
		if (!base.wgo.data.RemoveItems(craft.needs_from_wgo, amount))
		{
			Debug.LogError("Not enough needs_from_wgo to craft");
			return false;
		}
		if (craft.transfer_needs_to_wgo)
		{
			if (craft.needs.Count == 1 && _current_item != null && _current_item.id == craft.needs[0].id && craft.needs[0].value == 1 && _current_item.definition.has_durability)
			{
				base.wgo.data.inventory.Add(_current_item);
			}
			else if (craft.needs.Count == 1 && try_use_particular_item != null && try_use_particular_item.id == craft.needs[0].id)
			{
				base.wgo.data.inventory.Add(try_use_particular_item);
			}
			else
			{
				base.wgo.data.AddItems(craft.needs, return_false_if_cannot_add_all: false);
			}
		}
		if (craft.set_out_wgo_params_on_start)
		{
			base.wgo.SetParam(craft.output_set_res_wgo);
		}
		foreach (Item item4 in craft.output_to_wgo_on_start)
		{
			if (item4.value > 0)
			{
				base.wgo.AddToInventory(item4);
			}
			else
			{
				base.wgo.data.RemoveItem(item4.id, Mathf.Abs(item4.value));
			}
		}
		current_craft = craft;
		_multiquality_craft_item_id = ((multiquality_ids == null || multiquality_ids.Count == 0) ? null : multiquality_ids[0]);
		_multiquality_craft_result = craft.GetMultiqualityResult(multiquality_ids);
		base.wgo.progress = 0f;
		base.wgo.auto_craft_time_spent = 0f;
		base.wgo.is_current_craft_gratitude = for_gratitude_points;
		is_crafting = true;
		if (!string.IsNullOrEmpty(base.wgo.obj_def.craft_start_sound))
		{
			Sounds.PlaySound(base.wgo.obj_def.craft_start_sound);
		}
		if (!string.IsNullOrEmpty(base.wgo.obj_def.anim_on_craft_start))
		{
			base.wgo.TriggerSmartAnimation(base.wgo.obj_def.anim_on_craft_start);
		}
		OnEnabled();
		base.wgo.OnCraftStateChanged();
		if (craft.transfer_needs_to_wgo || craft.needs_from_wgo.Count > 0)
		{
			base.wgo.Redraw();
		}
		if ((craft.is_auto || for_gratitude_points) && craft.craft_time.EvaluateFloat(base.wgo, MainGame.me.player).EqualsTo(0f))
		{
			FinishCurrentCraft();
		}
		return true;
	}

	public void CancelRemovalCraft()
	{
		Debug.Log("Canceling removal craft...");
		End();
	}

	public virtual void Cancel()
	{
		List<Item> cur_craft_items_used = _cur_craft_items_used;
		if (cur_craft_items_used != null && cur_craft_items_used.Count > 0)
		{
			Debug.Log("Canceling craft, dropping items: " + _cur_craft_items_used.JoinToString());
			if (current_craft != null && current_craft.takes_item_durability && _dur_item != null)
			{
				_dur_item.durability += current_craft.dur_needs_item;
			}
			base.wgo.DropItems(_cur_craft_items_used);
			_cur_craft_items_used = new List<Item>();
		}
		if (current_craft != null)
		{
			base.wgo.SetParam(current_craft.set_when_cancelled);
		}
		else
		{
			Debug.LogError("Trying to cancel craft when current_craft is null at wgo: " + base.wgo.name, base.wgo);
		}
		End();
	}

	protected virtual void End()
	{
		if (destroyed)
		{
			return;
		}
		base.wgo.progress = 0f;
		base.wgo.data.RemoveZeroParams();
		is_gratitude_points_spent_for_craft = false;
		string arg = "NULL";
		try
		{
			if (other_obj != null && other_obj.gameObject != null)
			{
				arg = other_obj.name;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("WGO instance has no gameObject! other_obj.id = " + ((other_obj == null) ? "NULL" : other_obj.obj_id) + " \nException: " + ex);
		}
		Debug.Log($"Craft end, other_obj = {arg}, n = {craft_amount}, q = {craft_queue?.Count}");
		if (craft_amount <= 1)
		{
			is_crafting = false;
			current_craft = null;
			_multiquality_craft_item_id = null;
			_dur_item = null;
			_multiquality_craft_result = null;
			base.wgo.is_current_craft_gratitude = false;
			TryStartCraftFromQueue(other_obj != null && other_obj.is_player);
		}
		else
		{
			craft_amount--;
		}
		base.wgo.OnCraftStateChanged();
	}

	public override void UnprepareForInteraction()
	{
		if (MainGame.me.player.components.character.wgo_hilighted_for_work == base.wgo)
		{
			MainGame.me.player.components.character.wgo_hilighted_for_work = null;
		}
		if (base.wgo.prepared_for_interaction && !GUIElements.me.craft.is_just_opened && !base.wgo.is_autopsy_table)
		{
			if (GUIElements.me.craft.is_shown)
			{
				GUIElements.me.craft.Hide();
			}
			if (GUIElements.me.rat_cell_gui.is_shown)
			{
				GUIElements.me.rat_cell_gui.Hide();
			}
		}
	}

	private void RemoveBubble()
	{
		InteractionBubbleGUI.RemoveBubble(base.wgo.unique_id, immediate: true);
	}

	public override void UpdateComponent(float delta_time)
	{
	}

	protected void ReallyUpdateComponent(float delta_time)
	{
		if (!is_crafting && _time_worker_tried_to_start_craft <= 0f && HasLinkedWorker() && !IsCraftQueueEmpty())
		{
			TryStartCraftFromQueue();
			_time_worker_tried_to_start_craft = 1f;
		}
		if (!is_crafting && HasGratitudeCraftInQueue())
		{
			WorldZone myWorldZone = base.wgo.GetMyWorldZone();
			bool can_use_player_inventory = ((myWorldZone != null && myWorldZone.IsPlayerInZone()) ? true : false);
			TryStartCraftFromQueue(can_use_player_inventory, start_by_player: false);
		}
		_time_worker_tried_to_start_craft -= delta_time;
		if (is_crafting && current_craft != null)
		{
			if (current_craft.is_auto)
			{
				base.wgo.auto_craft_time_spent += delta_time;
				float num = (current_craft.craft_time_is_zero ? 0.01f : 0.5f);
				if (current_craft.craft_time.has_expression)
				{
					while (base.wgo.auto_craft_time_spent >= num)
					{
						base.wgo.auto_craft_time_spent -= num;
						DoAction(base.wgo, num);
					}
				}
			}
			else if (HasLinkedWorker() && !base.wgo.is_removing)
			{
				if (_worker_is_paused)
				{
					_time_worker_tried_to_continue_craft -= delta_time;
					if (_time_worker_tried_to_continue_craft < 0f)
					{
						_time_worker_tried_to_continue_craft = 1f;
						SetWorkerPausedMode(paused: false);
					}
				}
				if (!_worker_is_paused)
				{
					if (base.wgo.linked_worker != null)
					{
						DoAction(base.wgo.linked_worker, delta_time);
					}
					else
					{
						Debug.LogError("ReallyUpdateCraftComponent: Linked Worker is Null");
					}
				}
			}
		}
		if (!is_crafting || current_craft == null || current_craft.is_auto || !base.wgo.is_current_craft_gratitude)
		{
			return;
		}
		if (!base.wgo.components.craft.is_gratitude_points_spent_for_craft)
		{
			int num2 = 0;
			if (current_craft.gratitude_points_craft_cost != null)
			{
				num2 = Mathf.RoundToInt(current_craft.gratitude_points_craft_cost.EvaluateFloat(MainGame.me.player) * (1f - base.wgo.progress));
			}
			if (!TrySpendPlayerGratitudePoints(num2))
			{
				return;
			}
			RefreshComponentBubbleData(show_interaction_buttons: false);
		}
		base.wgo.auto_craft_time_spent += delta_time;
		float num3 = (current_craft.craft_time_is_zero ? 0.01f : 0.5f);
		if (current_craft.craft_time.has_expression)
		{
			while (base.wgo.auto_craft_time_spent >= num3)
			{
				base.wgo.auto_craft_time_spent -= num3;
				DoAction(base.wgo, num3, for_gratitude_points: true);
			}
		}
	}

	public bool HasLinkedWorker()
	{
		if (base.wgo != null)
		{
			return base.wgo.has_linked_worker;
		}
		return false;
	}

	public override bool HasUpdate()
	{
		return true;
	}

	private bool GetCraftCoeffForPlayer(out float k)
	{
		Item equippedTool = other_obj.GetEquippedTool();
		ItemDefinition.ItemType item_type = equippedTool?.definition.type ?? ItemDefinition.ItemType.None;
		if (base.wgo.is_removing)
		{
			k = 1f;
			return true;
		}
		if (!base.wgo.obj_def.tool_actions.GetToolK(item_type, out k))
		{
			return false;
		}
		if (equippedTool != null)
		{
			k *= equippedTool.definition.efficiency;
		}
		return true;
	}

	private bool TrySpendPlayerEnergy(WorldGameObject player_wgo, float delta_time)
	{
		Item equippedTool = player_wgo.GetEquippedTool();
		float num = 1f;
		if (equippedTool != null && equippedTool.definition != null && equippedTool.definition.tool_energy_k != null && equippedTool.definition.tool_energy_k.has_expression)
		{
			num = equippedTool.definition.tool_energy_k.EvaluateFloat(base.wgo, player_wgo);
		}
		float num2 = current_craft.craft_time.EvaluateFloat(base.wgo, player_wgo);
		if (num2.EqualsTo(0f))
		{
			Debug.LogWarning("Time = 0 in craft id = " + current_craft.id, base.wgo);
			return true;
		}
		float num3 = current_craft.energy.EvaluateFloat(base.wgo, player_wgo) * delta_time / num2;
		num3 *= num;
		return player_wgo.components.character.player.TrySpendEnergy(num3);
	}

	public bool CanSpendPlayerEnergy(WorldGameObject player_wgo, float delta_time)
	{
		if (!is_crafting || current_craft == null)
		{
			return false;
		}
		float num = current_craft.craft_time.EvaluateFloat(base.wgo, player_wgo);
		if (num.EqualsTo(0f))
		{
			Debug.LogWarning("Time = 0 in craft id = " + current_craft.id, base.wgo);
			return true;
		}
		float num2 = current_craft.energy.EvaluateFloat(base.wgo) * delta_time / num;
		return player_wgo.energy >= num2;
	}

	public bool TrySpendPlayerGratitudePoints(float value)
	{
		if (CanSpendPlayerGratitudePoints(value))
		{
			MainGame.me.player.gratitude_points -= value;
			is_gratitude_points_spent_for_craft = true;
			return true;
		}
		return false;
	}

	public void ReturnPlayerGratitudePoints()
	{
		if (current_craft != null && current_craft.gratitude_points_craft_cost != null)
		{
			float num = (current_craft.gratitude_points_craft_cost?.EvaluateFloat()).Value;
			MainGame.me.player.gratitude_points += num;
		}
	}

	public bool CanSpendPlayerGratitudePoints(float value)
	{
		return MainGame.me.player.gratitude_points >= value;
	}

	private void SpendPlayerSanity(WorldGameObject player_wgo, float delta_time)
	{
		float need_sanity = current_craft.sanity.EvaluateFloat(base.wgo) * delta_time / current_craft.craft_time.EvaluateFloat(base.wgo, player_wgo);
		player_wgo.GetComponent<PlayerComponent>().SpendSanity(need_sanity);
	}

	public override void UpdateEnableState(ObjectDefinition.ObjType obj_type)
	{
		base.enabled = base.wgo.obj_def.has_craft || base.wgo.is_removing;
	}

	public SerializableWGO.SerializableCraft GetSerializedCraftComponent()
	{
		SerializableWGO.SerializableCraft result = default(SerializableWGO.SerializableCraft);
		result.available = base.enabled;
		result.multiquality_item_id = _multiquality_craft_item_id;
		result.multiquality_craft_result = _multiquality_craft_result;
		result.last_craft_id = _last_craft_id;
		result.last_craft_id_2 = _last_craft_id_2;
		result.cur_last_craft_slot = _cur_last_craft_slot;
		result.cur_craft_items_used = _cur_craft_items_used;
		if (!base.enabled)
		{
			return result;
		}
		result.is_crafting = is_crafting && current_craft != null;
		if (result.is_crafting)
		{
			result.cur_craft_id = current_craft.id;
			result.craft_amount = craft_amount;
		}
		result.queue = craft_queue;
		if (_current_item == null)
		{
			result.cur_item_id = "";
		}
		else
		{
			result.cur_item_id = _current_item.id;
			result.cur_item_dur = _current_item.durability;
		}
		if (_dur_item == null)
		{
			result.dur_item_id = "";
		}
		else
		{
			result.dur_item_id = _dur_item.id;
			result.dur_item_dur = _dur_item.durability;
		}
		result.is_gratitude_points_spent_for_craft = is_gratitude_points_spent_for_craft;
		return result;
	}

	private CraftDefinition DeserializeCraftDefinition(string craft_id)
	{
		if (string.IsNullOrEmpty(craft_id))
		{
			return null;
		}
		return GameBalance.me.GetDataOrNull<CraftDefinition>(craft_id) ?? GameBalance.me.GetData<ObjectCraftDefinition>(craft_id);
	}

	public void DeserializeCraftComponent(SerializableWGO.SerializableCraft data)
	{
		base.enabled = data.available;
		_last_craft_id = data.last_craft_id;
		_last_craft_id_2 = data.last_craft_id_2;
		_cur_last_craft_slot = data.cur_last_craft_slot;
		if (base.enabled)
		{
			is_crafting = data.is_crafting;
			if (is_crafting)
			{
				craft_amount = data.craft_amount;
			}
			_multiquality_craft_item_id = data.multiquality_item_id;
			_multiquality_craft_result = data.multiquality_craft_result;
			current_craft = DeserializeCraftDefinition(data.cur_craft_id);
			craft_queue = data.queue ?? new List<CraftQueueItem>();
			_cur_craft_items_used = data.cur_craft_items_used ?? new List<Item>();
			_current_item = (string.IsNullOrEmpty(data.cur_item_id) ? null : new Item(data.cur_item_id, 1)
			{
				durability = data.cur_item_dur
			});
			_dur_item = (string.IsNullOrEmpty(data.dur_item_id) ? null : new Item(data.dur_item_id, 1)
			{
				durability = data.dur_item_dur
			});
			is_gratitude_points_spent_for_craft = data.is_gratitude_points_spent_for_craft;
			base.wgo.OnCraftStateChanged();
			if (is_crafting)
			{
				OnEnabled();
			}
		}
	}

	private void ShowSurveyCompleteWindow(CraftDefinition craft)
	{
		Item item = craft.needs[0];
		if (GameBalance.me.GetDataOrNull<ItemDefinition>(item.id) == null)
		{
			List<string> itemsOfBaseName = GameBalance.me.GetItemsOfBaseName(item.id);
			if (itemsOfBaseName.Count <= 0)
			{
				Debug.LogError("Couldn't show survey complete window for item id = " + item.id);
				return;
			}
			item = new Item(itemsOfBaseName[0], item.value);
		}
		ItemDefinition.ItemDetails itemDetails = item.definition.GetItemDetails();
		if (itemDetails == null)
		{
			Debug.LogError("Couldn't get survey details for item " + item.id);
			return;
		}
		string text = "";
		if (itemDetails.alchemy != null)
		{
			if (itemDetails.alchemy.details_type != 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			string text2 = "";
			foreach (int decompose in itemDetails.alchemy.decomposes)
			{
				if (!string.IsNullOrEmpty(text2))
				{
					text2 += ", ";
				}
				text2 = text2 + "(alcd" + decompose + ")" + GJL.L("alc_ingr_" + decompose);
			}
			if (string.IsNullOrEmpty(text2))
			{
				text2 = "-";
			}
			text = "\n" + GJL.L("alch_decompose_full") + GJL.L(":") + "\n[AAAEBBFF]" + text2 + "[-]";
		}
		GUIElements.me.dialog.OpenDialog(GJL.L("survey_complete_1"), GJL.L("OK"), ProcessFinishedCraft, null, null, null, GameKey.Select, GameKey.Back, text, new Item(item.id, 0));
		GUIElements.me.dialog.item_container.GetComponent<BaseItemCellGUI>().quality_icon.gameObject.SetActive(value: false);
	}

	public static void ClearCraftsListOnGameStart()
	{
		Debug.Log("ClearCraftsListOnGameStart");
		_all_crafts.Clear();
	}

	public static void UpdateAllCrafts(float delta_time)
	{
		_all_crafts_iterating = true;
		_crafts_to_add = new List<CraftComponent>();
		_crafts_to_del = new List<CraftComponent>();
		foreach (CraftComponent all_craft in _all_crafts)
		{
			try
			{
				all_craft.ReallyUpdateComponent(delta_time);
			}
			catch (Exception exception)
			{
				Debug.LogError("Error while updating craft component, item " + all_craft?._current_item);
				Debug.LogException(exception);
			}
		}
		foreach (CraftComponent item in _crafts_to_del)
		{
			_all_crafts.Remove(item);
		}
		foreach (CraftComponent item2 in _crafts_to_add)
		{
			_all_crafts.Add(item2);
		}
		_all_crafts_iterating = false;
	}

	public override void RefreshComponentBubbleData(bool show_interaction_buttons)
	{
		BubbleWidgetItemData bubbleWidgetItemData = null;
		BubbleWidgetProgressData wdata = null;
		string text = "";
		string text2 = "";
		if (base.wgo.obj_def == null)
		{
			Debug.LogError("Object definition is null for object id = " + base.wgo.obj_id, base.wgo);
			return;
		}
		ObjectInteractionDefinition validInteraction = base.wgo.obj_def.GetValidInteraction(base.wgo);
		if (!string.IsNullOrEmpty(validInteraction?.hint))
		{
			text = GameKeyTip.Get(GameKey.Interaction, validInteraction.hint);
		}
		if (is_crafting)
		{
			Item item = null;
			bool show_quality = true;
			if (current_craft == null)
			{
				Debug.LogError("Current_craft is null", base.wgo);
				return;
			}
			if (current_craft.hidden)
			{
				return;
			}
			if (current_craft.craft_type != 0)
			{
				text = "";
			}
			switch (current_craft.craft_type)
			{
			case CraftDefinition.CraftType.None:
			case CraftDefinition.CraftType.ResourcesBasedCraft:
			case CraftDefinition.CraftType.AlchemyDecompose:
				if (base.wgo.obj_id == "grave_ground" && current_craft.needs.Count > 0 && current_craft.id.Contains("set_"))
				{
					item = new Item(current_craft.needs[0]);
				}
				else if (current_craft.IsBodyPartInsertionCraft() && _current_item != null)
				{
					item = new Item(_current_item);
				}
				else if (!current_craft.id.Contains(":r:") && current_craft.GetFirstRealOutput() != null)
				{
					string item_id = (current_craft.output[0].is_multiquality ? current_craft.output[0].multiquality_items[0] : current_craft.output[0].id);
					show_quality = !current_craft.output[0].is_multiquality;
					item = new Item(item_id);
				}
				break;
			case CraftDefinition.CraftType.Survey:
				if (current_craft.needs.Count > 0)
				{
					item = current_craft.needs[0];
				}
				break;
			case CraftDefinition.CraftType.MixedCraft:
				if (!MainGame.me.save.completed_one_time_crafts.Contains(current_craft.id))
				{
					item = new Item("unknown", 1);
				}
				else if (current_craft.GetFirstRealOutput() != null)
				{
					item = new Item(current_craft.output[0]);
				}
				break;
			}
			if (item != null)
			{
				_ = !current_craft.is_auto || show_interaction_buttons;
				string item_id2 = item.id;
				if (!string.IsNullOrEmpty(_multiquality_craft_item_id) && current_craft.craft_type != 0)
				{
					item_id2 = _multiquality_craft_item_id;
				}
				bubbleWidgetItemData = new BubbleWidgetItemData(item_id2, show_back: true, show_quality, GetCraftAnmountCounter(), IsCraftCounterInfinite());
				if (_worker_is_paused)
				{
					bubbleWidgetItemData.cap_limit = true;
				}
				bubbleWidgetItemData.is_gratitude = base.wgo.is_current_craft_gratitude;
				bubbleWidgetItemData.is_enough_gratitude = is_gratitude_points_spent_for_craft || current_craft.is_auto;
				if (!string.IsNullOrEmpty(current_craft.icon) && current_craft.craft_type == CraftDefinition.CraftType.None)
				{
					bubbleWidgetItemData.icon_id = current_craft.icon;
					if (current_craft.hide_quality_icon)
					{
						bubbleWidgetItemData.show_quality = false;
					}
				}
			}
			else if (base.wgo.is_removing || current_craft.id.Contains(":r:"))
			{
				bubbleWidgetItemData = new BubbleWidgetItemData
				{
					icon_id = "i_b_remove"
				};
			}
			if (!current_craft.hidden && (!current_craft.is_auto || show_interaction_buttons))
			{
				wdata = new BubbleWidgetProgressData(() => base.wgo.progress);
			}
			if (current_craft.id.StartsWith("camp_kitchen") || current_craft.id.StartsWith("refugee_honey_production"))
			{
				show_quality = !current_craft.output[0].is_multiquality;
				bubbleWidgetItemData = new BubbleWidgetItemData(current_craft.output[0].id, show_back: true, show_quality, GetCraftAnmountCounter(), IsCraftCounterInfinite());
				wdata = new BubbleWidgetProgressData(() => base.wgo.progress);
			}
		}
		else if (!IsCraftQueueEmpty())
		{
			List<Item> output = craft_queue[0].craft.output;
			if (output.Count != 0)
			{
				bubbleWidgetItemData = new BubbleWidgetItemData(output[0].id);
				if (!string.IsNullOrEmpty(craft_queue[0].craft.icon))
				{
					bubbleWidgetItemData.icon_id = craft_queue[0].craft.icon;
					if (craft_queue[0].craft.hide_quality_icon)
					{
						bubbleWidgetItemData.show_quality = false;
					}
				}
				bubbleWidgetItemData.is_gratitude = !is_crafting && HasGratitudeCraftInQueue();
				bubbleWidgetItemData.is_enough_gratitude = false;
			}
		}
		if (show_interaction_buttons && CanInteractCraft() && string.IsNullOrEmpty(text))
		{
			text = GameKeyTip.Get(GameKey.Interaction, "craft_hint");
		}
		else if (is_crafting && !CanInteractCraft())
		{
			text = "";
		}
		base.wgo.SetBubbleWidgetData(bubbleWidgetItemData, BubbleWidgetData.WidgetID.CraftingItem);
		base.wgo.SetBubbleWidgetData(wdata, BubbleWidgetData.WidgetID.CraftingProgress);
		if (show_interaction_buttons)
		{
			if (PlayerCanWork())
			{
				text2 = GameKeyTip.Get(GameKey.Work, "work");
				MainGame.me.player.components.character.wgo_hilighted_for_work = base.wgo;
			}
		}
		else
		{
			text = "";
			text2 = "";
		}
		base.wgo.SetBubbleWidgetData(text, BubbleWidgetData.WidgetID.Interaction);
		if (!string.IsNullOrEmpty(text2))
		{
			base.wgo.SetBubbleWidgetData(text2, BubbleWidgetData.WidgetID.Work);
		}
		if (MainGame.game_started && string.IsNullOrEmpty(text2) && MainGame.me.player.components.character.wgo_hilighted_for_work == base.wgo)
		{
			MainGame.me.player.components.character.wgo_hilighted_for_work = null;
		}
	}

	private bool PlayerCanWork()
	{
		if (base.wgo.has_linked_worker)
		{
			return false;
		}
		if (base.wgo.player_cant_work)
		{
			return false;
		}
		if (is_crafting && !current_craft.is_auto)
		{
			return true;
		}
		if (!IsCraftQueueEmpty())
		{
			return true;
		}
		return false;
	}

	private int GetCraftAnmountCounter()
	{
		if (!base.wgo.obj_def.can_insert_zombie)
		{
			return craft_amount;
		}
		if (IsCraftQueueEmpty())
		{
			return 1;
		}
		foreach (CraftQueueItem item in craft_queue)
		{
			if (item.craft == current_craft)
			{
				return item.n + 1;
			}
		}
		return 1;
	}

	private bool IsCraftCounterInfinite()
	{
		foreach (CraftQueueItem item in craft_queue)
		{
			if (item.craft == current_craft && item.infinite)
			{
				return true;
			}
		}
		return false;
	}

	private void SetWorkerPausedMode(bool paused)
	{
		if (_worker_is_paused != paused)
		{
			_worker_is_paused = paused;
			base.components.RefreshBubblesData(null);
		}
	}

	private void DistributeDropsFromSoulsCraft(List<Item> drop_list)
	{
		List<Item> list = new List<Item>();
		for (int i = 0; i < drop_list.Count; i++)
		{
			if (drop_list[i].is_tech_point)
			{
				list.Add(drop_list[i]);
				drop_list.RemoveAt(i);
				i--;
			}
		}
		base.wgo.PutToAllPossibleInventories(drop_list, out var cant_insert);
		base.wgo.DropItems(cant_insert);
		base.wgo.DropItems(list);
	}
}
