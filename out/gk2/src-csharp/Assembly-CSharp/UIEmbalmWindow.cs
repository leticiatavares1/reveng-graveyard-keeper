using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIEmbalmWindow : LazyWindow<UIEmbalmWindowData>
{
	[SerializeField]
	private UIInfoWidget infoWidget;

	[SerializeField]
	private UICorpseWidget corpseWidget;

	[SerializeField]
	private BodyOrgansInventoryWidget bodyOrgansInventoryWidget;

	[SerializeField]
	private BodyPocketInventoryWidget bodyPocketInventoryWidget;

	private Action onTakeBodyButtonPressed;

	public override void Redraw()
	{
		base.Redraw();
		corpseWidget.Draw(data.CorpseWidgetData);
		bodyPocketInventoryWidget.Draw(data.BodyPocketInventoryWidgetData);
		bodyOrgansInventoryWidget.Draw(data.BodyOrgansInventoryWidgetData);
		infoWidget.Draw(data.InfoWidgetData);
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
		Wgo wgo = Wgo.Spawn(new WgoData("embalm_table_1", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId), MainGame.PlayerController.CurrentGameScene.transform);
		EmbalmInteractionHandler embalmInteractionHandler = new EmbalmInteractionHandler();
		embalmInteractionHandler.Init(wgo);
		embalmInteractionHandler.HasInteraction(MainGame.PlayerController);
		embalmInteractionHandler.Interact(MainGame.PlayerController);
	}

	[LazyUITest]
	protected void TestDrawBody()
	{
		WgoData wgoData = new WgoData("embalm_table_1", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		Wgo wgo = Wgo.Spawn(wgoData, MainGame.PlayerController.CurrentGameScene.transform);
		EmbalmInteractionHandler embalmInteractionHandler = new EmbalmInteractionHandler();
		embalmInteractionHandler.Init(wgo);
		Item item = GameBalance.Me.GetData<BodyDef>("body_0_2").GenerateItem();
		wgoData.Inventory.AddItemToInventory(item);
		embalmInteractionHandler.HasInteraction(MainGame.PlayerController);
		embalmInteractionHandler.Interact(MainGame.PlayerController);
	}

	[LazyUITest]
	protected void TestZombieSmallCollar()
	{
		WgoData wgoData = new WgoData("embalm_table_1", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		Wgo wgo = Wgo.Spawn(wgoData, MainGame.PlayerController.CurrentGameScene.transform);
		EmbalmInteractionHandler embalmInteractionHandler = new EmbalmInteractionHandler();
		embalmInteractionHandler.Init(wgo);
		Item item = GameBalance.Me.GetData<BodyDef>("body_zombie_test_5").GenerateItem();
		MainGame.ZombieSystemData.CreateZombieDrop("zombie", MainGame.PlayerData.position.Value, MainGame.PlayerData.currentGameSceneId, item);
		wgoData.Inventory.AddItemToInventory(item);
		embalmInteractionHandler.HasInteraction(MainGame.PlayerController);
		embalmInteractionHandler.Interact(MainGame.PlayerController);
	}

	[LazyUITest]
	protected void TestZombieBigCollar()
	{
		WgoData wgoData = new WgoData("embalm_table_1", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		Wgo wgo = Wgo.Spawn(wgoData, MainGame.PlayerController.CurrentGameScene.transform);
		EmbalmInteractionHandler embalmInteractionHandler = new EmbalmInteractionHandler();
		embalmInteractionHandler.Init(wgo);
		Item item = GameBalance.Me.GetData<BodyDef>("body_zombie_test_5").GenerateItem();
		MainGame.ZombieSystemData.CreateZombieDrop("zombie", MainGame.PlayerData.position.Value, MainGame.PlayerData.currentGameSceneId, item, "collar_big");
		wgoData.Inventory.AddItemToInventory(item);
		embalmInteractionHandler.HasInteraction(MainGame.PlayerController);
		embalmInteractionHandler.Interact(MainGame.PlayerController);
	}
}
