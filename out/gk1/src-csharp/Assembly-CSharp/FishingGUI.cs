using System;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using Fishing;
using UnityEngine;

public class FishingGUI : BaseGUI
{
	public enum FishingState
	{
		None,
		BaitChoosing,
		DistanceChoosing,
		WaitingForBite,
		WaitingForPulling,
		Pulling,
		TakingOut
	}

	private const float THROWING_DISTANCE_BAR_FILLING_TIME = 2f;

	private const float THROWING_ENERGY_COST_MODIFICATOR = 2f;

	private const string FISHING_STATE = "fishing_state";

	private const string FISHING_DIST = "fishing_distance";

	private const string START_FISHING_TRIGGER = "start_fishing";

	private const string START_THROWING_TRIGGER = "on_fishing_throw";

	private const string FISH_CATCH_TIME_MLTPLR = "buff_fish_catch_time_mltplr";

	public GameObject bait_choosing_go;

	public GameObject distance_choosing_go;

	public GameObject pulling_go;

	public UIWidget fishing_rod;

	public UIWidget process_back;

	public FishingDistanceChoosingBarGUI distance_choosing_bar;

	public UIProgressBar progress_bar;

	public Transform fish_tf;

	public UILabel bait_name;

	public UILabel bait_decription;

	public BaseItemCellGUI bait_cell_gui;

	public bool taking_out_animation_finished;

	public bool can_take_out;

	private bool is_success_fishing;

	public List<FishWithWeight>[] fishes_with_weights = new List<FishWithWeight>[3];

	private FishingState _state;

	private float _waiting_for_bite_delay;

	private float _throwing_distance;

	private int _throwing_distance_int;

	private bool _throwing_dist_bar_increasing;

	private List<Item> _avaliable_baits;

	private int _cur_bait_num;

	private FishDefinition _fish_def;

	private Item _fish;

	private FishPreset _fish_preset;

	private float _waiting_for_pulling_time;

	private Item _equipped_fishing_rod;

	private float _screen_k;

	private Transform _fishing_rod_tf;

	private FishingRodLogic _fishing_rod_logic;

	private FishLogic _fish_logic;

	private WorldGameObject _fishing_spot_wgo;

	private CustomNetworkAnimatorSync _player_animator;

	public bool is_to_right;

	public UILabel txt_casting_hint;

	public UILabel txt_pull_hint;

	public SimpleUITable bait_text_container;

	public int pull_pos_x_left = -90;

	public int pull_pos_x_right = 90;

	public ReservoirsDefinition reservoir_data { get; private set; }

	public FishingState state => _state;

	public override void Init()
	{
		_fishing_rod_tf = fishing_rod.transform;
		base.Init();
	}

	public void Open(WorldGameObject fishing_spot)
	{
		_equipped_fishing_rod = MainGame.me.player.GetEquippedItem(ItemDefinition.EquipmentType.FishingRod);
		if (_equipped_fishing_rod == null)
		{
			Debug.LogError("No fishing rod!");
			MainGame.me.player.Say("no_fishing_rod");
			return;
		}
		_fishing_spot_wgo = fishing_spot;
		if (_fishing_spot_wgo == null)
		{
			Debug.LogError("Fishing spot is null!");
			return;
		}
		reservoir_data = GameBalance.me.GetData<ReservoirsDefinition>(_fishing_spot_wgo.obj_id);
		if (reservoir_data != null)
		{
			RecalcAvaliableBaits();
			LoadLastBait();
			GUIElements.me.hud.ToolbarSetEnabled(enabled: false);
			Open(play_open_sound: false);
			MainGame.me.player_char.SetAnimationState(CharAnimState.Fishing);
			_player_animator = MainGame.me.player.components.animator;
			_player_animator.SetTrigger("start_fishing");
			_player_animator.SetFloat("fishing_distance", 0f);
			_player_animator.SetInteger("fishing_state", 0);
			GDPoint componentInChildren = _fishing_spot_wgo.GetComponentInChildren<GDPoint>();
			if (componentInChildren != null && componentInChildren.direction == Direction.Right)
			{
				MainGame.me.player.gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
				is_to_right = true;
			}
			else
			{
				is_to_right = false;
			}
			is_success_fishing = false;
			Debug.Log("#fishing# Started fishing on [" + _fishing_spot_wgo.obj_id + "] with rod \"" + _equipped_fishing_rod.id + "\", is_to_right = " + is_to_right);
			pulling_go.transform.localPosition = new Vector2(is_to_right ? pull_pos_x_right : pull_pos_x_left, pulling_go.transform.localPosition.y);
			RedrawSelectedBait();
			ChangeState(FishingState.BaitChoosing);
			txt_pull_hint.text = PlatformSpecific.GetInteractionButtonHint(BaseGUI.for_gamepad);
		}
	}

