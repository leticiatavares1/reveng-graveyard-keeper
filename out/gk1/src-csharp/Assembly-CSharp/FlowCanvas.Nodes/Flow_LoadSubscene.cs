using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Load Subscene", 0)]
[Category("Game Actions")]
[Description("Load scene additive to the current scene")]
[Icon("CubePlus", false, "")]
public class Flow_LoadSubscene : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> par_custom_tag = AddValueInput<string>("Scene name");
		FlowOutput flow_out_on_scene_loaded = AddFlowOutput("On loaded");
		FlowOutput flow_out_on_unfaded = AddFlowOutput("On unfaded");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.ChangeBubblesVisibility(show: false);
			CameraTools.Fade(delegate
			{
				SubsceneLoadManager.Load(par_custom_tag.value, delegate
				{
					flow_out_on_scene_loaded.Call(f);
					SubsceneLoadManager.CameraFlyToLastScene(delegate
					{
						CameraTools.UnFade(delegate
						{
							GUIElements.ChangeBubblesVisibility(MainGame.me.player_char.control_enabled);
							flow_out_on_unfaded.Call(f);
						}, 2f);
					});
				});
			}, 2f);
		});
	}
}
