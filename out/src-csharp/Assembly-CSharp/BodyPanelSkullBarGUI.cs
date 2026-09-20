using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BodyPanelSkullBarGUI : MonoBehaviour
{
	[Serializable]
	public enum Align
	{
		Left,
		Center
	}

	public int negative;

	public int positive;

	public int filled;

	[Range(0f, 1f)]
	public float durability;

	public UIWidget green_back;

	public UIWidget back;

	public UIProgressBar green_bar;

	public GameObject skull_red;

	public GameObject skull_white;

	public UIGrid grid;

	public UILabel txt_percent;

	public bool scroll_setup;

	public UIScrollView _scroll_view;

	public UIPanel green_panel;

	public UI2DSprite back_spr;

	public UI2DSprite frame;

	public Sprite active_back;

	public Sprite inactive_back;

	public UIWidget grid_widget;

	public GamepadNavigationItem gamepad_item;

	public int scroll_skulls_limit = 10;

	private const float INACTIVE_ALPHA = 1f;

	private List<GameObject> _skulls;

	public Align bar_align;

	public event Action on_enable_skulls_frame;

	public event Action on_disable_skulls_frame;

	private void Awake()
	{
		InitGamepadItem();
	}

	public void InitGamepadItem()
	{
		gamepad_item.SetCallbacks(EnableSelectionFrame, DisableSelectionFrame, null);
	}

	public void Redraw()
	{
		DisableSelectionFrame();
		back_spr.SetActive(active: true);
		skull_red.SetActive(value: false);
		skull_white.SetActive(value: false);
		int num = 0;
		while (grid.transform.childCount > 2)
		{
			for (int i = 0; i < grid.transform.childCount; i++)
			{
				GameObject gameObject = grid.transform.GetChild(i).gameObject;
				if (!(gameObject == skull_red) && !(gameObject == skull_white))
				{
					gameObject.transform.parent = null;
					NGUITools.Destroy(gameObject);
				}
			}
			if (++num > 100)
			{
				break;
			}
		}
		_skulls = new List<GameObject>();
		for (int j = 0; j < negative; j++)
		{
			_skulls.Add(skull_red.Copy());
		}
		for (int k = 0; k < positive; k++)
		{
			_skulls.Add(skull_white.Copy());
		}
		int num2 = Mathf.CeilToInt(durability * 100f);
		int num3 = num2;
		if (num2 > 90)
		{
			num2 = 100;
		}
		int num4 = Mathf.FloorToInt((float)(positive * num2) / 100f);
		int num5 = positive - num4;
		green_back.gameObject.SetActive(num5 > 0 && base.gameObject.activeSelf);
		green_bar.gameObject.SetActive(num5 > 0 && base.gameObject.activeSelf);
		green_back.width = 2 + 12 * num5;
		green_bar.value = (1f + (float)positive * 12f * (1f - (float)num2 / 100f)) / 182f;
		green_bar.ForceUpdate();
		back.width = 3 + (positive + negative) * 12;
		txt_percent.text = num3 + "%";
		switch (bar_align)
		{
		case Align.Left:
			base.transform.localPosition = new Vector3((negative - 1 + positive - 3) * 12, 0f, 0f);
			break;
		case Align.Center:
			base.transform.localPosition = new Vector3((negative - 1 + positive - 3 - (negative + positive) / 2) * 12, 0f, 0f);
			break;
		}
		RedrawFilledSkulls();
		if (num3 < num2)
		{
			green_bar.gameObject.SetActive(base.gameObject.activeSelf);
		}
		txt_percent.gameObject.SetActive(green_bar.gameObject.activeSelf);
		bool flag = scroll_setup && _skulls.Count > scroll_skulls_limit && base.gameObject.activeSelf;
		if (scroll_setup)
		{
			_scroll_view.transform.localPosition = Vector3.zero;
			_scroll_view.panel.UpdateAnchors();
			_scroll_view.enabled = flag;
			gamepad_item.active = flag && gamepad_item.gameObject.activeInHierarchy;
			back_spr.sprite2D = (flag ? active_back : inactive_back);
			back_spr.type = UIBasicSprite.Type.Sliced;
			back_spr.border = new Vector4(24f, 12f, 24f, 12f);
			grid_widget.width = 12 * _skulls.Count;
			if (_skulls.Count <= scroll_skulls_limit)
			{
				back_spr.width = grid_widget.width + 20;
			}
			else
			{
				back_spr.width = 12 * scroll_skulls_limit + 20;
			}
			int num6 = _skulls.Count - scroll_skulls_limit;
			frame.width = back_spr.width;
			Vector4 baseClipRegion = _scroll_view.panel.baseClipRegion;
			baseClipRegion.z = back_spr.width - 18;
			_scroll_view.panel.baseClipRegion = baseClipRegion;
			green_panel.baseClipRegion = baseClipRegion;
			Vector3 zero = Vector3.zero;
			_scroll_view.StopScrolling();
			_scroll_view.transform.DOKill();
			_scroll_view.RestrictWithinBounds(instant: false);
			if (num6 > 0)
			{
				zero.x = num6 * -6;
				_scroll_view.transform.localPosition = zero;
			}
			else
			{
				_scroll_view.transform.localPosition = zero;
				_scroll_view.ResetPosition();
			}
			_scroll_view.panel.UpdateAnchors();
		}
		if (!base.gameObject.activeSelf)
		{
			green_bar.SetActive(active: false);
		}
		grid.Reposition();
		grid.repositionNow = true;
		if (scroll_setup)
		{
			green_panel.UpdateAnchors();
		}
		Update();
	}

	private void OnDisable()
	{
		DisableSelectionFrame();
	}

	public void NoBodyRedraw()
	{
		back_spr.SetActive(active: false);
		green_bar.SetActive(active: false);
		gamepad_item.active = false;
	}

	public void RedrawFilledSkulls()
	{
		if (_skulls == null)
		{
			Debug.LogError("skulls is null in body");
			return;
		}
		for (int i = 0; i < _skulls.Count; i++)
		{
			_skulls[i].GetComponent<UI2DSprite>().color = new Color(1f, 1f, 1f, (i < filled) ? 1f : 1f);
		}
	}

	public void EnableSelectionFrame()
	{
		if (!(frame == null) && (_scroll_view.enabled || !base.gameObject.activeInHierarchy))
		{
			frame.gameObject.SetActive(value: true);
			this.on_enable_skulls_frame?.Invoke();
			Sounds.OnGUIHover();
		}
	}

	public void DisableSelectionFrame()
	{
		if (!(frame == null) && _scroll_view.enabled)
		{
			this.on_disable_skulls_frame?.Invoke();
			frame.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (!scroll_setup)
		{
			return;
		}
		if (green_bar.gameObject.activeSelf)
		{
			Transform obj = green_bar.transform;
			Vector3 localPosition = obj.localPosition;
			localPosition.x = (float)(-39 + (_skulls.Count - 1) * 6) + grid.transform.localPosition.x + _scroll_view.transform.localPosition.x;
			obj.localPosition = localPosition;
		}
		if (LazyInput.gamepad_active && frame.gameObject.activeSelf)
		{
			float num = LazyInput.GetDirection2().x * -0.1f;
			if (!num.EqualsTo(0f))
			{
				_scroll_view.Scroll(num);
			}
		}
	}
}
