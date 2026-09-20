using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HUD : MonoBehaviour
{
	public UIProgressBar bar_hp;

	public UIProgressBar bar_energy;

	public UIProgressBar time_meter;

	public UIProgressBar bar_sanity;

	public UILabel day_label;

	public UILabel hint_x;

	public UILabel hint_a;

	public UIPanel panel;

	public UILabel r_label;

	public UILabel g_label;

	public UILabel b_label;

	public ToolbarGUI toolbar;

	private bool _inited;

	public HUDSinIcon[] sin_icons;

	public AnimationCurve time_k_to_lighttime;

	public Transform time_circle_rotating;

	public UIWidget[] appear_at_night;

	public UIWidget[] appear_at_day;

	public UIWidget sin_glow;

	public UIWidget sins_circle;

	[Range(0f, 1f)]
	public float test_time;

	private List<Sprite> _sins_spr_back;

	private List<Sprite> _sins_spr_active;

	private List<Color> _sins_glow_colors;

	private const float SIN_CIRCLE_ANIM_TIME = 0.13f;

	public UILabel zone_name;

	public UILabel zone_descr;

	public GameObject zone_descr_object;

	public HUDTechPointsBar tech_points_bar;

	public HUDTechTrashCan tech_trash_can;

	public HUDRelationBubble relation_bubble;

	public UILabel version_label;

	public void Init()
	{
		BaseGUI.on_window_opened += OnAnyWindowOpened;
		BaseGUI.on_window_closed += OnAnyWindowClosed;
		panel = GetComponent<UIPanel>();
		_inited = true;
		Update();
		version_label.text = "";
		toolbar = GetComponentInChildren<ToolbarGUI>(includeInactive: true);
		LazyInput.on_input_changed += Redraw;
		_sins_glow_colors = new List<Color>();
		_sins_spr_back = new List<Sprite>();
		_sins_spr_active = new List<Sprite>();
		HUDSinIcon[] array = sin_icons;
		foreach (HUDSinIcon hUDSinIcon in array)
		{
			_sins_glow_colors.Add(hUDSinIcon.glow_color);
			_sins_spr_back.Add(hUDSinIcon.spr_back.sprite2D);
			_sins_spr_active.Add(hUDSinIcon.spr_active.sprite2D);
		}
		tech_points_bar = GetComponentInChildren<HUDTechPointsBar>(includeInactive: true);
		if (tech_points_bar != null)
		{
			tech_points_bar.Init();
		}
		tech_trash_can = GetComponentInChildren<HUDTechTrashCan>(includeInactive: true);
		if (tech_trash_can != null)
		{
			tech_trash_can.Init();
		}
		if (relation_bubble != null)
		{
			relation_bubble.Init();
		}
	}

	public void Update()
	{
		if (!_inited || !MainGame.game_started)
		{
			return;
		}
		bar_hp.value = MainGame.me.save.GetHPPercentage();
		bar_energy.value = MainGame.me.player.energy / (float)MainGame.me.save.max_energy;
		bar_sanity.value = MainGame.me.player.sanity / (float)MainGame.me.save.max_sanity;
		day_label.text = "day " + MainGame.me.save.day;
		r_label.text = Mathf.RoundToInt(MainGame.me.player.GetParam("r")).ToString();
		g_label.text = Mathf.RoundToInt(MainGame.me.player.GetParam("g")).ToString();
		b_label.text = Mathf.RoundToInt(MainGame.me.player.GetParam("b")).ToString();
		if (hint_x != null)
		{
			if (MainGame.me.player_char.has_overhead)
			{
				hint_x.text = "(X) - drop";
			}
			else
			{
				hint_x.text = "(X) - attack/use tool";
			}
		}
		RedrawTime(TimeOfDay.me.GetTimeK());
	}

	public void UpdateZoneInfo(string name, string description)
	{
		zone_name.text = name;
		zone_descr.text = description;
		zone_descr_object.SetActive(!string.IsNullOrEmpty(description));
	}

	public void Redraw()
	{
		if (base.gameObject.activeSelf)
		{
			toolbar.Redraw();
			RedrawSinsIcons();
		}
	}

	public void Open()
	{
		Debug.Log("HUD show");
		base.gameObject.SetActive(GUIElements.me.hud_enabled);
		GUIElements.me.buffs_panel.gameObject.SetActive(value: true);
		Redraw();
	}

	public void Hide()
	{
		Debug.Log("HUD hide");
		base.gameObject.SetActive(value: false);
		GUIElements.me.buffs_panel.gameObject.SetActive(value: false);
	}

	public void OnAnyWindowOpened(BaseGUI gui)
	{
		if (!BaseGUI.all_guis_closed)
		{
			base.gameObject.TryFinishAlphaTween();
			Hide();
		}
	}

	public void OnAnyWindowClosed(BaseGUI gui)
	{
		if (BaseGUI.all_guis_closed && MainGame.game_started)
		{
			Open();
		}
	}

	private void RedrawTime(float time)
	{
		time_meter.value = time;
		float num = time_k_to_lighttime.Evaluate(time) - 0.5f;
		time_circle_rotating.rotation = Quaternion.Euler(0f, 0f, 360f * num);
		float num2 = Mathf.Abs(num) * 2f;
		UIWidget[] array = appear_at_day;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].alpha = 1f - num2;
		}
		array = appear_at_night;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].alpha = num2;
		}
	}

	public void OnValidate()
	{
		if (!Application.isPlaying)
		{
			RedrawTime(test_time);
		}
	}

	public void OnEndOfDay()
	{
		sins_circle.transform.DORotate(new Vector3(0f, 0f, -60f), 0.13f).OnComplete(delegate
		{
			sins_circle.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			RedrawSinsIcons();
		});
		if (sin_glow.alpha > 0f)
		{
			DOTween.To(() => sin_glow.alpha, delegate(float x)
			{
				sin_glow.alpha = x;
			}, 0f, 0.13f);
		}
		Update();
	}

	private void RedrawSinsIcons()
	{
		int index = 0;
		Sins.SinType sin = Sins.SinType.Envy;
		for (int i = 0; i < sin_icons.Length; i++)
		{
			int num = (9 - MainGame.me.save.day_of_week + i) % 6;
			HUDSinIcon obj = sin_icons[i];
			Sins.SinType sinType = (Sins.SinType)(num + 1);
			if (i == 3)
			{
				index = num;
				sin = sinType;
			}
			obj.Draw(sinType, _sins_spr_back[num], _sins_spr_active[num], _sins_glow_colors[num]);
		}
		sin_glow.color = _sins_glow_colors[index];
		if (MainGame.me.save.GetSinState(sin))
		{
			DOTween.To(() => sin_glow.alpha, delegate(float x)
			{
				sin_glow.alpha = x;
			}, 1f, 0.13f);
		}
	}

	public void ToolbarSetEnabled(bool enabled = true)
	{
		toolbar.gameObject.SetActive(enabled);
		if (tech_trash_can != null)
		{
			tech_trash_can.gameObject.SetActive(enabled);
		}
	}
}
