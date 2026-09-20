using System;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using LinqTools;

public class UIBaseCraftWindowData : LazyWidgetDataBase
{
	public const string DEFAULT_TAB_ID = "default_tab";

	public CraftComponent.DelCraftAddedToQueue onCraftAddedToQueue;

	public CraftComponent.DelCraftRemovedFromQueue onCraftRemovedFromQueue;

	public Action onCraftStarted;

	public Action onCraftEnded;

	private bool subscribedEvents;

	private bool isAlchemyWorkbench;

	private bool isGravePartRemove;

	public CraftComponent CraftComponent { get; private set; }

	public HashSet<CraftDef> AllCrafts { get; private set; }

	public List<KeyValuePair<string, List<CraftDef>>> CraftsByTabs { get; private set; }

	public List<KeyValuePair<string, List<CraftDef>>> ExtensionCrafts { get; private set; } = new List<KeyValuePair<string, List<CraftDef>>>();


	public Action<CraftElement> OnAddToQueuePressed { get; private set; }

	public Action<CraftElement> OnStartCraftPressed { get; private set; }

	public Action<CraftElementBase> OnQueueElementRemovePressed { get; private set; }

	public UIInfoWidgetData InfoWidgetData { get; private set; }

	public Wgo AssignedWgo { get; private set; }

	public bool IsAutocraftsWgo { get; private set; }

	public bool IsGravePartRemove => isGravePartRemove;

