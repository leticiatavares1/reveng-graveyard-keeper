using System.Collections.Generic;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Show Town Building Window", 0)]
[Category("Game/UI")]
public class Flow_OpenTownBuildingWindow : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onBuildPressed;

	public bool isForLevelUp;

	private WgoData buildOnWgoData;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Show);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onBuildPressed = AddFlowOutput("onBuildPressed".CapitalizeFirst());
	}

	private void Show(Flow flow)
	{
		buildOnWgoData = (isForLevelUp ? MainGame.WorldData.GetWgoData(base.SelfWgoData.LinkedToTownBuildingUniqueId) : base.SelfWgoData);
		UITownBuildingWindow buildWindow = LazyUI.GetWindow<UITownBuildingWindow>();
		UITownBuildingWindowData data = new UITownBuildingWindowData(GameScene.GetWgoViewGlobal(buildOnWgoData.UniqueId), MainGame.PlayerController.PlayerData, isForLevelUp ? new List<TownBuildingDef> { GameBalance.Me.GetData<TownBuildingDef>(buildOnWgoData.TownBuildingWgoComponent.TownBuildingDef.lvlUpId) } : buildOnWgoData.TownBuildingWgoComponent.GetAvailableBuildings(buildOnWgoData), OnBuildPressed);
		buildWindow.Open(data);
		@out.Call(flow);
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
					CraftElement craftElement = new CraftElement(craftId, 1, new CraftParamsData(craftId, buildOnWgoData));
					multiInventory.RemoveItems(selectedNeedItems);
					buildOnWgoData.CraftComponent.LastStartedCraftWithRequirements = craftElement;
					buildOnWgoData.CraftComponent.ProcessInstantCraft(buildOnWgoData, craftElement);
					buildWindow.Close();
					onBuildPressed.Call(flow);
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
	}
}
