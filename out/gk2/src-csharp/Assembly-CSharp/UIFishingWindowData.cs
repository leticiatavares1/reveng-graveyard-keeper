using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using UnityEngine;

public class UIFishingWindowData : LazyWidgetDataBase
{
	private Wgo reservoir;

	private ItemDef fishingRodDef;

	private List<FishingDef> reservoirFishings = new List<FishingDef>();

	private List<Item> baitItems = new List<Item>();

	public Wgo Reservoir => reservoir;

	public ItemDef FishingRodDef => fishingRodDef;

	public List<Item> BaitItems => baitItems;

	public List<FishingDef> ReservoirFishings => reservoirFishings;

	public UIFishingWindowData(Wgo reservoirWgo, ItemDef playerFishingRodDef)
	{
		reservoir = reservoirWgo;
		fishingRodDef = playerFishingRodDef;
		UpdateAvailableFishingAndBaits();
	}

	public void UpdateAvailableFishingAndBaits()
	{
		if (reservoir == null)
		{
			Debug.LogError("[UIFishingWindowData]: reservoir wgo is null");
			return;
		}
		if (reservoirFishings == null)
		{
			Debug.LogError("[UIFishingWindowData]: There's no available fishing for this reservoir wgo [" + reservoir.Data.id + "]");
			return;
		}
		reservoirFishings = GameBalance.Me.fishingDefs.FindAll((FishingDef x) => x.reservoirId.Equals(reservoir.Data.id));
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < reservoirFishings.Count; i++)
		{
			for (int j = 0; j < reservoirFishings[i].baitMod.List.Count; j++)
			{
				GameResAtom gameResAtom = reservoirFishings[i].baitMod.List[j];
				hashSet.Add(gameResAtom.type);
			}
		}
		hashSet.Add("no_bait");
		baitItems.Clear();
		List<string> list = hashSet.ToList();
		for (int k = 0; k < list.Count; k++)
		{
			string text = list[k];
			if (text == "no_bait")
			{
				baitItems.Insert(0, new Item(text));
			}
			else
			{
				baitItems.Add(new Item(text, MainGame.PlayerData.inventory.Data.GetTotalCountInInventory(text)));
			}
		}
	}
}
