using System;
using UnityEngine;

public class SleepGUI : BaseGUI
{
	private enum State
	{
		Sleeping,
		PlayingAppearing,
		PlayingDisappearing
	}

	private static string[] _params = new string[2] { "hp", "energy" };

	public const float ENERGY_K = 0.75f;

	public const float SANITY_K = 0.75f;

	public const float HP_K = 0.5f;

	public float anim_time = 1f;

	public Transform bubble_pos_tf;

	public UI2DSprite bed_sprite;

	private UIPanel _panel;

	private GJCommons.VoidDelegate _on_wake_up;

	private GJCommons.VoidDelegate _on_sleeped;

	private GJCommons.VoidDelegate _on_after_save;

	private State _state;

	private GameRes _added_res = new GameRes();

	private float _normal_fixed_timestep = 1f / 120f;

	public Vector3 bubble_pos => MainGame.me.world_cam.ScreenToWorldPoint(MainGame.me.gui_cam.WorldToScreenPoint(bubble_pos_tf.position));

	public override void Init()
	{
		_panel = GetComponent<UIPanel>();
		base.Init();
	}

	public void Open(GJCommons.VoidDelegate on_appeared = null, GJCommons.VoidDelegate on_wake_up = null, GJCommons.VoidDelegate on_doesnt_need_sleep = null, GJCommons.VoidDelegate on_after_save = null)
	{
		if (base.is_shown)
		{
			return;
		}
		bed_sprite.sprite2D = WorldMap.GetWorldGameObjectByCustomTag("hero_bed").GetComponentInChildren<SpriteRenderer>().sprite;
		if (MainGame.me.player.energy.EqualsTo(MainGame.me.save.max_energy) && MainGame.me.player.sanity.EqualsTo(MainGame.me.save.max_sanity))
		{
			on_doesnt_need_sleep.TryInvoke();
			return;
		}
		_panel.alpha = 0f;
		base.Open();
		GUIElements.me.hud.Open();
		_added_res.Clear();
		_added_res.durability = 0f;
		_on_wake_up = on_appeared;
		_on_sleeped = on_wake_up;
		_on_after_save = on_after_save;
		_state = State.PlayingAppearing;
		_panel.ChangeAlpha(_panel.alpha, 1f, anim_time, delegate
		{
			_state = State.Sleeping;
			_ = Time.timeScale;
			_ = 2f;
			_on_wake_up.TryInvoke();
			Time.timeScale = 10f;
			Time.fixedDeltaTime = 1f / 12f;
			base.button_tips.Print(new GameKeyTip(GameKey.Interaction, "wake up", active: true, gamepad_only: false));
			GC.Collect();
			Resources.UnloadUnusedAssets();
		});
		base.button_tips.Clear();
		MainGame.me.player.ClearTiredness();
		BuffsLogics.RemoveBuff("buff_tired");
	}

	public override void Update()
	{
		base.Update();
		if (_state != 0)
		{
			return;
		}
		WorldGameObject player = MainGame.me.player;
		GameSave save = MainGame.me.save;
		float num = 1f;
		num += MainGame.me.player.GetParam("sleep_k_add");
		if (player.energy < (float)save.max_energy)
		{
			float energy = player.energy;
			player.energy += Time.deltaTime * 0.75f * num;
			if (player.energy > (float)save.max_energy)
			{
				player.energy = save.max_energy;
			}
			_added_res.Add("energy", player.energy - energy);
		}
		if (player.hp < (float)save.max_hp)
		{
			float energy = player.hp;
			player.hp += Time.deltaTime * 0.5f * num;
			if (player.hp > (float)save.max_hp)
			{
				player.hp = save.max_hp;
			}
			_added_res.Add("hp", player.hp - energy);
		}
		if ((player.energy.EqualsOrMore(save.max_energy) && player.hp.EqualsOrMore(save.max_hp)) || LazyInput.GetKeyDown(GameKey.Interaction))
		{
			WakeUp();
			return;
		}
		string[] @params = _params;
		foreach (string text in @params)
		{
			if (_added_res.GetInt(text) >= 1)
			{
				EffectBubblesManager.ShowStacked(player, new GameRes(text, _added_res.Get(text)));
				_added_res.Set(text, 0f);
			}
		}
	}

	protected override bool OnPressedBack()
	{
		WakeUp();
		return true;
	}

	private void WakeUp()
	{
		if (_state != 0)
		{
			return;
		}
		_state = State.PlayingDisappearing;
		Time.timeScale = 1f;
		Time.fixedDeltaTime = 1f / 60f;
		base.button_tips.Clear();
		EffectBubblesManager.RemoveAllBubbles();
		PlatformSpecific.SaveGame(MainGame.me.save_slot, MainGame.me.save, delegate
		{
			Debug.Log("Autosave: done");
			_on_after_save.TryInvoke();
			_panel.ChangeAlpha(_panel.alpha, 0f, anim_time, delegate
			{
				Hide(play_hide_sound: false);
				MainGame.me.save.quests.CheckKeyQuests("wake_up");
				_on_sleeped.TryInvoke();
			});
		});
	}
}
