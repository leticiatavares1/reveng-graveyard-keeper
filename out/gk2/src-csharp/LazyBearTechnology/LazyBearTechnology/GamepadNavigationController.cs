using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

public class GamepadNavigationController : MonoBehaviour
{
	[Serializable]
	public class NavigationGroupTarget
	{
		public int group;

		public GUIDirection direction;

		public NavigationGroupTarget(int group, GUIDirection direction)
		{
			this.group = group;
			this.direction = direction;
		}
	}

	[Serializable]
	public class NavigationGroupSource
	{
		public int group;

		public List<NavigationGroupTarget> groupTargets;

		public NavigationGroupSource(int group, List<NavigationGroupTarget> groupTargets)
		{
			this.group = group;
			this.groupTargets = groupTargets;
		}
	}

	public List<NavigationGroupSource> navigationGroupSources;

	[SerializeField]
	protected List<GamepadNavigationItem> selectableItems = new List<GamepadNavigationItem>();

	private Dictionary<int, GamepadNavigationItem> lastFocusedItems = new Dictionary<int, GamepadNavigationItem>();

	public bool restoreLastInGroup;

	public bool useGridSkippedIfListEmpty;

	public bool ignoreHoldedKeys;

	public bool loopVerticalNavigation;

	public bool checkMaxDistanceBetweenElements;

	public float maxDistanceBetweenElements;

	private bool isEnabled;

	private float guiScale;

	public GamepadNavigationItem FocusedItem
	{
		get
		{
			foreach (GamepadNavigationItem selectableItem in selectableItems)
			{
				if (selectableItem.IsFocused)
				{
					return selectableItem;
				}
			}
			return null;
		}
	}

	public int FocusedItemIndex
	{
		get
		{
			GamepadNavigationItem focusedItem = FocusedItem;
			if (!(focusedItem != null))
			{
				return -1;
			}
			return focusedItem.Index;
		}
	}

	public bool IsEnabled => isEnabled;

	public event Action<GamepadNavigationItem> OnFocusedItemChanged;

	public void Enable()
	{
		if (!isEnabled)
		{
			isEnabled = true;
		}
	}

	public void Enable(bool focusOnFirstActive)
	{
		Enable();
		FocusedItem?.Unfocus();
		ReinitItems(focusOnFirstActive);
	}

	public void ReinitItems(bool focusOnFirstActive, List<GamepadNavigationItem> customItems = null, GamepadNavigationItem skipUnfocusItem = null)
	{
		guiScale = base.transform.lossyScale.x;
		foreach (GamepadNavigationItem selectableItem in selectableItems)
		{
			if (selectableItem != null && selectableItem != skipUnfocusItem)
			{
				selectableItem.Unfocus();
			}
		}
		selectableItems.Clear();
		if (customItems == null)
		{
			selectableItems.AddRange(GetComponentsInChildren<GamepadNavigationItem>());
		}
		else
		{
			selectableItems.AddRange(customItems);
		}
		RemoveNullsAndSetIndexes(selectableItems);
		int num = 0;
		foreach (GamepadNavigationItem selectableItem2 in selectableItems)
		{
			selectableItem2.Init(num++, this, guiScale);
		}
		if (focusOnFirstActive)
		{
			FocusOnFirstActive();
		}
	}

	protected void RemoveNullsAndSetIndexes(List<GamepadNavigationItem> items)
	{
		items.RemoveUnityNulls();
		for (int i = 0; i < items.Count; i++)
		{
			items[i].Index = i;
		}
	}

	public void Disable()
	{
		if (!isEnabled)
		{
			return;
		}
		isEnabled = false;
		foreach (GamepadNavigationItem selectableItem in selectableItems)
		{
			selectableItem.Unfocus();
		}
		selectableItems.Clear();
	}

	public void ResetCustomDirections()
	{
		foreach (GamepadNavigationItem selectableItem in selectableItems)
		{
			selectableItem.ResetCustomDirections();
		}
	}

