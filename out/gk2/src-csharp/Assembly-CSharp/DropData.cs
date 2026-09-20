using System;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class DropData
{
	[SerializeField]
	private DropType dropType;

	[SerializeField]
	private Item item;

	[SerializeField]
	private Vector3 pos;

	[SerializeField]
	private string worldId;

	[SerializeField]
	private float autoDestroyTimer = -1f;

	private WgoData wgoData;

	private MultiFlagOR<CanNotBeAutoDestroyedReason> canNotBeAutoDestroyed;

	public Vector3 Position
	{
		get
		{
			return pos;
		}
		set
		{
			pos = value;
		}
	}

	public bool IsDroppedFromPlayer { get; private set; }

	public bool IsRemoving { get; private set; }

	public SGuid UniqueId => item.UniqueId;

	public string WorldId
	{
		get
		{
			return worldId;
		}
		set
		{
			worldId = value;
		}
	}

	public string Id => item.id;

	public int Count => item.Count;

	public string IconId => item.Definition.iconId;

	public ItemSize Size => item.Definition.itemSize;

	public Item Item => item;

	public WgoData WgoData => wgoData;

	public bool IsResDrop => Item.id.StartsWith("game_res_");

	public DropType DropType => dropType;

	public float AutoDestroyTimer
	{
		get
		{
			return autoDestroyTimer;
		}
		set
		{
			autoDestroyTimer = value;
		}
	}

	public MultiFlagOR<CanNotBeAutoDestroyedReason> CanNotBeAutoDestroyed
	{
		get
		{
			if (canNotBeAutoDestroyed == null)
			{
				canNotBeAutoDestroyed = new MultiFlagOR<CanNotBeAutoDestroyedReason>();
				if (item != null && item.HasProperty<NeverAutoDestroyDropSerializedItemProperty>())
				{
					canNotBeAutoDestroyed.UpdateFlag(CanNotBeAutoDestroyedReason.Never, newValue: true);
				}
			}
			return canNotBeAutoDestroyed;
		}
		set
		{
			canNotBeAutoDestroyed = value;
		}
	}

	public event Action OnCountChanged;

	public DropData()
	{
	}

	public DropData(Item item, Vector3 pos, string worldId)
	{
		dropType = DropType.Item;
		this.item = item;
		this.pos = pos;
		this.worldId = worldId;
		PlayerData playerData = MainGame.PlayerData;
		if (SpecialPhysicsCastUtils.GetPlayerDropPosition(playerData.position.Value, playerData.Direction, out var foundDropPos) && this.pos == foundDropPos)
		{
			IsDroppedFromPlayer = true;
		}
		if (item.Definition.isLinkedToWgo)
		{
			dropType = DropType.WgoData;
		}
		canNotBeAutoDestroyed = new MultiFlagOR<CanNotBeAutoDestroyedReason>();
		if (item.HasProperty<NeverAutoDestroyDropSerializedItemProperty>())
		{
			canNotBeAutoDestroyed.UpdateFlag(CanNotBeAutoDestroyedReason.Never, newValue: true);
		}
	}

	public DropData(GameResAtom gameResAtom, Vector3 pos, string worldId)
		: this(gameResAtom.ItemFromAtom(), pos, worldId)
	{
	}

	public bool TryAddDropItemPartial(DropData drop)
	{
		if (dropType == DropType.WgoData)
		{
			return false;
		}
		return item.TryAddItemPartial(drop.item);
	}

	public bool AddItem(Item item)
	{
		if (dropType == DropType.WgoData)
		{
			return false;
		}
		return item.TryAddItem(item);
	}

	public int CanAddItemCount(Item item)
	{
		if (dropType == DropType.WgoData)
		{
			return 0;
		}
		return item.CanAddItemCount(item);
	}

	public void MarkAsRemoving()
	{
		IsRemoving = true;
	}

	public void NotifyCountChanged()
	{
		this.OnCountChanged?.Invoke();
	}

	public void TryStartAutoDestroyTimer()
	{
		if (dropType == DropType.Item && item.Definition.itemGroupIds.Contains("body") && item.Definition.itemGroupIds.Contains("corpse"))
		{
			autoDestroyTimer = ConstDef.Get("corpse_auto_destroy_timer").FloatValue;
		}
	}
}
