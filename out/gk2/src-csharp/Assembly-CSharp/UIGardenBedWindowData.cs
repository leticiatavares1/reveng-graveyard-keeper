using System.Collections.Generic;
using LazyBearTechnology;

public class UIGardenBedWindowData : LazyWidgetDataBase
{
	private bool isSubscribedToDataChanges;

	private bool wasPlayerSetAsWorker;

	public CraftElement CraftElement { get; private set; }

	public CraftDef CraftDefinition { get; private set; }

	public WgoData WgoData { get; private set; }

	public UIInfoWidgetData UIInfoWidgetData { get; private set; }

	public List<PerkData> GardenPerks { get; private set; }

	public bool IsGrowing { get; private set; }

	public UIGardenBedWindowData(WgoData wgoData)
	{
		WgoData = wgoData;
		CraftElement = ((wgoData.CraftComponent.CurrentCraftElement == null) ? null : (wgoData.CraftComponent.CurrentCraftElement as CraftElement));
		CraftDefinition = CraftElement?.Definition;
		UIInfoWidgetData = new UIInfoWidgetData(wgoData);
		UIInfoWidgetData.ForcedWorker = MainGame.PlayerController;
		GardenPerks = new List<PerkData>();
		IsGrowing = CraftElement != null;
		foreach (PerkData activePerk in WgoData.ActivePerks)
		{
			if (activePerk.Definition.IsFertilizerPerk)
			{
				GardenPerks.Add(activePerk);
			}
		}
	}

	public void UpdateData()
	{
		CraftElement = ((WgoData.CraftComponent.CurrentCraftElement == null) ? null : (WgoData.CraftComponent.CurrentCraftElement as CraftElement));
		IsGrowing = CraftElement != null;
		CraftDefinition = CraftElement?.Definition;
		UIInfoWidgetData = new UIInfoWidgetData(WgoData);
		UIInfoWidgetData.ForcedWorker = MainGame.PlayerController;
		GardenPerks = new List<PerkData>();
		foreach (PerkData activePerk in WgoData.ActivePerks)
		{
			if (activePerk.Definition.IsFertilizerPerk)
			{
				GardenPerks.Add(activePerk);
			}
		}
	}

	public void TrySetWorker()
	{
		if (WgoData.Worker == null)
		{
			wasPlayerSetAsWorker = true;
			WgoData.TrySetWorker(MainGame.PlayerController);
		}
	}

	public void TryRemoveWorker()
	{
		if (wasPlayerSetAsWorker)
		{
			wasPlayerSetAsWorker = false;
			WgoData.ClearWorker();
		}
	}
}