	public void LinkRoundNavigation()
	{
		if (selectableItems.Count > 1)
		{
			GamepadNavigationItem gamepadNavigationItem = selectableItems[0];
			List<GamepadNavigationItem> list = selectableItems;
			gamepadNavigationItem.SetCustomDirectionItem(GUIDirection.Up, list[list.Count - 1], setAlsoBackwardsCustomDirection: true);
		}
	}

	public void FindAndRememberFocusedItem()
	{
		RememberFocused(FocusedItem);
	}

	public void RememberFocused(GamepadNavigationItem item)
	{
		if (!(item == null))
		{
			if (lastFocusedItems.ContainsKey(item.group))
			{
				lastFocusedItems[item.group] = item;
			}
			else
			{
				lastFocusedItems.Add(item.group, item);
			}
		}
	}

	public bool FocusOnFirstActive(int group = -1)
	{
		int num = -1;
		bool flag = group >= 0;
		foreach (GamepadNavigationItem selectableItem in selectableItems)
		{
			num++;
			if (selectableItem == null)
			{
				Debug.LogError("Found a null item while selecting group = " + group);
			}
			else if (selectableItem.isActiveAndEnabled && selectableItem.Active && (!flag || selectableItem.group == group))
			{
				SetFocusedItem(num);
				return true;
			}
		}
		Debug.LogWarning("no selectable items", this);
		return false;
	}

	public bool FocusOnLastActive(int group = -1)
	{
		bool flag = group >= 0;
		for (int num = selectableItems.Count - 1; num >= 0; num--)
		{
			GamepadNavigationItem gamepadNavigationItem = selectableItems[num];
			if (gamepadNavigationItem == null)
			{
				Debug.LogError("Found a null item while selecting group = " + group);
			}
			else if (gamepadNavigationItem.isActiveAndEnabled && gamepadNavigationItem.Active && (!flag || gamepadNavigationItem.group == group))
			{
				SetFocusedItem(num);
				return true;
			}
		}
		Debug.LogWarning("no selectable items", this);
		return false;
	}

	public bool HaveSavedFocusForGroup(int group)
	{
		if (!lastFocusedItems.ContainsKey(group))
		{
			return false;
		}
		if (lastFocusedItems[group] == null)
		{
			lastFocusedItems.Remove(group);
			return false;
		}
		if (lastFocusedItems[group].isActiveAndEnabled && lastFocusedItems[group].Active)
		{
			return true;
		}
		lastFocusedItems.Remove(group);
		return false;
	}

	public void RestoreFocus(int group = 0)
	{
		if (HaveSavedFocusForGroup(group))
		{
			SetFocusedItem(lastFocusedItems[group]);
			return;
		}
		Debug.LogWarning("no saved last focus for group " + group + ", trying to select any in this group");
		FocusOnFirstActive(group);
	}

	public void RestoreSelection(int group)
	{
		RestoreFocus(group);
		SelectFocusedItem();
	}

	public void SetFocusedItem(GamepadNavigationItem item)
	{
		SetFocusedItem(selectableItems.IndexOf(item));
	}

	public void SetFocusedItem(int focusIndex)
	{
		if (focusIndex < 0 || focusIndex >= selectableItems.Count)
		{
			focusIndex = 0;
		}
		GamepadNavigationItem gamepadNavigationItem = null;
		for (int i = 0; i < selectableItems.Count; i++)
		{
			if (i == focusIndex)
			{
				if (selectableItems[i].Active)
				{
					gamepadNavigationItem = selectableItems[i];
				}
			}
			else
			{
				selectableItems[i].Unfocus();
			}
		}
		if (gamepadNavigationItem != null)
		{
			gamepadNavigationItem.Focus();
			RememberFocused(gamepadNavigationItem);
			this.OnFocusedItemChanged?.Invoke(gamepadNavigationItem);
		}
	}

	public void SelectFocusedItem()
	{
		FocusedItem?.Select();
	}

