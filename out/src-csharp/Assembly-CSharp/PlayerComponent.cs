using System;
using UnityEngine;

public class PlayerComponent : MonoBehaviour
{
	public const string DO_NOT_SHOW_WGO_QUALITIES = "do_not_show_wgo_qualities";

	[NonSerialized]
	public bool is_local_player;

	private LeaveTrailComponent _trail;

	private float _energy_spent_sum;

	private float _sanity_spent_sum;

	private float _gratitude_points_spent_sum;

	private WorldGameObject _wgo;

	private bool _wgo_set;

	public SpriteRenderer spr_overhead_obj;

	public SpriteRenderer spr_tool;

	public SpriteRenderer spr_tool_2;

	public SpriteRenderer fish;

	public SpriteRenderer fishadow;

	public GameObject light_go;

	public CustomNode garry_wash_talk_pos;

	public const float PLAYER_ZONE_UPDATE_PERIOD = 0.5f;

	private float _time_passed_after_zone_update;

	private DropCollectorComponent _drop_collector;

	private BuffDefinition _pray_buff;

	private bool _pray_buff_success;

	public Material player_material;

	public Color player_additional_color = Color.black;

	private CraftDefinition _pray_craft;

	private Item _throwing_item;

	private float _last_need_energy_bubble_time;

	public LeaveTrailComponent Trail => _trail;

	public WorldZone current_zone { get; private set; }

	public bool show_wgo_qualities { get; private set; }

	public WorldGameObject wgo
	{
		get
		{
			if (!_wgo_set)
			{
				_wgo_set = true;
				_wgo = GetComponent<WorldGameObject>();
			}
			return _wgo;
		}
	}

	public static GameObject GetPlayerPrefab()
	{
		return CustomNetworkManager.me.playerPrefab;
	}

	public void Awake()
	{
		Debug.Log("Player Component Awake", this);
		if (CustomNetworkManager.is_running)
		{
			base.transform.SetParent(MainGame.me.world_root, worldPositionStays: false);
		}
		_trail = new LeaveTrailComponent(wgo.components.character, "human");
	}

	private void OnDestroy()
	{
		UnsubscribeMethods();
	}

	public void OnStartLocalPlayer()
	{
		Debug.Log("<color=cyan>OnStartLocalPlayer</color>", this);
		ResetPlayerPosition();
		PlayerComponent[] componentsInChildren = MainGame.me.world_root.GetComponentsInChildren<PlayerComponent>();
		foreach (PlayerComponent playerComponent in componentsInChildren)
		{
			if (playerComponent == this)
			{
				MainGame.me.SetMainPlayer(this);
			}
			else
			{
				playerComponent.GetComponent<BaseCharacterComponent>().can_be_locally_controlled = false;
			}
		}
	}

	private void ResetPlayerPosition()
	{
		Vector3 vector = MainGame.me.save.player_position;
		if (!MainGame.loaded_from_scene_main)
		{
			vector = World.player_default_pos;
		}
		Vector3 vector2 = vector;
		Debug.Log("ResetPlayerPosition: " + vector2.ToString() + ", loaded_from_scene_main = " + MainGame.loaded_from_scene_main);
		base.gameObject.transform.SetParent(MainGame.me.world_root, worldPositionStays: false);
		base.transform.localPosition = vector;
	}

	private void InitLocalPlayer()
	{
		MainGame.me.SetCameraPlayerFollow(base.transform);
		is_local_player = true;
		wgo.is_player = true;
		ResetPlayerPosition();
		CameraTools.MoveToPos(base.transform.position);
		wgo.components.character.control_enabled = true;
		wgo.components.InitAllComponents();
		_drop_collector = new DropCollectorComponent();
		_drop_collector.Init(wgo);
		_drop_collector.StartComponent();
		BaseGUI.on_window_opened += OnAnyWindowOpenStopUsingTool;
	}

