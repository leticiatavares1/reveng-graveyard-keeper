using System.Collections.Generic;
using Fishing;
using UnityEngine;

public class FishingDistanceChoosingBarSelectionGUI : MonoBehaviour
{
	public GameObject selection_frame;

	public UIGrid grid;

	public FishingDistanceChoosingBarGUI father;

	private bool _is_selected;

	private int _distance;

	private List<FishingDistanceChoosingBarItemGUI> _fish_items = new List<FishingDistanceChoosingBarItemGUI>();

	public void Init(int dist)
	{
		_distance = dist;
		_is_selected = true;
		SetSelected(selected: false);
		father = MainGame.me.gui_elements.fishing.distance_choosing_bar;
		UpdateFishItems(need_reposition: true);
	}

	public void SetSelected(bool selected = true)
	{
		if (_is_selected != selected)
		{
			_is_selected = selected;
			selection_frame.SetActive(_is_selected);
		}
	}

	private void CreateNewFishItem()
	{
		if (_fish_items == null)
		{
			_fish_items = new List<FishingDistanceChoosingBarItemGUI>();
		}
		FishingDistanceChoosingBarItemGUI fishingDistanceChoosingBarItemGUI = Object.Instantiate(father.fish_item_prefab, grid.transform);
		fishingDistanceChoosingBarItemGUI.gameObject.SetActive(value: true);
		_fish_items.Add(fishingDistanceChoosingBarItemGUI);
	}

	private void RemoveFishItem()
	{
		if (_fish_items.Count != 0)
		{
			Object.Destroy(_fish_items[_fish_items.Count - 1].gameObject);
			_fish_items.RemoveAt(_fish_items.Count - 1);
		}
	}

	public void UpdateFishItems(bool need_reposition = false)
	{
		List<FishWithWeight> list = MainGame.me.gui_elements.fishing.fishes_with_weights[_distance];
		float num = 0f;
		foreach (FishWithWeight item4 in list)
		{
			num += item4.weight;
		}
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		List<float> list4 = new List<float>();
		foreach (FishWithWeight item5 in list)
		{
			string item = ItemDefinition.StaticGetNameWithoutQualitySuffix(item5.fish.item_id);
			int num2 = list2.IndexOf(item);
			if (num2 >= 0)
			{
				list4[num2] += item5.weight;
				continue;
			}
			string item2 = "i_unknown_fish";
			string item3 = GUIElements.me.fishing.reservoir_data.id + ":" + (_distance + 1) + ":" + ItemDefinition.StaticGetNameWithoutQualitySuffix(item5.fish.item_id);
			if (MainGame.me.save.known_fishes.Contains(item3))
			{
				foreach (ItemDefinition items_datum in GameBalance.me.items_data)
				{
					if (items_datum.id == item5.fish.item_id)
					{
						item2 = items_datum.GetIcon();
						break;
					}
				}
			}
			list2.Add(item);
			list3.Add(item2);
			list4.Add(item5.weight);
		}
		if (list2.Count > 3)
		{
			string text = string.Empty;
			foreach (string item6 in list2)
			{
				text = text + (string.IsNullOrEmpty(text) ? "" : ", ") + item6;
			}
			Debug.LogError("Wrong fishes count at " + GUIElements.me.fishing.reservoir_data.id + ":" + (_distance + 1) + " == " + list2.Count + ":\n" + text);
		}
		while (list2.Count > _fish_items.Count && _fish_items.Count != 3)
		{
			CreateNewFishItem();
			need_reposition = true;
		}
		while (list2.Count < _fish_items.Count)
		{
			RemoveFishItem();
			need_reposition = true;
		}
		for (int i = 0; i < _fish_items.Count && i != list2.Count; i++)
		{
			_fish_items[i].SetFishItem(list3[i], (num > 0f) ? Mathf.RoundToInt(list4[i] * 100f / num) : 0);
		}
		if (need_reposition)
		{
			grid.Reposition();
			grid.repositionNow = true;
		}
	}
}
