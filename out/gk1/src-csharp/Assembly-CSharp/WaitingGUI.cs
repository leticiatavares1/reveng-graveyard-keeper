using System;
using UnityEngine;

public class WaitingGUI : BaseGUI
{
	private enum State
	{
		Waiting,
		PlayingAppearing,
		PlayingDisappearing
	}

	private static string[] _params = new string[2] { "hp", "energy" };

	public const float ENERGY_K = 0.25f;

	public const float HP_K = 0.25f;

	public float anim_time = 1f;

	private UIPanel _panel;

	private State _state;

	private GameRes _added_res = new GameRes();

	private GJCommons.VoidDelegate _on_started_waiting;

	private GJCommons.VoidDelegate _on_ended_waiting;

	public override void Init()
	{
		_panel = GetComponent<UIPanel>();
		base.Init();
	}

	public void Open(GJCommons.VoidDelegate on_started_waiting = null, GJCommons.VoidDelegate on_ended_waiting = null, GJCommons.VoidDelegate on_after_save = null)
	{
		if (!base.is_shown)
		{
			_panel.alpha = 0f;
			base.Open();
			GUIElements.me.hud.Open();
			_added_res.Clear();
			_added_res.durability = 0f;
			_on_started_waiting = on_started_waiting;
			_on_ended_waiting = on_ended_waiting;
			_state = State.PlayingAppearing;
			_panel.ChangeAlpha(_panel.alpha, 1f, anim_time, delegate
			{
				_state = State.Waiting;
				_on_started_waiting.TryInvoke();
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
			player.energy += Time.deltaTime * 0.25f * num;
			if (player.energy > (float)save.max_energy)
			{
				player.energy = save.max_energy;
			}
			_added_res.Add("energy", player.energy - energy);
		}
		if (player.hp < (float)save.max_hp)
		{
			float energy = player.hp;
			player.hp += Time.deltaTime * 0.25f * num;
			if (player.hp > (float)save.max_hp)
			{
				player.hp = save.max_hp;
			}
			_added_res.Add("hp", player.hp - energy);
		}
		if (!player.energy.EqualsOrMore(save.max_energy))
		{
			string text = _params[1];
			if (_added_res.GetInt(text) >= 1)
			{
				EffectBubblesManager.ShowStacked(player, new GameRes(text, _added_res.Get(text)));
				_added_res.Set(text, 0f);
			}
		}
		if (!player.hp.EqualsOrMore(save.max_hp))
		{
			string text2 = _params[0];
			if (_added_res.GetInt(text2) >= 1)
			{
				EffectBubblesManager.ShowStacked(player, new GameRes(text2, _added_res.Get(text2)));
				_added_res.Set(text2, 0f);
			}
		}
		if (LazyInput.GetKeyDown(GameKey.Interaction))
		{
			StopWaiting();
		}
	}

	protected override bool OnPressedBack()
	{
		StopWaiting();
		return true;
	}

	private void StopWaiting()
	{
		if (_state == State.Waiting)
		{
			_state = State.PlayingDisappearing;
			Time.timeScale = 1f;
			Time.fixedDeltaTime = 1f / 60f;
			base.button_tips.Clear();
			EffectBubblesManager.RemoveAllBubbles();
			_panel.ChangeAlpha(_panel.alpha, 0f, anim_time, delegate
			{
				Hide(play_hide_sound: false);
				MainGame.me.save.quests.CheckKeyQuests("stop_waiting");
				_on_ended_waiting.TryInvoke();
			});
		}
	}
}
