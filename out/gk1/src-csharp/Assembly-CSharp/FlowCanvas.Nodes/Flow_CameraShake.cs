using Com.LuisPedroFonseca.ProCamera2D;
using DarkTonic.MasterAudio;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Camera Shake", 0)]
[Category("Game Actions")]
[Description("Camera Shake")]
public class Flow_CameraShake : MyFlowNode
{
	public string shake_preset = "shake_small";

	public float vibro_time = 0.5f;

	public float vibro_intensity = 1f;

	protected override void RegisterPorts()
	{
		ValueInput<bool> in_mute_shake_sound = AddValueInput<bool>("Mute sound?");
		FlowOutput flow_immediate = AddFlowOutput("Immediate");
		FlowOutput flow_finished = AddFlowOutput("Finished");
		AddFlowInput("In", delegate(Flow f)
		{
			bool value = in_mute_shake_sound.value;
			ProCamera2DShake shaker = MainGame.me.GetComponent<ProCamera2DShake>();
			LazyInput.Vibrate(vibro_intensity, vibro_time);
			if (!value)
			{
				MasterAudio.PlaySound("fall_boulder");
			}
			if (shaker != null)
			{
				shaker.OnShakeCompleted = delegate
				{
					shaker.OnShakeCompleted = null;
					flow_finished.Call(f);
				};
				shaker.Shake(Resources.Load<ShakePreset>("Shake/" + shake_preset));
				flow_immediate.Call(f);
			}
			else
			{
				flow_immediate.Call(f);
				flow_finished.Call(f);
			}
		});
	}
}
