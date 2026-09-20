using System.Collections.Generic;
using UnityEngine;

public class SoulContainerGUI : BaseGUI
{
	[SerializeField]
	private UniversalObjectInfoGUI _universal_object_info;

	[SerializeField]
	private UIGrid _grid;

	[SerializeField]
	private SoulContainerWidget _soul_container_widget;

	private List<SoulContainerWidget> _cached_widgets;

	public override void Init()
	{
		_cached_widgets = new List<SoulContainerWidget>();
		_soul_container_widget.SetActive(active: false);
		base.Init();
	}

	public void Open(WorldGameObject wgo)
	{
		base.Open();
		int inventory_size = wgo.obj_def.inventory_size;
		int num = _cached_widgets.Count;
		for (int i = 0; i < inventory_size; i++)
		{
			if (num > 0)
			{
				_cached_widgets[i].Draw(i, wgo);
				_cached_widgets[i].SetActive(active: true);
				num--;
			}
			else
			{
				SoulContainerWidget soulContainerWidget = _soul_container_widget.Copy();
				_cached_widgets.Add(soulContainerWidget);
				soulContainerWidget.Draw(i, wgo);
			}
		}
		_universal_object_info.Draw(wgo.GetUniversalObjectInfo());
		_grid.Reposition();
	}

	public void Redraw(WorldGameObject wgo)
	{
		int num = 0;
		foreach (SoulContainerWidget cached_widget in _cached_widgets)
		{
			if (cached_widget.gameObject.activeSelf)
			{
				cached_widget.Draw(num, wgo);
				num++;
			}
		}
	}

	public override void Hide(bool play_hide_sound = true)
	{
		for (int i = 0; i < _cached_widgets.Count; i++)
		{
			_cached_widgets[i].SetActive(active: false);
		}
		base.Hide(play_hide_sound);
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}
}