	public static void DrawCanHookFishHint()
	{
		MainGame.me.player.DrawFishingPullBubble("pull_fish");
	}

	private void ChangeState(FishingState target_state)
	{
		Debug.Log("#fishing# Changing state: \"" + _state.ToString() + "\" => \"" + target_state.ToString() + "\"");
		_state = target_state;
		_player_animator.SetInteger("fishing_state", (int)_state);
		MainGame.me.player.components.interaction.RedrawCurrentInteractiveHint();
		switch (_state)
		{
		case FishingState.BaitChoosing:
			UpdateFishesWithWeights(update_gui: false);
			distance_choosing_bar.Init();
			txt_casting_hint.text = GJL.L("fish_casting_hint", PlatformSpecific.GetInteractionButtonHint(BaseGUI.for_gamepad));
			MainGame.me.player.DrawFishingPullBubble("");
			break;
		case FishingState.DistanceChoosing:
			_throwing_distance = 0f;
			_throwing_dist_bar_increasing = true;
			_player_animator.SetTrigger("on_fishing_throw");
			distance_choosing_bar.SetMarkerActive(set_active: true);
			distance_choosing_bar.SetMarkerPos(0f);
			txt_casting_hint.text = GJL.L("fish_casting_hint_2", PlatformSpecific.GetInteractionButtonHint(BaseGUI.for_gamepad));
			MainGame.me.player.DrawFishingPullBubble("");
			can_take_out = false;
			break;
		case FishingState.WaitingForBite:
		{
			LazyInput.WaitForRelease(GameKey.MiniGameAction);
			float num = _equipped_fishing_rod.definition.params_on_use.Get("energy") * -1f;
			if (num > 0f && !MainGame.me.player.components.character.player.TrySpendEnergy(num * 2f))
			{
				_state = FishingState.None;
				EffectBubblesManager.ShowImmediately(MainGame.me.player.bubble_pos, GJL.L("not_enough_something", "(en)"), EffectBubblesManager.BubbleColor.Energy);
				OnPressedBack();
				return;
			}
			FishDefinition randomFish = GetRandomFish(out _waiting_for_bite_delay);
			txt_casting_hint.text = "";
			if (randomFish == null)
			{
				_state = FishingState.None;
				Debug.Log("#fishing# Nothing to bite here.");
				MainGame.me.player.Say("fishing_something_wrong");
				OnPressedBack();
				return;
			}
			MainGame.me.player.DrawFishingPullBubble("");
			_fish_def = randomFish;
			break;
		}
		case FishingState.WaitingForPulling:
		{
			LazyInput.WaitForRelease(GameKey.MiniGameAction);
			float param = MainGame.me.player.data.GetParam("buff_fish_catch_time_mltplr");
			_waiting_for_pulling_time = (((double)Mathf.Abs(param) > 0.01) ? (_fish_preset.catch_time * param) : _fish_preset.catch_time);
			txt_casting_hint.text = "";
			break;
		}
		case FishingState.Pulling:
		{
			LazyInput.WaitForRelease(GameKey.MiniGameAction);
			txt_casting_hint.text = "";
			_fish_logic = new FishLogic(_fish_preset);
			FishingRodPreset fishingRodPreset = Resources.Load<FishingRodPreset>("MiniGames/Fishing/" + _equipped_fishing_rod.id);
			_fishing_rod_logic = ((fishingRodPreset != null) ? new FishingRodLogic(fishingRodPreset) : null);
			if (_fishing_rod_logic != null)
			{
				_screen_k = (float)process_back.height / 100f;
				fishing_rod.height = Mathf.RoundToInt(_fishing_rod_logic.rect_size * _screen_k);
			}
			_fishing_rod_tf.localPosition = Vector3.zero;
			UpdatePulling();
			Sounds.PlaySound("fishing_reel_long");
			MainGame.me.player.DrawFishingPullBubble("bump_fish");
			break;
		}
		case FishingState.TakingOut:
			StopReelSound();
			txt_casting_hint.text = "";
			MainGame.me.player.DrawFishingPullBubble("");
			taking_out_animation_finished = false;
			if (_cur_bait_num != -1)
			{
				SaveLastBait();
				RemoveBait(_avaliable_baits[_cur_bait_num]);
			}
			if (is_success_fishing)
			{
				MainGame.me.player_char.player.fish.sprite = EasySpritesCollection.GetSprite(_fish.GetIcon());
				MainGame.me.player_char.player.fishadow.sprite = EasySpritesCollection.GetSprite("fish_shadow");
			}
			else
			{
				MainGame.me.player_char.player.fish.sprite = null;
				MainGame.me.player_char.player.fishadow.sprite = null;
			}
			break;
		}
		distance_choosing_go.SetActive(_state == FishingState.DistanceChoosing || _state == FishingState.BaitChoosing);
		pulling_go.SetActive(_state == FishingState.Pulling);
		bait_choosing_go.SetActive(_state == FishingState.BaitChoosing);
	}

