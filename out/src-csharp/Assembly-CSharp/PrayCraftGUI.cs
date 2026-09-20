using System;
using UnityEngine;

[RequireComponent(typeof(ResourceBasedCraftGUI))]
public class PrayCraftGUI : MonoBehaviour
{
	private ResourceBasedCraftGUI _res_craft;

	public UIProgressBar progress_bar;

	public UIWidget angels_widget;

	public UIWidget icon_widget;

	public UILabel l_value_in;

	public UILabel l_value_out;

	public UILabel l_value_max;

	public UILabel l_pick_res;

	public UILabel l_button;

	public UILabel l_total_values;

	public GameObject full_indicator;

	public int angels_y_min;

	public int angels_y_max;

	public int angels_y_sky;

	private bool _animating;

	private float _result_f;

	private float _t;

	private float _cur_quality;

	private bool _block_close;

	private Item _selected_item;

	public CraftDefinition pray_craft;

	private Action _on_finished_animation;

	private Action _on_middle_animation;

	public void Init()
	{
		_res_craft = GetComponent<ResourceBasedCraftGUI>();
		_res_craft.Init();
		base.gameObject.SetActive(value: false);
	}

	public void Open(WorldGameObject craftery_wgo)
	{
		_res_craft.Open(craftery_wgo, CraftDefinition.CraftType.PrayCraft);
		angels_widget.alpha = 0f;
		UILabel uILabel = l_value_max;
		UILabel uILabel2 = l_value_in;
		string text2 = (l_value_out.text = "");
		string text4 = (uILabel2.text = text2);
		uILabel.text = text4;
		pray_craft = null;
		_cur_quality = craftery_wgo.GetMyWorldZone().GetTotalQuality();
		l_pick_res.text = GJL.L("preach_pick_res_hint", $"{_cur_quality:0.0}");
		full_indicator.SetActive(value: false);
		progress_bar.value = 0f;
		progress_bar.thumb.gameObject.SetActive(value: false);
		icon_widget.alpha = 1f;
		RedrawTextValues(0f, 0f);
	}

	public void OnPrayButtonPressed()
	{
		if (!_block_close)
		{
			Debug.Log("OnPrayButtonPressed");
			MainGame.me.player.SetParam("prayed_this_week", 1f);
			_animating = false;
			_res_craft.Hide();
			GS.RunFlowScript("pray");
		}
	}

	public void OnResourcePickerClosed(Item item)
	{
		if (item != null)
		{
			_selected_item = item;
			float num = _cur_quality / item.definition.linked_craft.needs_quality;
			bool flag = false;
			if (num >= 1f)
			{
				num = 1f;
				flag = true;
			}
			pray_craft = ((_selected_item == null || _selected_item.IsEmpty()) ? null : _selected_item.definition.linked_craft);
			RedrawTextValues(item.definition.linked_craft.needs_quality, num);
			l_button.text = GJL.L(flag ? "btn_pray" : "btn_try_pray");
		}
	}

	private void RedrawTextValues(float needs_q, float chance)
	{
		l_total_values.text = GJL.L("pray_gui_church_q", $"(cross){_cur_quality:0.#}");
		if (pray_craft != null)
		{
			UILabel uILabel = l_total_values;
			uILabel.text = uILabel.text + "\n" + GJL.L("pray_gui_sermon_needs", $"(cross){needs_q:0}") + "\n" + GJL.L("sermon_success_chance", Mathf.RoundToInt(chance * 100f) + "%");
		}
	}

	public bool CanClose()
	{
		return !_block_close;
	}

	public void DoPrayForBuff(bool success, Action on_finished_animation, Action on_middle)
	{
		Debug.Log("DoPrayForBuff, success = " + success);
		_on_finished_animation = on_finished_animation;
		_on_middle_animation = on_middle;
		if (pray_craft == null)
		{
			Debug.LogError("pray_craft is null");
			OnMiddlePrayBuffAnimation();
			OnFinishedPrayBuffAnimation();
		}
		else
		{
			MainGame.me.player_component.StartPrayAnimation(pray_craft, success);
		}
	}

	public void OnFinishedPrayBuffAnimation()
	{
		Debug.Log("OnFinishedPrayBuffAnimation");
		Stats.DesignEvent("Pray:" + pray_craft.id);
		if (_on_finished_animation != null)
		{
			_on_finished_animation();
		}
	}

	public void OnMiddlePrayBuffAnimation()
	{
		Debug.Log("OnMiddlePrayBuffAnimation");
		if (_on_middle_animation != null)
		{
			_on_middle_animation();
		}
	}

	public float GetCurrentZoneQuality()
	{
		return _cur_quality;
	}
}
