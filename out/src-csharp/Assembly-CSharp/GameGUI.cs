using System;
using System.Collections.Generic;
using LinqTools;

public class GameGUI : BaseGUI
{
	[Serializable]
	public enum TabType
	{
		Inventory,
		Techs,
		NPCs,
		Bodies,
		Map
	}

	public UILabel LB;

	public UILabel RB;

	private Dictionary<TabType, GameTabItemGUI> _tabs;

	private UITable _tabs_grid;

	private TabType _current_tab_type;

	private int _current_tab_index;

	private Dictionary<TabType, BaseGameGUI> TABS;

	public override void Init()
	{
		GameTabItemGUI componentInChildren = GetComponentInChildren<GameTabItemGUI>();
		TABS = new Dictionary<TabType, BaseGameGUI>
		{
			{
				TabType.Inventory,
				GUIElements.me.inventory
			},
			{
				TabType.Techs,
				GUIElements.me.tech_tree
			},
			{
				TabType.NPCs,
				GUIElements.me.npcs_list
			},
			{
				TabType.Map,
				GUIElements.me.map
			}
		};
		_tabs = new Dictionary<TabType, GameTabItemGUI>();
		foreach (TabType key in TABS.Keys)
		{
			GameTabItemGUI gameTabItemGUI = componentInChildren.Copy();
			gameTabItemGUI.Init(key, TABS[key]);
			_tabs.Add(key, gameTabItemGUI);
		}
		componentInChildren.Deactivate();
		_tabs_grid = GetComponentInChildren<UITable>(includeInactive: true);
		_tabs_grid.Reposition();
		base.gameObject.Deactivate();
	}

	public override void Open()
	{
		GUIElements.me.hud.OnAnyWindowOpened(this);
		Sounds.OnWindowOpened();
		base.Open();
		_tabs_grid.Reposition();
		SelectTab(_current_tab_type);
		MainGame.SetPausedMode(is_paused: true);
		if (LB != null)
		{
			LB.text = GameKeyTip.GetIcon(GameKey.RotateLeft);
		}
		if (RB != null)
		{
			RB.text = GameKeyTip.GetIcon(GameKey.RotateRight);
		}
	}

	public void OpenAtTab(TabType tab)
	{
		_current_tab_type = tab;
		Open();
	}

	public void OpenOrSelectTab(TabType tab)
	{
		if (base.is_shown)
		{
			SelectTab(tab);
			return;
		}
		_current_tab_type = tab;
		Open();
	}

	public override void Hide(bool play_hide_sound = true)
	{
		GUIElements.me.hud.OnAnyWindowClosed(this);
		Sounds.OnClosePressed();
		base.Hide(play_hide_sound);
		foreach (GameTabItemGUI value in _tabs.Values)
		{
			value.Unselect();
		}
		MainGame.me.player.components.interaction.UpdateNearestHint();
		MainGame.SetPausedMode(is_paused: false);
	}

	public void NextTab(int step)
	{
		if (GUIElements.me.equip_to_toolbar.is_shown)
		{
			GUIElements.me.equip_to_toolbar.Hide(play_hide_sound: false);
		}
		_current_tab_index += step;
		if (_current_tab_index < 0)
		{
			_current_tab_index = _tabs.Values.Count - 1;
		}
		else if (_current_tab_index >= _tabs.Values.Count)
		{
			_current_tab_index = 0;
		}
		_tabs.Values.ElementAt(_current_tab_index).OnPressed();
	}

	public void SelectTab(TabType tab_type)
	{
		GameTabItemGUI gameTabItemGUI = null;
		int num = -1;
		foreach (TabType key in _tabs.Keys)
		{
			num++;
			if (tab_type == key)
			{
				gameTabItemGUI = _tabs[key];
				_current_tab_index = num;
			}
			else
			{
				_tabs[key].Unselect();
			}
		}
		if (gameTabItemGUI != null)
		{
			gameTabItemGUI.Select();
		}
		_current_tab_type = tab_type;
	}

	public override void Update()
	{
		base.Update();
		if (LazyInput.GetKeyDown(GameKey.GameGUI))
		{
			Hide();
			LazyInput.ClearKeyDown(GameKey.GameGUI);
		}
		if (!GUIElements.me.item_count.is_shown)
		{
			if (LazyInput.GetKeyDown(GameKey.PrevTab))
			{
				NextTab(-1);
			}
			if (LazyInput.GetKeyDown(GameKey.NextTab))
			{
				NextTab(1);
			}
		}
	}

	protected override bool OnPressedBack()
	{
		Hide();
		return true;
	}

	protected override bool OnPressedPrevTab()
	{
		if (GUIElements.me.item_count.is_shown)
		{
			return false;
		}
		NextTab(-1);
		return true;
	}

	protected override bool OnPressedNextTab()
	{
		if (GUIElements.me.item_count.is_shown)
		{
			return false;
		}
		NextTab(1);
		return true;
	}
}
