using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffsGUI : BaseGUI
{
	public GameObject buffs_hud;

	public UIPanel buffs_hud_panel;

	public BuffIcon buff_icon_prefab;

	public UIGrid grid;

	[NonSerialized]
	private List<BuffIcon> _buffs = new List<BuffIcon>();

	public override void Init()
	{
		base.Init();
		buff_icon_prefab.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: true);
	}

	public void Redraw()
	{
		Clear();
		if (!MainGame.game_started)
		{
			return;
		}
		foreach (PlayerBuff buff in MainGame.me.save.buffs)
		{
			if (!buff.definition.is_hidden)
			{
				BuffIcon buffIcon = buff_icon_prefab.Copy();
				_buffs.Add(buffIcon);
				buffIcon.Draw(buff);
			}
		}
		grid.repositionNow = true;
		grid.Reposition();
		if (GUIElements.me.inventory.is_shown)
		{
			GUIElements.me.inventory.RedrawBuffsAndPerks();
		}
	}

	private void Clear()
	{
		foreach (BuffIcon buff in _buffs)
		{
			NGUITools.Destroy(buff.gameObject);
		}
		_buffs.Clear();
	}

	public override void Update()
	{
		base.Update();
		foreach (BuffIcon buff in _buffs)
		{
			buff.Redraw();
		}
	}
}
