using UnityEngine;

public class FishingDistanceChoosingBarGUI : MonoBehaviour
{
	public GameObject backs_gfx;

	public GameObject marker;

	public FishingDistanceChoosingBarSelectionGUI selection_left;

	public FishingDistanceChoosingBarSelectionGUI selection_center;

	public FishingDistanceChoosingBarSelectionGUI selection_right;

	public FishingDistanceChoosingBarItemGUI fish_item_prefab;

	private bool _is_to_right;

	private float _bar_length;

	private bool _marker_is_active;

	private int _prev_zone = -1;

	public void Init()
	{
		_is_to_right = MainGame.me.gui_elements.fishing.is_to_right;
		_bar_length = GetComponent<UIWidget>().width - 30;
		string id = MainGame.me.gui_elements.fishing.reservoir_data.id;
		for (int i = 0; i < backs_gfx.transform.childCount; i++)
		{
			Transform child = backs_gfx.transform.GetChild(i);
			child.gameObject.SetActive(child.gameObject.name == id);
		}
		fish_item_prefab.gameObject.SetActive(value: true);
		selection_left.Init((!_is_to_right) ? 2 : 0);
		selection_center.Init(1);
		selection_right.Init(_is_to_right ? 2 : 0);
		fish_item_prefab.gameObject.SetActive(value: false);
		_marker_is_active = true;
		_prev_zone = -1;
		SetMarkerActive(set_active: false);
	}

	public void SetMarkerPos(float pos)
	{
		pos = ((!_is_to_right) ? (0.5f - pos) : (pos - 0.5f));
		int num = Mathf.CeilToInt((pos + 0.5f) * 3f);
		if (num < 1)
		{
			num = 1;
		}
		else if (num > 3)
		{
			num = 3;
		}
		pos *= _bar_length;
		Transform transform = marker.transform;
		transform.localPosition = new Vector3(pos, transform.localPosition.y, 0f);
		selection_left.SetSelected(num == 1);
		selection_center.SetSelected(num == 2);
		selection_right.SetSelected(num == 3);
		if (num != _prev_zone)
		{
			_prev_zone = num;
			Sounds.OnGUITabClick();
		}
	}

	public void UpdateBar()
	{
		selection_left.UpdateFishItems();
		selection_center.UpdateFishItems();
		selection_right.UpdateFishItems();
	}

	public void SetMarkerActive(bool set_active)
	{
		if (_marker_is_active != set_active)
		{
			_marker_is_active = set_active;
			marker.SetActive(_marker_is_active);
		}
	}
}
