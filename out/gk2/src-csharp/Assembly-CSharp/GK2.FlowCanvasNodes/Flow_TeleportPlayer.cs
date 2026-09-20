using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Teleport Player", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_TeleportPlayer : GKCustomFlowNode
{
	public enum TeleportDestination
	{
		GDPoint,
		Wgo,
		DockPoint
	}

	[GatherPortsCallback]
	public TeleportDestination teleportDestination;

	[GatherPortsCallback]
	public bool isNeedApplyPreset;

	[GatherPortsCallback]
	public bool doFade;

	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onSceneLoaded;

	private ValueInput<GDPointData> gdPointData;

	private ValueInput<WgoData> wgoData;

	private ValueInput<string> presetNameToApply;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Teleport);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onSceneLoaded = AddFlowOutput("onSceneLoaded".CapitalizeFirst());
		if (isNeedApplyPreset)
		{
			presetNameToApply = AddValueInput<string>("presetNameToApply");
		}
		switch (teleportDestination)
		{
		case TeleportDestination.GDPoint:
			gdPointData = AddValueInput<GDPointData>("GDPointData");
			break;
		case TeleportDestination.Wgo:
		case TeleportDestination.DockPoint:
			wgoData = AddValueInput<WgoData>("WgoData");
			break;
		}
	}

	private void Teleport(Flow flow)
	{
		MainGame.PlayerController.SetControlTakenType(TakenControlType.ByTeleport, isEnabled: false);
		string environmentPreset = (isNeedApplyPreset ? presetNameToApply.value : string.Empty);
		switch (teleportDestination)
		{
		case TeleportDestination.GDPoint:
		{
			GDPointData value = gdPointData.value;
			if (value != null)
			{
				PlayerController.Teleport(new GDPointTeleportData(value, environmentPreset, "", delegate
				{
					onSceneLoaded.Call(flow);
				}, !doFade));
			}
			break;
		}
		case TeleportDestination.Wgo:
		{
			WgoData value2 = this.wgoData.value;
			if (value2 != null)
			{
				PlayerController.Teleport(new WgoTeleportData(value2.id, environmentPreset, "", teleportToDockPoint: false, delegate
				{
					onSceneLoaded.Call(flow);
				}, !doFade));
			}
			break;
		}
		case TeleportDestination.DockPoint:
		{
			WgoData wgoData = WgoDataParamOrSelf(this.wgoData);
			if (wgoData != null)
			{
				PlayerController.Teleport(new WgoTeleportData(wgoData.id, environmentPreset, "", teleportToDockPoint: true, delegate
				{
					onSceneLoaded.Call(flow);
				}, !doFade));
			}
			break;
		}
		}
		@out.Call(flow);
	}
}