	public static PlayerComponent SpawnPlayer(bool is_local_player = true, Item inventory = null)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Prefabs.me.player_prefab);
		PlayerComponent component = gameObject.GetComponent<PlayerComponent>();
		component.wgo.is_player = true;
		component.ResetPlayerPosition();
		if (is_local_player)
		{
			MainGame.me.player = gameObject.GetComponent<WorldGameObject>();
			AStarSearcher.AddDefaultSeekerModifier(gameObject, is_player: true);
			component.InitLocalPlayer();
		}
		if (inventory != null)
		{
			component.wgo.RestoreSavedInventory(inventory);
		}
		BaseCharacterComponent character = component.wgo.components.character;
		if (is_local_player)
		{
			MainGame.me.player_char = character;
		}
		character.SetLocalPlayerState(is_local_player);
		gameObject.GetComponent<WorldGameObject>().wop.gameObject.transform.localScale = Vector3.one;
		component.show_wgo_qualities = true;
		Debug.Log("Spawning player, is_local_player = " + is_local_player, gameObject);
		return component;
	}

	public void Update()
	{
		if (!MainGame.paused)
		{
			if (is_local_player)
			{
				MainGame.me.player_pos = base.transform.position;
			}
			if (_trail != null)
			{
				_trail.CustomUpdate();
			}
			if (_drop_collector != null)
			{
				_drop_collector.UpdateComponent(Time.deltaTime);
			}
			_time_passed_after_zone_update += Time.deltaTime;
			if (_time_passed_after_zone_update > 0.5f)
			{
				UpdateZone();
			}
			player_material.SetColor("_AdditionalColour", player_additional_color);
		}
	}

	public void CheckShowWGOQuality()
	{
		show_wgo_qualities = MainGame.me.player.GetParamInt("do_not_show_wgo_qualities") == 0;
		RedrawQualities();
	}

	public void RedrawQualities(bool separate_k = false)
	{
		if (current_zone != null)
		{
			current_zone.RedrawQualities(show_wgo_qualities, separate_k);
		}
	}

	public void UpdateZone()
	{
		_time_passed_after_zone_update = 0f;
		WorldZone myWorldZone = wgo.GetMyWorldZone();
		if (myWorldZone == null)
		{
			GUIElements.me.hud.UpdateZoneInfo("...", "");
		}
		else
		{
			string description = (string.IsNullOrEmpty(myWorldZone.definition.hud_descr_str) ? "" : GJL.L(myWorldZone.definition.hud_descr_str, myWorldZone.GetQualityString()));
			GUIElements.me.hud.UpdateZoneInfo(GJL.L("zone_" + myWorldZone.id), description);
		}
		if (current_zone != myWorldZone)
		{
			if (current_zone != null)
			{
				current_zone.OnPlayerExit();
			}
			current_zone = myWorldZone;
			if (current_zone != null)
			{
				current_zone.OnPlayerEnter();
			}
		}
	}

	public void ForceTrailChange(Ground.GroudType ground_type)
	{
		_trail.SteppedOnANewSurface(ground_type);
	}

	public bool CheckEnergy(float need_energy)
	{
		return wgo.energy >= need_energy;
	}

	public bool TrySpendEnergy(float need_energy)
	{
		if (wgo.IsPlayerInvulnerable())
		{
			need_energy = 0f;
		}
		if (wgo.energy >= need_energy)
		{
			_energy_spent_sum += need_energy;
			wgo.energy -= need_energy;
			if (_energy_spent_sum >= 1f)
			{
				int num = Mathf.FloorToInt(_energy_spent_sum);
				_energy_spent_sum -= num;
				EffectBubblesManager.ShowStackedEnergy(wgo, -num);
			}
			return true;
		}
		if (wgo.is_player && wgo.components.character.anim_state != CharAnimState.Fishing)
		{
			wgo.components.character.SetAnimationState(CharAnimState.Idle);
		}
		wgo.components.tool.StopUsingTool(work_holded: true);
		ShowNeedEnergyBubble();
		return false;
	}

	public bool IsEnoughEnergyToWork()
	{
		WorldGameObject nearest = wgo.components.interaction.nearest;
		float deltaTime = Time.deltaTime;
		if (nearest == null)
		{
			return false;
		}
		CraftComponent craft = nearest.components.craft;
		if (craft.enabled && craft.is_crafting && !craft.current_craft.is_auto)
		{
			return craft.CanSpendPlayerEnergy(wgo, deltaTime);
		}
		return nearest.components.hp.CanSpendPlayerEnergy(wgo, deltaTime);
	}

	public void ShowNeedEnergyBubble()
	{
		if (!(Time.time - _last_need_energy_bubble_time < 0.5f))
		{
			_last_need_energy_bubble_time = Time.time;
			EffectBubblesManager.ShowImmediately(wgo.bubble_pos, GJL.L("not_enough_something", "(en)"), EffectBubblesManager.BubbleColor.Energy);
		}
	}

	public void SpendSanity(float need_sanity)
	{
		if (wgo.sanity >= need_sanity)
		{
			wgo.sanity -= need_sanity;
			_sanity_spent_sum += need_sanity;
			if (_sanity_spent_sum > 1f)
			{
				int num = Mathf.FloorToInt(_sanity_spent_sum);
				_sanity_spent_sum -= num;
				EffectBubblesManager.ShowStackedSanity(wgo, -num);
			}
		}
		else
		{
			wgo.sanity = 0f;
		}
	}

	public void ResetSpentCounters()
	{
		int num = Mathf.FloorToInt(_energy_spent_sum);
		if (num > 0)
		{
			_energy_spent_sum -= num;
			EffectBubblesManager.ShowStackedEnergy(wgo, -num);
		}
	}

	public void OnTriggerStay2D(Collider2D col)
	{
		if (_drop_collector != null)
		{
			_drop_collector.OnTriggerStay2D(col);
		}
	}

	public void OnTriggerEnter2D(Collider2D col)
	{
		if (_drop_collector != null)
		{
			_drop_collector.OnTriggerEnter2D(col);
		}
	}

	public static string GetTechPointsString(string separator = " ")
	{
		return string.Format("(r){0}{3}(g){1}{3}(b){2}", MainGame.me.player.GetParam("r"), MainGame.me.player.GetParam("g"), MainGame.me.player.GetParam("b"), separator);
	}

	public static float GetTextObfuscationChance()
	{
		return 0f;
	}

	public static float GetTechPointsLoseChance()
	{
		return 0f;
	}

	public void DoSpawnPrayBuff()
	{
		WorldMap.GetChurchPulpit().animator.SetTrigger("start_buff");
	}

	public void CreatePrayBuffFlyingObject(Vector2 pos)
	{
		FlyingObject.CreateBuffFlyingObject(_pray_buff, pos, _pray_craft.dur_parameter.EqualsTo(0f) ? null : new float?(_pray_craft.dur_parameter)).StartSmoothFly(GUIElements.me.buffs.grid.transform);
	}

	public void StartPrayAnimation(CraftDefinition pray_craft, bool success)
	{
		_pray_buff = GameBalance.me.GetDataOrNull<BuffDefinition>(pray_craft.buff);
		_pray_buff_success = success;
		_pray_craft = pray_craft;
		wgo.components.character.StartPrayAnimation(success);
		ChurchPulpit churchPulpit = WorldMap.GetChurchPulpit();
		string text = pray_craft.icon;
		if (_pray_buff != null && string.IsNullOrEmpty(text))
		{
			text = _pray_buff.GetIconName();
		}
		Debug.Log("StartPrayAnimation, icon = " + text + ", pray_craft.id = " + pray_craft.id);
		churchPulpit.buff_spr.sprite = EasySpritesCollection.GetSprite(text);
	}

	public void ThrowBodyInRiver(out bool thrown_worker)
	{
		_throwing_item = wgo.components.character.GetOverheadItem();
		thrown_worker = _throwing_item.is_worker;
		wgo.components.character.SetOverheadItem(null);
		wgo.components.character.control_enabled = false;
		wgo.components.character.SetAnimationState(CharAnimState.ThrowBody);
	}

	public void OnThrowBodyAnimationFinished()
	{
		wgo.components.character.SetAnimationState(CharAnimState.Idle);
		wgo.components.character.control_enabled = true;
	}

	public void ProcessDropOfThrowingBody()
	{
		Vector3 vector = MainGame.me.player_pos + new Vector3(-96f, 0f, 0f);
		ProjectileObject.Create("body_in_water", MainGame.me.world_root, vector, Vector2.down, MainGame.me.player).pop.attack_collider.gameObject.SetActive(value: false);
	}

	public void UnsubscribeMethods()
	{
		BaseGUI.on_window_opened -= OnAnyWindowOpenStopUsingTool;
	}

	private void OnAnyWindowOpenStopUsingTool(BaseGUI active_gui)
	{
		if (wgo.components.tool.playing_animation)
		{
			wgo.components.tool.StopUsingTool();
			wgo.components.character.OnStopped();
		}
	}
}
