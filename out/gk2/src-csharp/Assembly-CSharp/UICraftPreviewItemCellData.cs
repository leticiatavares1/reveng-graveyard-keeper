using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UICraftPreviewItemCellData : LazyWidgetDataBase
{
	public Action<CraftDef, List<NeedItemData>, CraftParamsData, int> OnQueueAdded;

	public Action<CraftDef, List<NeedItemData>, CraftParamsData, int> OnCraftStarted;

	public CraftDef CraftDef { get; private set; }

	public WgoData WgoData { get; private set; }

	public bool IsUnknown
	{
		get
		{
			if (CraftDef != null && CraftDef.isNeedsUnlock)
			{
				return !MainGame.Instance.GameSave.knowledgeSystem.unlockedCrafts.Contains(CraftDef.id);
			}
			return false;
		}
	}

	public bool IsTab { get; private set; }

	public bool CanStart => CraftDef.CanActuallyStartCraft(WgoData);

	public string TabId { get; private set; }

	public string ExtensionId { get; private set; }

	public bool IsExtensionAvailable { get; private set; }

	public bool IsExtension { get; private set; }

	public bool UseTabAsIcon { get; private set; }

	public bool IsGravePartRemove { get; private set; }

	public Sprite CustomImageBackSprite { get; private set; }

	public UICraftPreviewItemCellData(CraftDef craftDef, WgoData wgoData, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onQueueAdded, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onCraftStarted, string tabId = "", bool isGravePartRemove = false, Sprite customImageBackSprite = null)
	{
		CraftDef = craftDef;
		WgoData = wgoData;
		OnQueueAdded = onQueueAdded;
		OnCraftStarted = onCraftStarted;
		TabId = tabId;
		IsGravePartRemove = isGravePartRemove;
		IsTab = !string.IsNullOrEmpty(tabId);
		CustomImageBackSprite = customImageBackSprite;
	}

	public UICraftPreviewItemCellData(string tabId, string extensionId, bool isExtensionAvailable, bool useTabAsIcon, bool isGravePartRemove = false, Sprite customImageBackSprite = null)
	{
		CraftDef = null;
		WgoData = null;
		OnQueueAdded = null;
		OnCraftStarted = null;
		TabId = tabId;
		ExtensionId = extensionId;
		IsTab = true;
		IsExtensionAvailable = isExtensionAvailable;
		IsExtension = true;
		UseTabAsIcon = useTabAsIcon;
		IsGravePartRemove = isGravePartRemove;
		CustomImageBackSprite = customImageBackSprite;
	}
}
