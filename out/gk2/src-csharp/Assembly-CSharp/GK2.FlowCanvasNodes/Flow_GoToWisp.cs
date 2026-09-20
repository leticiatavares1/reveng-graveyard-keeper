using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("GoTo Wisp", 0)]
[Category("Game/Environment")]
public class Flow_GoToWisp : Flow_GoTo
{
	private ValueInput<WispController> wispInput;

	public override string name => "GoTo Wisp";

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		wispInput = AddValueInput<WispController>("wispInput".CapitalizeFirst());
		wgoDataInput = null;
	}

	protected override void GoTo(Flow flow)
	{
		WispController wispController = wispInput.value;
		if (wispController == null)
		{
			wispController = MainGame.PlayerController.WispController;
		}
		WgoData wispWgoData = wispController.GetWispWgoData();
		GDPointData gDPointData = ((!useGdDataInsteadId) ? MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(gdPointId.value) : gdPointData.value);
		if (wispWgoData.WorldId != gDPointData.GameSceneDataId)
		{
			MainGame.Instance.GameSave.worldData.MoveWgoDataToAnotherGameScene(wispWgoData, gDPointData.GameSceneDataId);
		}
		wispWgoData.MovementComponent.StartPath(gDPointData.Position, wispWgoData.WorldId, gDPointData.GameSceneDataId, navigation.value, speed.value, fireEventOnFinish.value, delegate
		{
			onFinish.Call(flow);
		});
		wispController.ChangeTargetType(WispTargetType.WgoData);
		wispController.SetTargetWgoData(wispWgoData);
		@out.Call(flow);
	}
}
