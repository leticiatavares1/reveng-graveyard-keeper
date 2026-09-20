using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DropCollectGUI : MonoBehaviour
{
	public DropCollectItem item_prefab;

	private static DropCollectGUI _me;

	public Transform root_for_items;

	public UIGrid grid;

	private List<DropCollectItem> _items = new List<DropCollectItem>();

	public void Start()
	{
		_me = this;
		if (item_prefab == null)
		{
			Debug.LogError("Drop item_prefab is null");
		}
		else
		{
			item_prefab.gameObject.SetActive(value: false);
		}
	}

	public static void OnMoneyCollected(float money)
	{
		OnDropCollected(new Item("money", Mathf.FloorToInt(money * 100f)));
	}

	public static void OnDropCollected(Item item)
	{
		if (_me == null || (item.definition != null && item.definition.item_size > 1))
		{
			return;
		}
		foreach (DropCollectItem item2 in _me._items)
		{
			if (item2.item_id == item.id)
			{
				item2.AddMoreItems(item.value);
				return;
			}
		}
		DropCollectItem dropCollectItem = _me.item_prefab.Copy(_me.root_for_items);
		Vector3 position = dropCollectItem.transform.position;
		position.y = -1f;
		dropCollectItem.transform.position = position;
		dropCollectItem.Draw(item);
		_me._items.Add(dropCollectItem);
	}

	public static void RedrawGrid(Transform t = null)
	{
		if (!(_me == null))
		{
			_me.grid.AddChild(t);
		}
	}

	public static void Despawn(DropCollectItem i)
	{
		if (!(_me == null))
		{
			_me._items.Remove(i);
			_me.grid.RemoveChild(i.transform);
			UIWidget w = i.gameObject.GetComponent<UIWidget>();
			DOTween.To(() => w.alpha, delegate(float x)
			{
				w.alpha = x;
			}, 0f, 0.2f);
			i.gameObject.transform.DOMoveY(-1f, 0.2f).OnComplete(delegate
			{
				Object.Destroy(i.gameObject);
			});
		}
	}
}
