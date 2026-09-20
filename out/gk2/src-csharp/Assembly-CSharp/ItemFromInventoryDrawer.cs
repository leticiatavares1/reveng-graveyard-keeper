using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class ItemFromInventoryDrawer : MonoBehaviour
{
	[SerializeField]
	private WgoPart wgoPart;

	[SerializeField]
	private GameObject itemObject;

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	private bool isSubscribed;

	private Inventory Inventory => wgoPart.Wgo.Data.Inventory;

	private void OnEnable()
	{
		if (wgoPart?.Wgo?.Data != null)
		{
			if (!isSubscribed)
			{
				Inventory.OnItemsAdd += OnItemsChanged;
				Inventory.OnItemsRemove += OnItemsChanged;
				isSubscribed = true;
			}
			Redraw();
		}
	}

	private void OnDisable()
	{
		if (wgoPart?.Wgo?.Data != null && isSubscribed)
		{
			Inventory.OnItemsAdd -= OnItemsChanged;
			Inventory.OnItemsRemove -= OnItemsChanged;
			isSubscribed = false;
		}
	}

	private void OnDestroy()
	{
		if (wgoPart?.Wgo?.Data != null && isSubscribed)
		{
			Inventory.OnItemsAdd -= OnItemsChanged;
			Inventory.OnItemsRemove -= OnItemsChanged;
			isSubscribed = false;
		}
	}

	private void OnItemsChanged(List<Item> items)
	{
		Redraw();
	}

	private void Redraw()
	{
		if (Inventory.Data.InventoryFillSize <= 0 || Inventory.Data.Inventory[0].IsEmpty)
		{
			itemObject.SetActive(value: false);
			return;
		}
		itemObject.SetActive(value: true);
		spriteRenderer.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(Inventory.Data.Inventory[0].Definition.iconId);
	}
}
