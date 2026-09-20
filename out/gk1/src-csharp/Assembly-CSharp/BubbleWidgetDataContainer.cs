using System;
using System.Collections.Generic;
using LinqTools;

public class BubbleWidgetDataContainer
{
	public WidgetsBubbleGUI.Alignment alignment;

	public List<BubbleWidgetData> data_list = new List<BubbleWidgetData>();

	private WorldGameObject _linked_wgo;

	private InteractionBubbleGUI _bubble_gui;

	public WorldGameObject linked_wgo => _linked_wgo;

	public bool has_data => data_list.Count > 0;

	public BubbleWidgetDataContainer(WorldGameObject linked_wgo)
	{
		_linked_wgo = linked_wgo;
	}

	public BubbleWidgetDataContainer GetCopy()
	{
		BubbleWidgetDataContainer bubbleWidgetDataContainer = new BubbleWidgetDataContainer(WidgetsBubbleGUI.Alignment.Center);
		bubbleWidgetDataContainer.alignment = alignment;
		bubbleWidgetDataContainer._linked_wgo = _linked_wgo;
		bubbleWidgetDataContainer._bubble_gui = _bubble_gui;
		bubbleWidgetDataContainer.data_list.AddRange(data_list);
		return bubbleWidgetDataContainer;
	}

	public BubbleWidgetDataContainer(WidgetsBubbleGUI.Alignment alignment = WidgetsBubbleGUI.Alignment.Center, params BubbleWidgetData[] data_params)
	{
		this.alignment = alignment;
		data_list = data_params.ToList();
	}

	public void SetData(params BubbleWidgetData[] data_params)
	{
		data_list = data_params.ToList();
	}

	public void SetData(List<BubbleWidgetData> data_list)
	{
		if (data_list == null)
		{
			ClearData();
		}
		else
		{
			this.data_list = data_list;
		}
	}

	public T GetData<T>() where T : BubbleWidgetData
	{
		Type typeFromHandle = typeof(T);
		foreach (BubbleWidgetData item in data_list)
		{
			if (item.GetType() == typeFromHandle)
			{
				return item as T;
			}
		}
		return null;
	}

	public void RemoveData<T>() where T : BubbleWidgetData
	{
		Type typeFromHandle = typeof(T);
		for (int i = 0; i < data_list.Count; i++)
		{
			if (!(data_list[i].GetType() != typeFromHandle))
			{
				data_list.RemoveAt(i);
				i--;
			}
		}
	}

	public void AddData(params BubbleWidgetData[] data_params)
	{
		foreach (BubbleWidgetData bubbleWidgetData in data_params)
		{
			if (bubbleWidgetData != null)
			{
				data_list.Add(bubbleWidgetData);
			}
		}
	}

	public void AddInteractionHint(string hint)
	{
		if (!string.IsNullOrEmpty(hint))
		{
			data_list.Add(new BubbleWidgetTextData(hint, UITextStyles.TextStyle.InteractionHint));
		}
	}

	public void ClearData()
	{
		data_list.Clear();
	}

	public void SetWGOQualityData(BubbleWidgetTextData data)
	{
		RemoveWGOQualityData();
		data_list.Add(data);
	}

	public bool RemoveWGOQualityData()
	{
		return RemoveDataWithID(BubbleWidgetData.WidgetID.Quality);
	}

	public bool RemoveDataWithID(BubbleWidgetData.WidgetID widget_id)
	{
		if (data_list.Count > 0)
		{
			int index = data_list.Count - 1;
			if (data_list[index] is BubbleWidgetTextData bubbleWidgetTextData && bubbleWidgetTextData.widget_id == widget_id)
			{
				data_list.RemoveAt(index);
				return true;
			}
		}
		return false;
	}

	public void Redraw()
	{
		if (has_data)
		{
			if (_bubble_gui == null)
			{
				_bubble_gui = InteractionBubbleGUI.Show(_linked_wgo, this);
			}
			else
			{
				_bubble_gui.Redraw(is_hint: true);
			}
		}
		else if (_bubble_gui != null)
		{
			_bubble_gui.DestroyMe();
			_bubble_gui = null;
		}
		if (MainGame.game_started && _bubble_gui == null && MainGame.me.player.components.character.wgo_hilighted_for_work == _linked_wgo)
		{
			MainGame.me.player.components.character.wgo_hilighted_for_work = null;
		}
	}

	public InteractionBubbleGUI GetBubbleGUI()
	{
		return _bubble_gui;
	}

	public bool DoesContainWidgetDataWithID(BubbleWidgetData.WidgetID widget_id)
	{
		foreach (BubbleWidgetData item in data_list)
		{
			if (item.widget_id == widget_id)
			{
				return true;
			}
		}
		return false;
	}

	public void SetWidgetDataWithID(BubbleWidgetData wdata, BubbleWidgetData.WidgetID widget_id)
	{
		int num = -1;
		for (int i = 0; i < data_list.Count; i++)
		{
			if (data_list[i].widget_id == widget_id)
			{
				num = i;
				if (wdata == null)
				{
					data_list.RemoveAt(i);
					break;
				}
				wdata.widget_id = widget_id;
				data_list[i] = wdata;
				break;
			}
		}
		if (num == -1 && wdata != null)
		{
			wdata.widget_id = widget_id;
			AddData(wdata);
			num = data_list.Count - 1;
		}
	}
}
