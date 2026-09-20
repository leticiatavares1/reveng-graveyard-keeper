using System.Collections.Generic;
using LazyBearTechnology;

public class TownBuildingPlaceInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		UITownBuildingWindow buildWindow = LazyUI.GetWindow<UITownBuildingWindow>();
		List<TownBuildingDef> availableBuildings = assignedWgo.Data.TownBuildingWgoComponent.GetAvailableBuildings(assignedWgo.Data);
		if (availableBuildings.Count == 1 && availableBuildings[0].townBuildingType == TownBuildingType.TownRepair && availableBuildings[0].upgradeRequirements.Count > 0)
		{
			TownBuildingDef def2 = availableBuildings[0];
			List<AnswerVisualData> answers = new List<AnswerVisualData>
			{
				new AnswerVisualData
				{
					answerData = new AnswerData
					{
						lockRes = GetUpgradeRequirementsLockRes(def2)
					},
					hiddenByDefault = false,
					id = "hint_build"
				},
				new AnswerVisualData
				{
					answerData = new AnswerData(),
					hiddenByDefault = false,
					id = "common_leave"
				}
			};
			MainGame.PlayerController.SetControlTakenType(TakenControlType.ByFlow, isEnabled: false);
			Bubble.ShowMultiAnswer(answers, MainGame.PlayerController.BubblePoint, assignedWgo.Data, delegate(string chosen)
			{
				MainGame.PlayerController.SetControlTakenType(TakenControlType.ByFlow, isEnabled: true);
				if (chosen == "hint_build")
				{
					OpenBuildWindow();
				}
			}, delegate
			{
			});
			return true;
		}
		OpenBuildWindow();
		return true;
		void OnBuildPressed(TownBuildingDef def, List<NeedItemData> selectedNeedItems)
		{
			MultiInventory multiInventory = new MultiInventory(MainGame.PlayerController.PlayerData);
			if (multiInventory.HasItemsById(selectedNeedItems))
			{
				buildWindow.Close();
				UIFade fade = LazyUI.Get<UIFade>();
				float fadeTime = 0.6f;
				MainGame.PlayerController.SetControlTakenType(TakenControlType.ByFlow, isEnabled: false);
				fade.FadeIn(fadeTime, delegate
				{
					string craftId = "town_building_craft:" + def.id;
					CraftElement craftElement = new CraftElement(craftId, 1, new CraftParamsData(craftId, assignedWgo.Data));
					multiInventory.RemoveItems(selectedNeedItems);
					assignedWgo.Data.CraftComponent.LastStartedCraftWithRequirements = craftElement;
					assignedWgo.Data.CraftComponent.ProcessInstantCraft(assignedWgo.Data, craftElement);
					LazyAudio.PlayAndForget("town_repair");
					LazyTimer.AddTimer(2.5f, delegate
					{
						fade.FadeOut(fadeTime, delegate
						{
							MainGame.PlayerController.SetControlTakenType(TakenControlType.ByFlow, isEnabled: true);
						});
					});
				});
			}
		}
		void OpenBuildWindow()
		{
			UITownBuildingWindowData data = new UITownBuildingWindowData(GameScene.GetWgoViewGlobal(assignedWgo.Data.UniqueId), MainGame.PlayerController.PlayerData, availableBuildings, OnBuildPressed);
			buildWindow.Open(data);
		}
	}

	private SmartRes GetUpgradeRequirementsLockRes(TownBuildingDef def)
	{
		SmartRes smartRes = new SmartRes();
		smartRes.gameRes = new GameRes();
		foreach (ExpressionGameRes upgradeRequirement in def.upgradeRequirements)
		{
			smartRes.gameRes.Add(upgradeRequirement.name, upgradeRequirement.expression.EvaluateFloat());
		}
		return smartRes;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		return !assignedWgo.Data.CraftComponent.IsStarted;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		string hint;
		return new InteractionInfos(new InteractionInfo(TryGetCustomInteractionStr(out hint) ? hint : LocalizeHintWithActionIcon("hint_build", GameKey.Interaction)));
	}
}