	public bool IsAddToQueueDisabledForAllCrafts
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			foreach (CraftDef item in EnumerateDisplayedCrafts())
			{
				flag = true;
				if (!IsCraftQueueDisabled(item))
				{
					flag2 = false;
				}
			}
			return flag && flag2;
		}
	}

	private static bool IsCraftQueueDisabled(CraftDef craft)
	{
		if (!craft.isAddToQueueDisabled)
		{
			return craft.skipQueue;
		}
		return true;
	}

	private IEnumerable<CraftDef> EnumerateDisplayedCrafts()
	{
		HashSet<CraftDef> seen = new HashSet<CraftDef>();
		if (CraftsByTabs != null)
		{
			foreach (KeyValuePair<string, List<CraftDef>> craftsByTab in CraftsByTabs)
			{
				if (craftsByTab.Value == null || craftsByTab.Value.Count == 0)
				{
					continue;
				}
				foreach (CraftDef item in craftsByTab.Value)
				{
					if (item != null && seen.Add(item))
					{
						yield return item;
					}
				}
			}
		}
		if (ExtensionCrafts == null)
		{
			yield break;
		}
		foreach (KeyValuePair<string, List<CraftDef>> extensionCraft in ExtensionCrafts)
		{
			if (extensionCraft.Value == null || extensionCraft.Value.Count == 0)
			{
				continue;
			}
			foreach (CraftDef item2 in extensionCraft.Value)
			{
				if (item2 != null && seen.Add(item2))
				{
					yield return item2;
				}
			}
		}
	}

	public UIBaseCraftWindowData(Wgo wgo, Action<CraftElement> onAddToQueuePressed, Action<CraftElement> onStartCraftPressed = null)
	{
		UIBaseCraftWindowData uIBaseCraftWindowData = this;
		AssignedWgo = wgo;
		isAlchemyWorkbench = AssignedWgo.Data.id == "alchemy_workbench";
		CraftComponent = wgo.Data.CraftComponent;
		AllCrafts = LinqTools.Enumerable.OfType<CraftDef>(wgo.Data.CraftComponent.CraftsIn).ToHashSet();
		KnowledgeSystem knowledge = MainGame.Instance.GameSave.knowledgeSystem;
		foreach (string attachedWorkbenchExtensionId in wgo.Data.Definition.attachedWorkbenchExtensionIds)
		{
			List<CraftDef> list = LinqTools.Enumerable.ToList(LinqTools.Enumerable.Where(GameBalance.Me.GetWorkbenchExtensionCrafts(wgo.Data.id, attachedWorkbenchExtensionId), (CraftDef craft) => !uIBaseCraftWindowData.isAlchemyWorkbench && !knowledge.IsOneTimeCraftCompleted(craft)));
			if (list.Count != 0)
			{
				ExtensionCrafts.Add(new KeyValuePair<string, List<CraftDef>>(attachedWorkbenchExtensionId, list));
			}
		}
		OnAddToQueuePressed = onAddToQueuePressed;
		OnStartCraftPressed = onStartCraftPressed;
		OnQueueElementRemovePressed = RemoveQueueElement;
		InfoWidgetData = new UIInfoWidgetData(AssignedWgo.Data);
		IsAutocraftsWgo = wgo.Data.CraftComponent.HasCraftsByBalance && wgo.Data.CraftComponent.AvailableCrafts[0].isAuto;
		SortCraftsByTabs();
		List<string> unlockedCrafts = knowledge.unlockedCrafts;
		foreach (KeyValuePair<string, List<CraftDef>> item in LinqTools.Enumerable.Concat(CraftsByTabs, ExtensionCrafts))
		{
			item.Value.Sort(delegate(CraftDef x, CraftDef y)
			{
				bool num = unlockedCrafts.Contains(x.id);
				bool flag = unlockedCrafts.Contains(y.id);
				if (num)
				{
					if (flag)
					{
						return 0;
					}
					return -1;
				}
				return flag ? 1 : 0;
			});
		}
	}

	public UIBaseCraftWindowData(Wgo graveWgo, Item gravePart, Action<CraftElement> onCraftPressed)
	{
		AssignedWgo = graveWgo;
		CraftComponent = graveWgo.Data.CraftComponent;
		OnAddToQueuePressed = onCraftPressed;
		OnStartCraftPressed = onCraftPressed;
		InfoWidgetData = new UIInfoWidgetData(graveWgo.Data, gravePart);
		FillGravePartCrafts(gravePart);
		isGravePartRemove = true;
	}

	public void SubscribeEvents()
	{
		if (!subscribedEvents)
		{
			CraftComponent.OnCraftAddedToQueue += onCraftAddedToQueue;
			CraftComponent.OnCraftRemovedFromQueue += onCraftRemovedFromQueue;
			CraftComponent.OnCraftStart += onCraftStarted;
			CraftComponent.OnCraftFinish += onCraftEnded;
			subscribedEvents = true;
		}
	}

	public void UnsubscribeEvents()
	{
		if (subscribedEvents)
		{
			CraftComponent.OnCraftAddedToQueue -= onCraftAddedToQueue;
			CraftComponent.OnCraftRemovedFromQueue -= onCraftRemovedFromQueue;
			CraftComponent.OnCraftStart -= onCraftStarted;
			CraftComponent.OnCraftFinish -= onCraftEnded;
			subscribedEvents = false;
		}
	}

	public static CraftDef FindGravePartRemoveCraft(CraftComponent craftComponent, Item gravePartItem)
	{
		if (craftComponent?.AvailableCrafts == null || gravePartItem == null)
		{
			return null;
		}
		foreach (CraftDefBase availableCraft in craftComponent.AvailableCrafts)
		{
			if (availableCraft is CraftDef craftDef && (ContainsGravePart(craftDef.dropFromWgoItemsEnd, gravePartItem.id) || ContainsGravePart(craftDef.dropFromWgoItemsStart, gravePartItem.id)))
			{
				return craftDef;
			}
		}
		return null;
	}

	private static bool ContainsGravePart(List<NeedItemData> dropItems, string gravePartItemId)
	{
		if (dropItems == null)
		{
			return false;
		}
		foreach (NeedItemData dropItem in dropItems)
		{
			if (dropItem.id == gravePartItemId)
			{
				return true;
			}
		}
		return false;
	}

	private void FillGravePartCrafts(Item gravePartItem)
	{
		CraftDef craftDef = FindGravePartRemoveCraft(CraftComponent, gravePartItem);
		AllCrafts = ((craftDef != null) ? new HashSet<CraftDef> { craftDef } : new HashSet<CraftDef>());
		SortCraftsByTabs();
	}

	private void RemoveQueueElement(CraftElementBase craftElement)
	{
		CraftElementBase currentCraftElement = CraftComponent.CurrentCraftElement;
		if (currentCraftElement != null && currentCraftElement == craftElement)
		{
			CraftComponent.RemoveCurNotStartedCraft();
		}
		else
		{
			CraftComponent.RemoveFromQueue(craftElement);
		}
	}

	private void SortCraftsByTabs()
	{
		CraftsByTabs = new List<KeyValuePair<string, List<CraftDef>>>();
		CraftsByTabs.Add(new KeyValuePair<string, List<CraftDef>>("default_tab", new List<CraftDef>()));
		foreach (CraftDef allCraft in AllCrafts)
		{
			if ((!isAlchemyWorkbench || MainGame.Instance.GameSave.knowledgeSystem.unlockedCrafts.Contains(allCraft.id)) && string.IsNullOrEmpty(allCraft.extensionNeedId))
			{
				string tabId = (string.IsNullOrEmpty(allCraft.tabId) ? "default_tab" : allCraft.tabId);
				int num = CraftsByTabs.FindIndex((KeyValuePair<string, List<CraftDef>> x) => x.Key == tabId);
				if (num != -1)
				{
					CraftsByTabs[num].Value.Add(allCraft);
					continue;
				}
				CraftsByTabs.Add(new KeyValuePair<string, List<CraftDef>>(tabId, new List<CraftDef> { allCraft }));
			}
		}
	}
}
