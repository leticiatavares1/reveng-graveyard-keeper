using System.Collections.Generic;
using DG.Tweening;
using SmartPools;
using UnityEngine;

public class TechTreeGUI : BaseGameGUI
{
	private TechTreeGUIItem _item_prefab;

	private TechBranchGUIItem _branch_item_prefab;

	private TechTreeGUIItem _gamepad_focused_slot;

	private int _cur_branch = 1;

	private List<TechTreeGUIItem> _items = new List<TechTreeGUIItem>();

	private List<TechConnector> _connectors = new List<TechConnector>();

	private List<TechBranchGUIItem> _branches = new List<TechBranchGUIItem>();

	public GameObject content;

	public UITable branches_grid;

	public UILabel txt_tech_points;

	public TechConnector tech_connector_prefab;

	private UIScrollView _scroll_view;

	private const int TECH_SIZE_X = 165;

	private const int TECH_SIZE_Y = 74;

	private int _max_branch_in_balance = -1;

	public Color clr_line_available;

	public Color clr_line_purchased;

	public Color clr_line_not_available;

	public int current_branch => _cur_branch;

	public override void Init()
	{
		base.Init();
		if (_max_branch_in_balance == -1)
		{
			foreach (TechDefinition techs_datum in GameBalance.me.techs_data)
			{
				if (techs_datum.branch_type > _max_branch_in_balance)
				{
					_max_branch_in_balance = techs_datum.branch_type;
				}
			}
		}
		_scroll_view = GetComponentInChildren<UIScrollView>(includeInactive: true);
		_item_prefab = GetComponentInChildren<TechTreeGUIItem>(includeInactive: true);
		_item_prefab.InitPrefab();
		_branch_item_prefab = GetComponentInChildren<TechBranchGUIItem>(includeInactive: true);
		_branch_item_prefab.gameObject.SetActive(value: false);
		tech_connector_prefab = GetComponentInChildren<TechConnector>(includeInactive: true);
		SmartPooler.CreatePool(tech_connector_prefab, 100);
		tech_connector_prefab.gameObject.SetActive(value: false);
	}

	public void OpenTech(string tech_id)
	{
		TechDefinition data = GameBalance.me.GetData<TechDefinition>(tech_id);
		if (data != null)
		{
			_cur_branch = data.branch_type;
		}
		GUIElements.me.game_gui.Open();
		GUIElements.me.game_gui.SelectTab(GameGUI.TabType.Techs);
	}

