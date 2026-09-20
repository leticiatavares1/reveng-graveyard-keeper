using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HUDTasksGUI : MonoBehaviour
{
	public UITableOrGrid table;

	public HUDTaskItemGUI item_prefab;

	private string _npc_id;

	private List<HUDTaskItemGUI> _items = new List<HUDTaskItemGUI>();

	private List<string> _hidden_tasks = new List<string>();

	public void Init()
	{
		item_prefab.SetActive(active: false);
	}

	public void Draw(string npc_id)
	{
		_npc_id = npc_id;
		if (string.IsNullOrEmpty(npc_id))
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		table.DestroyChildren(new HUDTaskItemGUI[1] { item_prefab });
		_items.Clear();
		foreach (KnownNPC.TaskState task in MainGame.me.save.known_npcs.GetOrCreateNPC(npc_id).tasks)
		{
			if (task.state != KnownNPC.TaskState.State.Complete)
			{
				HUDTaskItemGUI hUDTaskItemGUI = item_prefab.Copy();
				hUDTaskItemGUI.txt.text = task.GetTaskText();
				if (task.is_dlc_stories_task)
				{
					hUDTaskItemGUI.quest_marker.text = "(*2)";
				}
				else if (task.is_dlc_refugee_task)
				{
					hUDTaskItemGUI.quest_marker.text = "(q_marker_refugee)";
				}
				else if (task.is_dlc_souls_task)
				{
					hUDTaskItemGUI.quest_marker.text = "(q_souls)";
				}
				else
				{
					hUDTaskItemGUI.quest_marker.text = "(*)";
				}
				hUDTaskItemGUI.linked_task = task;
				if (_hidden_tasks.Contains(task.id))
				{
					hUDTaskItemGUI.GetComponent<UIWidget>().alpha = 0f;
				}
				_items.Add(hUDTaskItemGUI);
			}
		}
		table.Reposition();
		base.gameObject.SetActive(value: false);
		base.gameObject.SetActive(_items.Count > 0);
	}

	public void Redraw()
	{
		Draw(_npc_id);
		GetComponentInParent<UIPanel>().BroadcastMessage("UpdateAnchors");
	}

	public Transform GetMarkerPointOfTask(string npc_id, string task_id)
	{
		if (_npc_id != npc_id)
		{
			return null;
		}
		if (!base.gameObject.activeInHierarchy)
		{
			return null;
		}
		foreach (HUDTaskItemGUI item in _items)
		{
			if (item.linked_task.id == task_id)
			{
				return item.marker_point;
			}
		}
		return null;
	}

	public void SetTaskHiddenState(bool hidden, string task_id)
	{
		if (hidden)
		{
			if (!_hidden_tasks.Contains(task_id))
			{
				_hidden_tasks.Add(task_id);
			}
			return;
		}
		if (_hidden_tasks.Contains(task_id))
		{
			_hidden_tasks.Remove(task_id);
		}
		foreach (HUDTaskItemGUI item in _items)
		{
			UIWidget w = item.GetComponent<UIWidget>();
			if (item.linked_task.id == task_id)
			{
				DOTween.To(() => w.alpha, delegate(float x)
				{
					w.alpha = x;
				}, 1f, 0.2f);
			}
		}
		GetComponentInParent<UIPanel>().BroadcastMessage("UpdateAnchors");
	}
}
