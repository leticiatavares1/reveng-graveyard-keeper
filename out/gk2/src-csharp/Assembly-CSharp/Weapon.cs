using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	public AnimationState animState = AnimationState.AttackCommon;

	public string defaultItemDefId;

	[SerializeField]
	private ItemType itemType;

	private List<IDamageDealer> dealers;

	private ItemDef itemDef;

	public List<IDamageDealer> Dealers => dealers ?? GetComponentsInChildren<IDamageDealer>(includeInactive: true).ToList();

	public ItemType ItemType => itemType;

	public ItemDef ItemDef
	{
		get
		{
			if (itemDef == null)
			{
				itemDef = GameBalance.Me.GetData<ItemDef>(defaultItemDefId);
			}
			return itemDef;
		}
		set
		{
			itemDef = value;
		}
	}
}
