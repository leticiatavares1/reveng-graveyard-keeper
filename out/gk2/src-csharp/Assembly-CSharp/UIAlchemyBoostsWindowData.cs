using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UIAlchemyBoostsWindowData : LazyWidgetDataBase
{
	public Wgo AssignedWgo { get; private set; }

	public List<CraftElement> CraftsToDisplay { get; private set; }

	public Action<CraftElement, List<NeedItemData>> OnCraftPressed { get; private set; }

	public Func<CraftElement, List<NeedItemData>, bool> CanCraft { get; private set; }

	public PlayerData PlayerData { get; private set; }

	public List<Inventory> AdditionalInventories { get; private set; }

	public UIAlchemyBoostsWindowData(Wgo wgo, PlayerData playerData, List<CraftElement> craftsToDisplay, Action<CraftElement, List<NeedItemData>> onCraftPressed, Func<CraftElement, List<NeedItemData>, bool> canCraft, List<Inventory> additionalInventories = null)
	{
		AssignedWgo = wgo;
		PlayerData = playerData;
		CraftsToDisplay = craftsToDisplay;
		OnCraftPressed = onCraftPressed;
		CanCraft = canCraft;
		AdditionalInventories = additionalInventories;
	}
}