	private void StopReelSound()
	{
		MasterAudio.StopAllOfSound("fishing_reel_long");
	}

	public override void Update()
	{
		base.Update();
		switch (_state)
		{
		case FishingState.BaitChoosing:
			UpdateBaitChoosing();
			break;
		case FishingState.DistanceChoosing:
			UpdateDistanceChoosing();
			break;
		case FishingState.WaitingForBite:
			UpdateWaitingForBite();
			break;
		case FishingState.WaitingForPulling:
			UpdateWaitingForPulling();
			break;
		case FishingState.Pulling:
			UpdatePulling();
			break;
		case FishingState.TakingOut:
			UpdateTakingOut();
			break;
		}
	}

	private void UpdateBaitChoosing()
	{
		if (LazyInput.GetKeyDown(GameKey.NextTab))
		{
			OnNextBait();
		}
		if (LazyInput.GetKeyDown(GameKey.PrevTab))
		{
			OnPrevBait();
		}
		if (LazyInput.GetKeyDown(GameKey.Interaction))
		{
			ChangeState(FishingState.DistanceChoosing);
		}
	}

	private void UpdateDistanceChoosing()
	{
		if (LazyInput.GetKey(GameKey.Interaction))
		{
			if (_throwing_dist_bar_increasing)
			{
				_throwing_distance += Time.deltaTime / 2f;
			}
			else
			{
				_throwing_distance -= Time.deltaTime / 2f;
			}
			if (_throwing_distance > 1f)
			{
				_throwing_dist_bar_increasing = false;
				_throwing_distance = 1f;
			}
			else if (_throwing_distance < 0f)
			{
				_throwing_dist_bar_increasing = true;
				_throwing_distance = 0f;
			}
			distance_choosing_bar.SetMarkerPos(_throwing_distance);
			return;
		}
		int num = Mathf.CeilToInt(_throwing_distance * 3f) - 1;
		if (num < 0)
		{
			num = 0;
		}
		if (num > 2)
		{
			num = 2;
		}
		if (reservoir_data.dist_avaliables[num] && fishes_with_weights[num].Count > 0)
		{
			Debug.Log("#fishing# Choosed throwing distance: " + (num + 1));
			_throwing_distance_int = num + 1;
			_player_animator.SetFloat("fishing_distance", _throwing_distance_int);
			ChangeState(FishingState.WaitingForBite);
		}
		else
		{
			Debug.Log("#fishing# Can not throw to dist " + (num + 1));
			ChangeState(FishingState.BaitChoosing);
		}
	}