	public override void Open()
	{
		_gamepad_focused_slot = null;
		base.Open();
		Draw();
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
			int num = _cur_branch - 1;
			if (num < 0)
			{
				num = 0;
			}
			else if (num >= _branches.Count)
			{
				num = _branches.Count - 1;
			}
			base.gamepad_controller.SetFocusedItem(_branches[num].GetComponent<GamepadNavigationItem>());
		}
		branches_grid.Reposition();
		branches_grid.repositionNow = true;
	}

	public override void Update()
	{
		base.Update();
		_scroll_view.UpdatePosition();
	}

	private new void LateUpdate()
	{
		if (_scroll_view.RestrictWithinBounds(instant: false))
		{
			_scroll_view.transform.DOKill();
		}
	}

	private void ScrollViewReposition()
	{
		_scroll_view.RestrictWithinBounds(instant: true);
		_scroll_view.ResetPosition();
	}

	private void DrawBranch(int tech_branch_id)
	{
		for (int i = 0; i < _items.Count; i++)
		{
			NGUITools.Destroy(_items[i].gameObject);
		}
		for (int j = 0; j < _connectors.Count; j++)
		{
			NGUITools.Destroy(_connectors[j].gameObject);
		}
		_items.Clear();
		_connectors.Clear();
		content.transform.localPosition = Vector3.zero;
		_scroll_view.ResetPosition();
		float num = float.MinValue;
		float num2 = float.MaxValue;
		foreach (TechDefinition techs_datum in GameBalance.me.techs_data)
		{
			if (techs_datum.branch_type == tech_branch_id && techs_datum.GetState() != TechDefinition.TechState.Invisible)
			{
				TechTreeGUIItem item = CreateItem(techs_datum);
				_items.Add(item);
				float y = techs_datum.y;
				if (y > num)
				{
					num = y;
				}
				if (y < num2)
				{
					num2 = y;
				}
			}
		}
		foreach (TechTreeGUIItem item2 in _items)
		{
			TechDefinition.TechState techState = item2.GetTechState();
			foreach (TechDefinition child in item2.tech.children)
			{
				if (child.GetState() != TechDefinition.TechState.Invisible)
				{
					_connectors.Add(TechConnector.Create(item2, GetTechByID(child.id)));
					_connectors[_connectors.Count - 1].SetState(techState);
				}
			}
		}
		txt_tech_points.text = PlayerComponent.GetTechPointsString("\n");
		ScrollViewReposition();
		float num3 = num - num2;
		int num4 = Mathf.RoundToInt(37f);
		Debug.Log("max_y = " + num + ", min_y = " + num2 + ", hgt = " + num3);
		Vector3 localPosition = content.transform.localPosition;
		if (num3 > 3.5f)
		{
			if (num2 > 0.5f)
			{
				localPosition.y = num4 + 5;
			}
		}
		else if (num3 < 4.5f)
		{
			localPosition.y = -num4;
		}
		localPosition.y = 2f;
		UpdateAllAnchors();
		content.transform.localPosition = new Vector2(localPosition.x, localPosition.y);
	}

	private TechTreeGUIItem GetTechByID(string id)
	{
		foreach (TechTreeGUIItem item in _items)
		{
			if (item.tech_id == id)
			{
				return item;
			}
		}
		Debug.LogError("Can't find tech with id = " + id);
		return null;
	}

	private TechTreeGUIItem CreateItem(TechDefinition tech)
	{
		TechTreeGUIItem techTreeGUIItem = _item_prefab.Copy();
		GJL.EnsureChildLabelsHasCorrectFont(techTreeGUIItem.gameObject, do_cache: false);
		techTreeGUIItem.Draw(tech);
		techTreeGUIItem.transform.localPosition = new Vector3(tech.x * 165, (0f - tech.y) * 74f);
		return techTreeGUIItem;
	}

	public void OnClickTech(TechDefinition tech)
	{
		Debug.Log("On Click tech " + tech.id, this);
		if (UtilityStuff.ProcessClickTech(tech, delegate
		{
			RedrawTechs();
			RestoreGamepadOnTech(tech.id);
		}))
		{
			return;
		}
		switch (tech.GetState())
		{
		case TechDefinition.TechState.Purchased:
			return;
		case TechDefinition.TechState.Hidden:
			Sounds.OnGUIClick();
			if (BaseGUI.for_gamepad)
			{
				base.button_tips.Deactivate();
			}
			GUIElements.me.dialog.OpenOK(GJL.L("tech_is_hidden"), delegate
			{
				if (BaseGUI.for_gamepad)
				{
					base.button_tips.Activate();
					RestoreGamepadOnTech(tech.id);
				}
			});
			return;
		}
		Sounds.OnGUIClick();
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Deactivate();
		}
		if (tech.price.IsEmpty())
		{
			GUIElements.me.dialog.OpenOK(GJL.L("tech_cant_be_unlocked"), delegate
			{
				if (BaseGUI.for_gamepad)
				{
					base.button_tips.Activate();
					RestoreGamepadOnTech(tech.id);
				}
			});
			return;
		}
		GUIElements.me.tech_dialog.Open(tech, delegate
		{
			RedrawTechs();
			if (BaseGUI.for_gamepad)
			{
				base.button_tips.Activate();
				RestoreGamepadOnTech(tech.id);
			}
		});
	}

	public void SelectTechBranch(int branch_id)
	{
		Debug.Log("Select tech branch = " + branch_id);
		_cur_branch = branch_id;
		foreach (TechBranchGUIItem branch in _branches)
		{
			branch.Redraw();
		}
		DrawBranch(branch_id);
		branches_grid.Reposition();
		UIWidget[] componentsInChildren = GetComponentsInChildren<UIWidget>(includeInactive: true);
		foreach (UIWidget uIWidget in componentsInChildren)
		{
			if (!(uIWidget.gameObject.GetComponentInParent<TechConnector>() != null) && !(uIWidget.gameObject.GetComponent<UIPanel>() != null))
			{
				uIWidget.UpdateAnchors();
			}
		}
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
		}
		branches_grid.Reposition();
		branches_grid.repositionNow = true;
	}

	public void Draw()
	{
		for (int i = 0; i < _branches.Count; i++)
		{
			_branches[i].gameObject.transform.SetParent(null, worldPositionStays: false);
			_branches[i].gameObject.Destroy();
		}
		_branches.Clear();
		for (int j = 0; j <= _max_branch_in_balance; j++)
		{
			if (MainGame.me.save.IsTechBranchVisible(j))
			{
				TechBranchGUIItem techBranchGUIItem = _branch_item_prefab.Copy();
				techBranchGUIItem.gameObject.SetActive(value: true);
				techBranchGUIItem.Draw(j);
				_branches.Add(techBranchGUIItem);
			}
		}
		for (int k = 0; k < _branches.Count; k++)
		{
			_ = _branches[k];
			if (k != 0)
			{
				_branches[k - 1].GetComponent<GamepadNavigationItem>();
			}
			if (k != _branches.Count - 1)
			{
				_branches[k + 1].GetComponent<GamepadNavigationItem>();
			}
		}
		branches_grid.Reposition();
		branches_grid.repositionNow = true;
		RedrawTechs();
	}

	private void RedrawTechs()
	{
		DrawBranch(_cur_branch);
	}

	private void RestoreGamepadOnTech(string tech_id)
	{
		if (!BaseGUI.for_gamepad)
		{
			return;
		}
		TechTreeGUIItem techTreeGUIItem = null;
		foreach (TechTreeGUIItem item in _items)
		{
			if (item.tech_id == tech_id)
			{
				techTreeGUIItem = item;
				break;
			}
		}
		base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
		base.gamepad_controller.SetFocusedItem((techTreeGUIItem == null) ? null : techTreeGUIItem.GetComponent<GamepadNavigationItem>());
	}

	public override void CloseFromGameGUI()
	{
		base.CloseFromGameGUI();
		TooltipBubbleGUI.ChangeAvaibility(available: false);
	}

	protected override bool OnPressedBack()
	{
		GUIElements.me.game_gui.Hide();
		return true;
	}

	protected override bool OnPressedNextSubTab()
	{
		_cur_branch++;
		if (_cur_branch > _max_branch_in_balance)
		{
			_cur_branch = 0;
		}
		if (!MainGame.me.save.IsTechBranchVisible(_cur_branch))
		{
			return OnPressedNextSubTab();
		}
		SelectTechBranch(_cur_branch);
		GJTimer.AddTimer(0f, delegate
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: true);
		});
		return true;
	}

	protected override bool OnPressedPrevSubTab()
	{
		_cur_branch--;
		if (_cur_branch < 0)
		{
			_cur_branch = _max_branch_in_balance;
		}
		if (!MainGame.me.save.IsTechBranchVisible(_cur_branch))
		{
			return OnPressedPrevSubTab();
		}
		SelectTechBranch(_cur_branch);
		GJTimer.AddTimer(0f, delegate
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: true);
		});
		return true;
	}
}
