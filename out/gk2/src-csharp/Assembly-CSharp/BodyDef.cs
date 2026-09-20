using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class BodyDef : BalanceBaseObject
{
	[AutoParse("body_item")]
	public string linkedBodyItemId;

	[AutoParse("tier")]
	public int tier;

	[AutoParse("body_parts")]
	public List<string> parts = new List<string>();

	[AutoParse("pockets")]
	public List<string> pocket = new List<string>();

	[AutoParse("burial_reward")]
	public List<string> burialReward = new List<string>();

	[AutoParse("armor_id")]
	public string armorId;

	[AutoParse("hands_id")]
	public string handsId;

	public Item GenerateItem()
	{
		Item item = new Item(linkedBodyItemId);
		foreach (string part in parts)
		{
			item.AddItemToInventory(new Item(part));
		}
		foreach (string item2 in pocket)
		{
			item.AddItemToInventory(new Item(item2));
		}
		foreach (string item3 in burialReward)
		{
			item.AddItemToInventory(new Item(item3));
		}
		BodyZombieSkinSerializedItemProperty bodyZombieSkinSerializedItemProperty = new BodyZombieSkinSerializedItemProperty();
		(bodyZombieSkinSerializedItemProperty.body, bodyZombieSkinSerializedItemProperty.head, _, bodyZombieSkinSerializedItemProperty.headLut) = ZombieSkinHelper.RollZombie("zombie_worker");
		item.AddProperty(bodyZombieSkinSerializedItemProperty);
		BodyZombieStartItemsSerializedItemProperty bodyZombieStartItemsSerializedItemProperty = new BodyZombieStartItemsSerializedItemProperty();
		bodyZombieStartItemsSerializedItemProperty.armorId = armorId;
		bodyZombieStartItemsSerializedItemProperty.handsId = handsId;
		item.AddProperty(bodyZombieStartItemsSerializedItemProperty);
		Debug.Log($"Created body :[{id}] skin body:[{bodyZombieSkinSerializedItemProperty.body}] head:[{bodyZombieSkinSerializedItemProperty.head}] headLut:[{bodyZombieSkinSerializedItemProperty.headLut}]");
		return item;
	}
}