	private void UpdateWaitingForBite()
	{
		if (!can_take_out)
		{
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.MiniGameAction))
		{
			is_success_fishing = false;
			ChangeState(FishingState.TakingOut);
			return;
		}
		_waiting_for_bite_delay -= Time.deltaTime;
		if (!(_waiting_for_bite_delay > 0f))
		{
			_fish = new Item(_fish_def.item_id, 1);
			_fish_preset = Resources.Load<FishPreset>("MiniGames/Fishing/" + _fish_def.fish_preset);
			if (_fish_preset != null)
			{
				ChangeState(FishingState.WaitingForPulling);
			}
		}
	}

	private void UpdateFishesWithWeights(bool update_gui = true)
	{
		for (int i = 0; i < 3; i++)
		{
			fishes_with_weights[i] = new List<FishWithWeight>();
		}
		bool is_night = TimeOfDay.me.is_night;
		string obj_id = _fishing_spot_wgo.obj_id;
		string bait = ((_cur_bait_num == -1) ? string.Empty : _avaliable_baits[_cur_bait_num].id);
		if (!_equipped_fishing_rod.id.StartsWith("fishing_rod_"))
		{
			Debug.LogError("Wrong equipped fishing rod: \"" + _equipped_fishing_rod.id + "\"");
		}
		string text = _equipped_fishing_rod.id.Split(new string[1] { "fishing_rod_" }, StringSplitOptions.None)[1];
		if (!int.TryParse(text, out var result))
		{
			Debug.LogError("Wrong equipped fishing rod: can not parse rod_lvl \"" + text + "\"");
			return;
		}
		foreach (FishDefinition fishes_datum in GameBalance.me.fishes_data)
		{
			for (int j = 0; j < 3; j++)
			{
				if (!reservoir_data.dist_avaliables[j])
				{
					continue;
				}
				float totalWeight = fishes_datum.GetTotalWeight(obj_id, is_night, j + 1, result, bait);
				bool flag = false;
				if (totalWeight < 0.01f)
				{
					float num = 0f;
					bool[] array = new bool[2] { true, false };
					foreach (bool is_night2 in array)
					{
						for (int l = 0; l < 3; l++)
						{
							num += fishes_datum.GetTotalWeight(obj_id, is_night2, j + 1, l, string.Empty);
							if (num > 0f)
							{
								break;
							}
							for (int m = 0; m < GameBalance.me.items_data.Count; m++)
							{
								ItemDefinition itemDefinition = GameBalance.me.items_data[m];
								if (itemDefinition.type == ItemDefinition.ItemType.Bait)
								{
									num += fishes_datum.GetTotalWeight(obj_id, is_night2, j + 1, l, itemDefinition.id);
									if (num > 0f)
									{
										break;
									}
								}
							}
							if (num > 0f)
							{
								break;
							}
						}
						if (num > 0f)
						{
							break;
						}
					}
					if (num > 0f)
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					fishes_with_weights[j].Add(new FishWithWeight
					{
						fish = fishes_datum,
						weight = totalWeight
					});
				}
			}
		}
		if (update_gui)
		{
			distance_choosing_bar.UpdateBar();
		}
	}

	private FishDefinition GetRandomFish(out float waiting_time)
	{
		UpdateFishesWithWeights(update_gui: false);
		waiting_time = 0f;
		float num = 0f;
		float num2 = 0f;
		string text = ((_cur_bait_num == -1) ? string.Empty : _avaliable_baits[_cur_bait_num].id);
		List<FishWithWeight> list = fishes_with_weights[_throwing_distance_int - 1];
		if (list.Count == 0)
		{
			return null;
		}
		bool flag = false;
		string text2 = string.Empty;
		foreach (FishWithWeight item in list)
		{
			num2 += item.weight;
			if (item.weight > 0f)
			{
				flag = true;
				text2 = text2 + (string.IsNullOrEmpty(text2) ? "" : ", ") + item.fish.id;
			}
		}
		if (!flag)
		{
			return null;
		}
		FishDefinition fishDefinition = null;
		float num3 = UnityEngine.Random.Range(0f, num2);
		num = 0f;
		foreach (FishWithWeight item2 in list)
		{
			num += item2.weight;
			if (num > num3)
			{
				fishDefinition = item2.fish;
				break;
			}
		}
		if (fishDefinition == null)
		{
			Debug.LogError("FATAL ERROR while fishing! random_fish not found");
			return null;
		}
		if (string.IsNullOrEmpty(text))
		{
			waiting_time = fishDefinition.no_bait_mod.wait_time;
		}
		else
		{
			waiting_time = fishDefinition.no_bait_mod.wait_time;
			foreach (FishDefinition.BaitData item3 in fishDefinition.baits_mod)
			{
				if (item3.bait_name == text)
				{
					waiting_time = item3.wait_time;
					break;
				}
			}
		}
		waiting_time *= UnityEngine.Random.Range(0.9f, 1.1f);
		Debug.Log("#fishing# Total_weight=" + num2 + ", rand=" + num3 + " => fish=" + fishDefinition.id + ", waiting_time=" + waiting_time + "\n Available fishes: {" + text2 + "}");
		return fishDefinition;
	}

	private void UpdateWaitingForPulling()
	{
		if (!(_fish_preset == null))
		{
			if (LazyInput.GetKeyDown(GameKey.MiniGameAction))
			{
				ChangeState(FishingState.Pulling);
			}
			_waiting_for_pulling_time -= Time.deltaTime;
			if (_waiting_for_pulling_time < 0f)
			{
				ChangeState(FishingState.WaitingForBite);
			}
		}
	}

	private void UpdatePulling()
	{
		if (_fishing_rod_logic != null)
		{
			float num = _fishing_rod_logic.CalculateRodPos();
			FishLogic.Result result = _fish_logic.CalculateFishPos(num, _fishing_rod_logic.rect_size);
			progress_bar.value = result.progress;
			_fishing_rod_tf.localPosition = Vector3.up * (num * _screen_k);
			fish_tf.localPosition = Vector3.up * (result.fish_pos * _screen_k);
			if (result.success || result.fail)
			{
				is_success_fishing = result.success;
				ChangeState(FishingState.TakingOut);
			}
		}
	}

	private void UpdateTakingOut()
	{
		if (!taking_out_animation_finished)
		{
			return;
		}
		OnPressedBack();
		MainGame.me.player.components.interaction.UpdateNearestHint();
		if (!is_success_fishing)
		{
			return;
		}
		MainGame.me.save.achievements.CheckKeyQuests("fishing_success");
		if (MainGame.me.player.AddToInventory(_fish))
		{
			DropCollectGUI.OnDropCollected(_fish);
			Sounds.PlaySound("pickup");
		}
		else
		{
			MainGame.me.player.DropItem(_fish);
		}
		string nameWithoutQualitySuffix = _fish.definition.GetNameWithoutQualitySuffix();
		string item = reservoir_data.id + ":" + _throwing_distance_int + ":" + nameWithoutQualitySuffix;
		Stats.DesignEvent("Fishing:" + ((reservoir_data == null) ? "NULL" : reservoir_data.id) + ":" + ((_fish == null) ? "NULL" : _fish.id));
		if (!MainGame.me.save.known_fishes_clear.Contains(nameWithoutQualitySuffix))
		{
			MainGame.me.save.known_fishes_clear.Add(nameWithoutQualitySuffix);
			MainGame.me.save.achievements.CheckKeyQuests("new_fish");
			MainGame.me.save.achievements.CheckKeyQuests("new_fish_" + nameWithoutQualitySuffix);
		}
		if (MainGame.me.save.known_fishes.Contains(item))
		{
			return;
		}
		MainGame.me.save.known_fishes.Add(item);
		string id = ((nameWithoutQualitySuffix == "fish_frog_green") ? "raw_meat_sliced_from_fish_frog_green" : (nameWithoutQualitySuffix + "_fillet"));
		CraftDefinition craftDefinition = GameBalance.me.GetDataOrNull<CraftDefinition>(id);
		if (craftDefinition == null)
		{
			foreach (CraftDefinition craft_datum in GameBalance.me.craft_data)
			{
				bool flag = false;
				foreach (Item need in craft_datum.needs)
				{
					if (need.id == nameWithoutQualitySuffix)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					continue;
				}
				bool flag2 = false;
				foreach (Item item2 in craft_datum.output)
				{
					if (item2.id.Contains("fillet_fish"))
					{
						craftDefinition = craft_datum;
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					break;
				}
			}
		}
		if (craftDefinition != null)
		{
			MainGame.me.save.unlocked_crafts.Add(craftDefinition.id);
			MainGame.me.save.unlocked_crafts.Add("t_" + craftDefinition.id);
		}
	}

	public override void Hide(bool play_hide_sound = true)
	{
		StopReelSound();
		MainGame.me.player.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		MainGame.me.player_char.SetAnimationState(CharAnimState.Idle);
		MainGame.me.player.DrawFishingPullBubble(string.Empty);
		_state = FishingState.None;
		GUIElements.me.hud.ToolbarSetEnabled();
		GS.SetPlayerEnable(player_enabled: true, affect_cinematic: false);
		base.Hide(play_hide_sound: false);
	}

	protected override bool OnPressedBack()
	{
		MainGame.me.player.DrawFishingPullBubble(string.Empty);
		OnClosePressed();
		return true;
	}

	public void OnPrevBait()
	{
		if (_avaliable_baits == null)
		{
			_avaliable_baits = new List<Item>();
		}
		if (_avaliable_baits.Count == 0)
		{
			_cur_bait_num = -1;
			return;
		}
		if (_cur_bait_num == -1)
		{
			_cur_bait_num += _avaliable_baits.Count;
		}
		else
		{
			_cur_bait_num--;
		}
		RedrawSelectedBait();
		UpdateFishesWithWeights();
	}

	public void OnNextBait()
	{
		if (_avaliable_baits == null)
		{
			_avaliable_baits = new List<Item>();
		}
		if (_avaliable_baits.Count == 0)
		{
			_cur_bait_num = -1;
			return;
		}
		if (_cur_bait_num == _avaliable_baits.Count - 1)
		{
			_cur_bait_num = -1;
		}
		else
		{
			_cur_bait_num++;
		}
		RedrawSelectedBait();
		UpdateFishesWithWeights();
	}

	private void RedrawSelectedBait()
	{
		Item item = ((_cur_bait_num == -1) ? null : _avaliable_baits[_cur_bait_num]);
		bait_name.text = GJL.L((item == null) ? "no_bait" : item.definition.GetItemName());
		bait_decription.text = GJL.L((item == null) ? "no_bait_descr" : item.definition.GetItemDescription());
		bait_cell_gui.DrawItem(item ?? Item.empty);
		bait_text_container.Reposition();
		Debug.Log("#fishing# Changed bait to \"" + ((_cur_bait_num == -1) ? "no_bate" : _avaliable_baits[_cur_bait_num].id) + "\"");
	}

	private void RemoveBait(Item bait)
	{
		if (_avaliable_baits == null)
		{
			RecalcAvaliableBaits();
		}
		if (_avaliable_baits.Count == 0)
		{
			Debug.LogError("FISHING ERROR: avaliable baits list is empty! Can not remove bait \"" + bait.id + "\"");
		}
		else if (_avaliable_baits.Contains(bait))
		{
			if (bait.definition.has_durability && bait.durability > bait.definition.durability_decrease_on_use_speed)
			{
				bait.durability -= bait.definition.durability_decrease_on_use_speed;
				Debug.Log("#fishing# Removed " + bait.definition.durability_decrease_on_use_speed + " durability from bait \"" + bait.id + "\" from player inventory.");
			}
			else
			{
				MainGame.me.player.data.RemoveItem(bait, 1);
				RecalcAvaliableBaits();
				_cur_bait_num = -1;
				Debug.Log("#fishing# Removed bait \"" + bait.id + "\" from player inventory. New bait: " + ((_cur_bait_num == -1) ? "No_Bait" : _avaliable_baits[_cur_bait_num].id));
			}
		}
		else
		{
			Debug.LogError("FISHING ERROR: Can not remove bait: _avaliable_baits not contains \"" + bait.id + "\"");
		}
	}

	private void RecalcAvaliableBaits()
	{
		_avaliable_baits = new List<Item>();
		string text = string.Empty;
		foreach (Item item in MainGame.me.player.data.inventory)
		{
			if (item.definition.type == ItemDefinition.ItemType.Bait)
			{
				text = text + ((_avaliable_baits.Count == 0) ? "" : ", ") + item.id;
				_avaliable_baits.Add(item);
			}
		}
		_cur_bait_num = -1;
		Debug.Log("#fishing# Recalculated avaliable baits: count=" + _avaliable_baits.Count + ((_avaliable_baits.Count > 0) ? ("\n{" + text + "}") : ""));
	}

	private void SaveLastBait()
	{
		if (MainGame.me.save.last_bait_baits == null || MainGame.me.save.last_bait_reservoirs == null || MainGame.me.save.last_bait_baits.Count != MainGame.me.save.last_bait_reservoirs.Count)
		{
			Debug.LogError("FATAL ERROR! CALL BULAT! <= SaveLastBait");
			MainGame.me.save.last_bait_baits = new List<string>();
			MainGame.me.save.last_bait_reservoirs = new List<string>();
		}
		int num = -1;
		for (int i = 0; i < MainGame.me.save.last_bait_reservoirs.Count; i++)
		{
			if (MainGame.me.save.last_bait_reservoirs[i] == reservoir_data.id)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			MainGame.me.save.last_bait_baits.Add(string.Empty);
			MainGame.me.save.last_bait_reservoirs.Add(reservoir_data.id);
			num = MainGame.me.save.last_bait_reservoirs.Count - 1;
		}
		MainGame.me.save.last_bait_baits[num] = ((_cur_bait_num >= 0) ? _avaliable_baits[_cur_bait_num].id : string.Empty);
	}

	private void LoadLastBait()
	{
		if (MainGame.me.save.last_bait_baits == null || MainGame.me.save.last_bait_reservoirs == null || MainGame.me.save.last_bait_baits.Count != MainGame.me.save.last_bait_reservoirs.Count)
		{
			Debug.LogError("FATAL ERROR! CALL BULAT! <= LoadLastBait");
			MainGame.me.save.last_bait_baits = new List<string>();
			MainGame.me.save.last_bait_reservoirs = new List<string>();
			return;
		}
		int num = -1;
		for (int i = 0; i < MainGame.me.save.last_bait_reservoirs.Count; i++)
		{
			if (MainGame.me.save.last_bait_reservoirs[i] == reservoir_data.id)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return;
		}
		string text = MainGame.me.save.last_bait_baits[num];
		for (int j = 0; j < _avaliable_baits.Count; j++)
		{
			if (_avaliable_baits[j].id == text)
			{
				_cur_bait_num = j;
				break;
			}
		}
	}
}
