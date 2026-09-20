using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAutopsyWindow : LazyWindow<UIAutopsyWindowData>
{
	[SerializeField]
	private UIInfoWidget infoWidget;

	[SerializeField]
	private UICorpseWidget corpseWidget;

	[SerializeField]
	private BodyOrgansInventoryWidget bodyOrgansInventoryWidget;

	[SerializeField]
	private BodyPocketInventoryWidget bodyPocketInventoryWidget;

	[SerializeField]
	private TMP_Text collarSkullsTxt;

	private Action onTakeBodyButtonPressed;

	public override void Redraw()
	{
		base.Redraw();
		corpseWidget.Draw(data.CorpseWidgetData);
		bodyPocketInventoryWidget.Draw(data.BodyPocketInventoryWidgetData);
		bodyOrgansInventoryWidget.Draw(data.BodyOrgansInventoryWidgetData);
		infoWidget.Draw(data.InfoWidgetData);
		UpdateCollarSkullsTxt();
		if (LazyInput.IsGamepadActive)
		{
			if (data.IsEmpty)
			{
				base.GamepadNavigationController.Disable();
			}
			else
			{
				base.GamepadNavigationController.Enable();
				base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			}
		}
		((RectTransform)base.transform).RefreshContentFitter();
	}

	private void UpdateCollarSkullsTxt()
	{
		if (!(collarSkullsTxt == null))
		{
			collarSkullsTxt.text = GetCollarSkullsText();
		}
	}

	private string GetCollarSkullsText()
	{
		if (data == null || data.IsEmpty || data.CorpseWidgetData == null || !data.CorpseWidgetData.IsZombie)
		{
			return string.Empty;
		}
		Item item = data.CorpseWidgetData.ZombieWgoData?.Collar;
		if (item == null || item.IsEmpty)
		{
			return string.Empty;
		}
		ItemDef definition = item.Definition;
		if (definition.redSkullsMaxCollar <= definition.redSkullsMinCollar)
		{
			return string.Empty;
		}
		string text = string.Format("{0}{1}-{2}", "rskull".FontIcon(), definition.redSkullsMinCollar, definition.redSkullsMaxCollar);
		return LLBase.L("zombie_collar") + LLBase.L("colon_symbol") + " " + text;
	}

	public override void Hide()
	{
		base.Hide();
		if (data != null)
		{
			corpseWidget.Hide();
			bodyPocketInventoryWidget.Hide();
			bodyOrgansInventoryWidget.Hide();
		}
	}

	private bool OnTakeBodyButtonPressed()
	{
		corpseWidget.OnButtonPressed();
		return true;
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.ExtractBody, OnTakeBodyButtonPressed);
		return gameKeyDelegates;
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Wgo wgo = Wgo.Spawn(new WgoData("autopsy_table_1", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId), MainGame.PlayerController.CurrentGameScene.transform);
		AutopsyInteractionHandler autopsyInteractionHandler = new AutopsyInteractionHandler();
		autopsyInteractionHandler.Init(wgo);
		autopsyInteractionHandler.HasInteraction(MainGame.PlayerController);
		autopsyInteractionHandler.Interact(MainGame.PlayerController);
	}

	[LazyUITest]
	protected void TestDrawBody()
	{
		WgoData wgoData = new WgoData("autopsy_table_1", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		Wgo wgo = Wgo.Spawn(wgoData, MainGame.PlayerController.CurrentGameScene.transform);
		AutopsyInteractionHandler autopsyInteractionHandler = new AutopsyInteractionHandler();
		autopsyInteractionHandler.Init(wgo);
		Item item = GameBalance.Me.GetData<BodyDef>("body_0_2").GenerateItem();
		wgoData.Inventory.AddItemToInventory(item);
		autopsyInteractionHandler.HasInteraction(MainGame.PlayerController);
		autopsyInteractionHandler.Interact(MainGame.PlayerController);
	}

	[LazyUITest]
	protected void TestZombieSmallCollar()
	{
		WgoData wgoData = new WgoData("autopsy_table_1", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		Wgo wgo = Wgo.Spawn(wgoData, MainGame.PlayerController.CurrentGameScene.transform);
		AutopsyInteractionHandler autopsyInteractionHandler = new AutopsyInteractionHandler();
		autopsyInteractionHandler.Init(wgo);
		Item item = GameBalance.Me.GetData<BodyDef>("body_zombie_test_5").GenerateItem();
		MainGame.ZombieSystemData.CreateZombieDrop("zombie", MainGame.PlayerData.position.Value, MainGame.PlayerData.currentGameSceneId, item);
		wgoData.Inventory.AddItemToInventory(item);
		autopsyInteractionHandler.HasInteraction(MainGame.PlayerController);
		autopsyInteractionHandler.Interact(MainGame.PlayerController);
	}

	[LazyUITest]
	protected void TestZombieBigCollar()
	{
		WgoData wgoData = new WgoData("autopsy_table_1", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		Wgo wgo = Wgo.Spawn(wgoData, MainGame.PlayerController.CurrentGameScene.transform);
		AutopsyInteractionHandler autopsyInteractionHandler = new AutopsyInteractionHandler();
		autopsyInteractionHandler.Init(wgo);
		Item item = GameBalance.Me.GetData<BodyDef>("body_zombie_test_5").GenerateItem();
		MainGame.ZombieSystemData.CreateZombieDrop("zombie", MainGame.PlayerData.position.Value, MainGame.PlayerData.currentGameSceneId, item, "collar_big");
		wgoData.Inventory.AddItemToInventory(item);
		autopsyInteractionHandler.HasInteraction(MainGame.PlayerController);
		autopsyInteractionHandler.Interact(MainGame.PlayerController);
	}
}
