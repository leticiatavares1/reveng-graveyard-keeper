using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Get Town Building Data For Character", 0)]
[Category("Game/Wgo")]
[Color("f47dff")]
public class Flow_GetTownBuildingData : GKCustomFlowNodeWithWgoData
{
	private ValueOutput<WgoData> tentWgoData;

	private ValueOutput<GDPointData> gdPointTent;

	private ValueOutput<GDPointData> gdPointHomeOutside;

	private ValueOutput<GDPointData> gdPointHomeInside;

	private ValueOutput<GDPointData> gdPointPlayer;

	private WgoData LinkedWgoData => MainGame.WorldData.GetWgoData(GetWgoData().LinkedToTownBuildingUniqueId);

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		tentWgoData = AddValueOutput("tentWgoData".CapitalizeFirst(), () => LinkedWgoData);
		gdPointTent = AddValueOutput("gdPointTent".CapitalizeFirst(), () => MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(LinkedWgoData.TownBuildingWgoComponent.SceneConfiguration.gdPointTent));
		gdPointHomeOutside = AddValueOutput("gdPointHomeOutside".CapitalizeFirst(), () => MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(LinkedWgoData.TownBuildingWgoComponent.SceneConfiguration.gdPointHomeOutside));
		gdPointHomeInside = AddValueOutput("gdPointHomeInside".CapitalizeFirst(), () => MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(LinkedWgoData.TownBuildingWgoComponent.SceneConfiguration.gdPointHomeInside));
		gdPointPlayer = AddValueOutput("gdPointPlayer".CapitalizeFirst(), () => MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(Flow_TownBuildingConsts.GetPlayerTpDuringFadeGdPointId(LinkedWgoData.TownBuildingWgoComponent.SceneConfiguration.gdPointTent)));
	}
}
