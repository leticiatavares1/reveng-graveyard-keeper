using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Removes last additive added subscene")]
[Category("Game Actions")]
[Icon("CubeArrowStraight", false, "")]
[Name("Unload Last Subscene", 0)]
public class Flow_UnloadLastSubscene : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out_fade = AddFlowOutput("On fade");
		FlowOutput flow_out = AddFlowOutput("On unload started");
		FlowOutput flow_out_unfade = AddFlowOutput("On unfaded");
		ValueInput<GameObject> game_object_to_restore = AddValueInput<GameObject>("Camera pos");
		ValueInput<bool> restore_camera_to_player = AddValueInput<bool>("Camera to player");
		AddFlowInput("In", delegate(Flow f)
		{
			GameObject value = game_object_to_restore.value;
			bool flag_restore_camera_to_player = restore_camera_to_player.value;
			float unfade_camera_time = 1f;
			if (value != null)
			{
				Transform target_transform = value.transform;
				if (target_transform != null)
				{
					CameraTools.Fade(delegate
					{
						flow_out_fade.Call(f);
						if (!flag_restore_camera_to_player)
						{
							CameraTools.CameraFlyTo(target_transform, delegate
							{
								CameraTools.UnFade(delegate
								{
									flow_out_unfade.Call(f);
								}, unfade_camera_time);
							});
						}
						else
						{
							CameraTools.CameraFlyBack(delegate
							{
								MainGame.me.player.GetComponent<ChunkedGameObject>().active_now_because_of_events = true;
								CameraTools.UnFade(delegate
								{
									flow_out_unfade.Call(f);
								}, unfade_camera_time);
							}, 0f);
						}
						SubsceneLoadManager.UnloadLastScene();
						flow_out.Call(f);
					}, 2f);
				}
				else
				{
					Debug.LogError("Target transform is NULL");
					flow_out_unfade.Call(f);
					flow_out.Call(f);
				}
			}
			else
			{
				Debug.LogError("Target GO is NULL");
				flow_out_fade.Call(f);
				flow_out_unfade.Call(f);
				flow_out.Call(f);
			}
		});
	}
}
