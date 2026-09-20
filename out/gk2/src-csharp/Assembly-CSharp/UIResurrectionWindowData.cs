using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIResurrectionWindowData : LazyWidgetDataBase
{
	public readonly Action onPrepareResurrectionButtonPressed;

	public Action onCollarUpdated;

	public Action<LazyButton> onPrepareResurrectionNonInteractableButtonOver;

	private Item bodyItem;

	private CraftElement craftElementBodyToZombie;

	private MultiInventory currentMultiInventory;

	private string cantStartResurrectionReason;

	public bool IsEmpty { get; private set; }

	public Func<bool> CanStartResurrection { get; private set; }

	public UICorpseWidgetData CorpseWidgetData { get; private set; }

	public BodyOrgansInventoryWidgetData BodyOrgansInventoryWidgetData { get; private set; }

	public NeedItemsWidgetData NeedItemsWidgetData { get; private set; }

	public WgoData Wgo { get; private set; }

	public UIInfoWidgetData InfoWidgetData { get; set; }

	public string ZombieName { get; set; }

	public Item Collar { get; set; } = Item.Empty;


	public int RedSkulls { get; private set; }

	public int WhiteSkulls { get; private set; }

	public (int body, int head, string headLut) RolledSkin { get; private set; }

	public UIResurrectionWindowData(WgoData wgoData)
	{
		wgoData.TrySetWorker(MainGame.PlayerController);
		InfoWidgetData = new UIInfoWidgetData(wgoData);
		Wgo = wgoData;
		foreach (Item item in wgoData.Inventory.Data.Inventory)
		{
			if (item.Definition.itemGroupIds.Contains("body"))
			{
				bodyItem = item;
				break;
			}
		}
		CraftDef craftDef = GameBalance.GetCraftDef("corpse_zombie_transition");
		craftElementBodyToZombie = new CraftElement(craftDef.id, 1, new List<NeedItemData>(craftDef.needItems), new CraftParamsData(craftDef.id, new GameRes()));
		InfoWidgetData.Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(craftDef.GetCraftResultIcon(wgoData));
		IsEmpty = bodyItem == null;
		if (!IsEmpty)
		{
			if (bodyItem.TryGetProperty<BodyZombieSkinSerializedItemProperty>(out var property))
			{
				RolledSkin = (body: property.body, head: property.head, headLut: property.headLut);
			}
			else
			{
				Debug.LogError(" Skin was not generated for body:[" + bodyItem.id + "]. Use default");
				RolledSkin = (body: 1001, head: 1050, headLut: "hed_lut_01");
			}
		}
		BodyOrgansInventoryWidgetData = new BodyOrgansInventoryWidgetData();
		if (!IsEmpty)
		{
			CorpseWidgetData = new UICorpseWidgetData(bodyItem, wgoData, TakeBody, !IsEmpty, GameKey.ExtractBody, LLBase.L("ui_grave_corpse_widget_header"), null, LLBase.L("btn_take_body_two_lines"));
			Inventory bodyItemInventory = new Inventory(bodyItem);
			BodyOrgansInventoryWidgetData = new BodyOrgansInventoryWidgetData(isActive: true, wgoData, null, bodyItemInventory, null, null);
			onPrepareResurrectionButtonPressed = PrepareResurrection;
			onPrepareResurrectionNonInteractableButtonOver = ShowResurrectionPrepareButtonTooltip;
			CanStartResurrection = () => CanStartResurrectionWithReason(out cantStartResurrectionReason);
			NeedItemsWidgetData = new NeedItemsWidgetData(craftElementBodyToZombie.Requirements, Wgo.GetCraftableMultiInventory(), isActive: true, Wgo);
			ZombieName = MainGame.Instance.GameSave.knowledgeSystem.GetZombieName();
			foreach (Item item2 in bodyItem.Inventory)
			{
				RedSkulls += item2.Definition.redSkulls * item2.Count;
				WhiteSkulls += item2.Definition.whiteSkulls * item2.Count;
			}
			RedSkulls = Mathf.Clamp(RedSkulls, 0, 999);
			WhiteSkulls = Mathf.Clamp(WhiteSkulls, 0, 999);
		}
		else
		{
			CorpseWidgetData = new UICorpseWidgetData(GameKey.ExtractBody);
			NeedItemsWidgetData = new NeedItemsWidgetData(craftElementBodyToZombie.Requirements, Wgo.GetCraftableMultiInventory(), isActive: false, Wgo);
			BodyOrgansInventoryWidgetData = new BodyOrgansInventoryWidgetData();
			onPrepareResurrectionButtonPressed = null;
			CanStartResurrection = () => false;
			onPrepareResurrectionNonInteractableButtonOver = null;
		}
	}

	private void TakeBody()
	{
		if (!IsEmpty)
		{
			PlayerData playerData = MainGame.PlayerData;
			if (!playerData.HasFreeOverheadSlot)
			{
				MainGame.Instance.dropSystem.DropItem(bodyItem, Wgo.WorldId, Wgo.Position);
			}
			else
			{
				playerData.AddOverheadItem(bodyItem);
			}
			Wgo.Inventory.RemoveItemFromInventoryByUID(bodyItem);
			LazyUI.GetWindow<UIResurrectionWindow>().Close();
		}
	}

	public void OnCollarPressed(UIItemCell cell)
	{
		if (!IsEmpty)
		{
			UIMultiInventoryWindow window = LazyUI.GetWindow<UIMultiInventoryWindow>();
			UIMultiInventoryWindowData uIMultiInventoryWindowData = new UIMultiInventoryWindowData(MainGame.PlayerData, delegate(UIItemCell cell)
			{
				Collar = cell.DisplayingItem;
				LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
				onCollarUpdated?.Invoke();
			}, (Item item) => item != null && !item.IsEmpty && item.Definition.type == ItemType.Collar && item.Definition.SkullsInBorders(WhiteSkulls, RedSkulls));
			window.Open(uIMultiInventoryWindowData);
			currentMultiInventory = uIMultiInventoryWindowData.MultiInventory;
		}
	}

	private bool CanStartResurrectionWithReason(out string reason)
	{
		reason = "";
		foreach (Item item in Wgo.Inventory.Data.Inventory)
		{
			if (item.Definition.itemGroupIds.Contains("zombie"))
			{
				return false;
			}
		}
		if (MainGame.PlayerData.GetResInt("zombies_limit_mechanic") > 0 && MainGame.PlayerData.GetResInt("cur_zombies_count") >= (int)MainGame.WorldData.GetWorldZoneDataById("resurrection").GetTotalQuality())
		{
			reason = "ui_resurrection_no_limit";
			return false;
		}
		if (!BodyOrgansInventoryWidgetData.HasAllMainOrgans())
		{
			reason = "ui_resurrection_no_organs";
			return false;
		}
		if (!Wgo.GetCraftableMultiInventory().HasItemsById(craftElementBodyToZombie.Requirements, Wgo))
		{
			reason = "ui_resurrection_no_liquid";
			return false;
		}
		if (Collar.IsEmpty)
		{
			reason = "ui_resurrection_no_collar";
			return false;
		}
		if (!Collar.Definition.SkullsInBorders(WhiteSkulls, RedSkulls))
		{
			reason = "ui_resurrection_insufficient_collar";
			return false;
		}
		return true;
	}

	private void ShowResurrectionPrepareButtonTooltip(LazyButton prepareButton)
	{
		UITooltip.ShowResurrectionPrepareButtonWidget(prepareButton, cantStartResurrectionReason);
	}

	private void PrepareResurrection()
	{
		if (!IsEmpty)
		{
			if (Collar == null || Collar.IsEmpty)
			{
				Collar = new Item("collar_bronze");
			}
			Item item = new Item((bodyItem.id == "body_corpse") ? "body_zombie" : "body_corpse");
			if (bodyItem.TryGetProperty<BodyZombieStartItemsSerializedItemProperty>(out var property))
			{
				item.AddProperty(property);
			}
			if (currentMultiInventory != null)
			{
				currentMultiInventory.RemoveItemFromInventoryByUID(Collar, 1);
			}
			Wgo.GetCraftableMultiInventory().RemoveItems(craftElementBodyToZombie.Requirements, Wgo);
			craftElementBodyToZombie.Requirements.Clear();
			for (int i = 0; i < bodyItem.Inventory.Count; i++)
			{
				item.AddItemToInventory(bodyItem.Inventory[i]);
			}
			Wgo.Inventory.RemoveItemFromInventoryByUID(bodyItem);
			Wgo.Inventory.AddItemToInventory(item);
			ZombieWgoData zombieWgoData = MainGame.ZombieSystemData.CreateZombieDrop("zombie", MainGame.PlayerData.position.Value, MainGame.PlayerData.currentGameSceneId, Wgo.Inventory.GetItemByGroupId("body"), Collar.id, null, rollSkin: false);
			ZombieSkinHelper.ApplySkinToZombieWgoData(zombieWgoData, RolledSkin.body, RolledSkin.head, string.Empty, RolledSkin.headLut);
			zombieWgoData.Name = ZombieName;
			ZombieName = string.Empty;
			Wgo.SetGameRes("resurrection_prepared", 1);
			LazyUI.GetWindow<UIResurrectionWindow>().Close();
		}
	}
}
