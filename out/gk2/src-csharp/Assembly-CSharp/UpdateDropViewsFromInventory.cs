using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UpdateDropViewsFromInventory : ConditionalDrawerActionBase
{
	[Tooltip("List of Bid drops to update")]
	public List<DropViewAtomMesh> bigDrops = new List<DropViewAtomMesh>();

	public override void Execute(ConditionalDrawerContext context, bool conditionMet)
	{
		if (context.WgoData == null)
		{
			return;
		}
		Inventory inventory = context.WgoData.Inventory;
		int num = inventory.Data.Inventory.Count - 1;
		for (int i = 0; i < bigDrops.Count; i++)
		{
			if (!(bigDrops[i] == null))
			{
				if (i > num)
				{
					bigDrops[i].Deactivate();
				}
				else
				{
					bigDrops[i].Activate(inventory.Data.Inventory[i].Definition.iconId);
				}
			}
		}
	}
}