	public virtual void Navigate(GUIDirection direction)
	{
		RemoveNullsAndSetIndexes(selectableItems);
		if (selectableItems.Count == 0)
		{
			return;
		}
		GamepadNavigationItem focusedItem = FocusedItem;
		if (focusedItem == null)
		{
			SetFocusedItem(0);
			return;
		}
		GamepadNavigationItem customDirectionItem = focusedItem.GetCustomDirectionItem(direction);
		if (customDirectionItem != null && selectableItems.Contains(customDirectionItem))
		{
			if (customDirectionItem != focusedItem)
			{
				SetFocusedItem(customDirectionItem);
			}
			return;
		}
		int group = focusedItem.group;
		Vector2 pos = focusedItem.Pos;
		List<GamepadNavigationItem> list = new List<GamepadNavigationItem>();
		foreach (GamepadNavigationItem selectableItem in selectableItems)
		{
			if (!(selectableItem == focusedItem) && !NeedSkipItemBecauseOfState(selectableItem) && !NeedSkipItemBecauseOfGroup(selectableItem, group) && !NeedSkipItemBecauseOfDirection(selectableItem, pos, direction))
			{
				list.Add(selectableItem);
			}
		}
		if (list.Count == 0)
		{
			GamepadNavigationItem gamepadNavigationItem = TryGetItemFromOtherGroup(FocusedItem.group, pos, direction);
			if (gamepadNavigationItem != null)
			{
				SetFocusedItem(gamepadNavigationItem);
				return;
			}
			if (TryNavigateVerticalLoop(focusedItem, group, direction))
			{
				return;
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		List<GamepadNavigationItem> list2 = new List<GamepadNavigationItem>(list);
		List<GamepadNavigationItem> list3 = new List<GamepadNavigationItem>();
		for (int i = 0; i < list2.Count; i++)
		{
			bool flag = false;
			if (NeedSkipItemBecauseOfGroup(list2[i], group))
			{
				flag = true;
			}
			if (!flag && SkipItemBecauseOfGrid(list2[i], pos, direction))
			{
				list3.Add(list2[i]);
				flag = true;
			}
			if (flag)
			{
				list2.RemoveAt(i);
				i--;
			}
		}
		bool flag2 = false;
		if (useGridSkippedIfListEmpty && list2.Count == 0 && list3.Count > 0)
		{
			list2.AddRange(list3);
			flag2 = true;
		}
		if (list2.Count == 0)
		{
			return;
		}
		GamepadNavigationItem nearestIfCheckMaxDistance;
		GamepadNavigationItem gamepadNavigationItem2 = GetNearestItemInList(list2, pos, direction, out nearestIfCheckMaxDistance);
		if (checkMaxDistanceBetweenElements)
		{
			if (nearestIfCheckMaxDistance == null)
			{
				if (useGridSkippedIfListEmpty && !flag2 && list3.Count > 0)
				{
					GetNearestItemInList(list3, pos, direction, out nearestIfCheckMaxDistance);
					if (nearestIfCheckMaxDistance != null)
					{
						gamepadNavigationItem2 = nearestIfCheckMaxDistance;
					}
				}
			}
			else
			{
				gamepadNavigationItem2 = nearestIfCheckMaxDistance;
			}
		}
		if (restoreLastInGroup)
		{
			int group2 = gamepadNavigationItem2.group;
			if (group2 != group && HaveSavedFocusForGroup(group2))
			{
				RestoreFocus(group2);
				return;
			}
		}
		SetFocusedItem(gamepadNavigationItem2);
	}

	protected virtual GamepadNavigationItem GetNearestItemInList(List<GamepadNavigationItem> items, Vector2 currentPos, GUIDirection direction, out GamepadNavigationItem nearestIfCheckMaxDistance)
	{
		GamepadNavigationItem gamepadNavigationItem = items[0];
		float num = gamepadNavigationItem.CalcDistToCurrentPos(currentPos, direction);
		nearestIfCheckMaxDistance = null;
		float num2 = maxDistanceBetweenElements * LazyUI.ScaleFactor;
		if (checkMaxDistanceBetweenElements && num <= num2)
		{
			nearestIfCheckMaxDistance = gamepadNavigationItem;
		}
		for (int i = 1; i < items.Count; i++)
		{
			float num3 = items[i].CalcDistToCurrentPos(currentPos, direction);
			if (num3 < num)
			{
				num = num3;
				gamepadNavigationItem = items[i];
				if (checkMaxDistanceBetweenElements && num <= num2)
				{
					nearestIfCheckMaxDistance = gamepadNavigationItem;
				}
			}
		}
		return gamepadNavigationItem;
	}

	protected GamepadNavigationItem TryGetItemFromOtherGroup(int groupIndex, Vector2 currentPos, GUIDirection direction)
	{
		for (int i = 0; i < navigationGroupSources.Count; i++)
		{
			if (navigationGroupSources[i].group != groupIndex)
			{
				continue;
			}
			List<GamepadNavigationItem> list = new List<GamepadNavigationItem>();
			bool flag = false;
			int num = -1;
			for (int j = 0; j < navigationGroupSources[i].groupTargets.Count; j++)
			{
				if (navigationGroupSources[i].groupTargets[j].direction != direction)
				{
					continue;
				}
				num = navigationGroupSources[i].groupTargets[j].group;
				flag = true;
				for (int k = 0; k < selectableItems.Count; k++)
				{
					if (selectableItems[k].group == num && !NeedSkipItemBecauseOfState(selectableItems[k]))
					{
						list.Add(selectableItems[k]);
					}
				}
			}
			GamepadNavigationItem nearestIfCheckMaxDistance;
			if (list.Count > 0)
			{
				return GetNearestItemInList(list, currentPos, direction, out nearestIfCheckMaxDistance);
			}
			if (flag)
			{
				return TryGetItemFromOtherGroup(num, currentPos, direction);
			}
		}
		return null;
	}

	private bool TryNavigateVerticalLoop(GamepadNavigationItem currentItem, int currentGroup, GUIDirection direction)
	{
		if (!loopVerticalNavigation)
		{
			return false;
		}
		if (currentGroup != 0)
		{
			return false;
		}
		if (direction != GUIDirection.Up && direction != GUIDirection.Down)
		{
			return false;
		}
		GamepadNavigationItem verticalWrapItem = GetVerticalWrapItem(currentItem, currentGroup, direction);
		if (verticalWrapItem == null)
		{
			return false;
		}
		SetFocusedItem(verticalWrapItem);
		return true;
	}

	private GamepadNavigationItem GetVerticalWrapItem(GamepadNavigationItem currentItem, int currentGroup, GUIDirection direction)
	{
		GamepadNavigationItem result = null;
		float num = ((direction == GUIDirection.Up) ? float.MaxValue : float.MinValue);
		foreach (GamepadNavigationItem selectableItem in selectableItems)
		{
			if (selectableItem == currentItem || NeedSkipItemBecauseOfState(selectableItem) || NeedSkipItemBecauseOfGroup(selectableItem, currentGroup))
			{
				continue;
			}
			float y = selectableItem.Pos.y;
			if (direction == GUIDirection.Up)
			{
				if (y < num)
				{
					num = y;
					result = selectableItem;
				}
			}
			else if (y > num)
			{
				num = y;
				result = selectableItem;
			}
		}
		return result;
	}

	protected bool NeedSkipItemBecauseOfState(GamepadNavigationItem item)
	{
		if (!item.IsFocused && item.isActiveAndEnabled)
		{
			return !item.Active;
		}
		return true;
	}

	protected bool NeedSkipItemBecauseOfGroup(GamepadNavigationItem item, int neededGroup)
	{
		return item.group != neededGroup;
	}

	protected bool NeedSkipItemBecauseOfDirection(GamepadNavigationItem item, Vector2 currentPos, GUIDirection direction)
	{
		return !item.CorrectDirection(currentPos, direction);
	}

	protected bool SkipItemBecauseOfGrid(GamepadNavigationItem item, Vector2 currentPos, GUIDirection direction)
	{
		return !item.CorrectGrid(currentPos, direction);
	}
}
