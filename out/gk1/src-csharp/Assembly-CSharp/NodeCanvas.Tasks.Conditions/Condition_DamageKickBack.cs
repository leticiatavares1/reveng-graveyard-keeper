using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Name("Damage Kick Back", 0)]
[Category("Player")]
public class Condition_DamageKickBack : WGOBehaviourCondition
{
	public BBParameter<float> speed = new BBParameter<float>(4f);

	public BBParameter<float> friction = new BBParameter<float>(0.9f);

	public BBParameter<float> sleep_time = new BBParameter<float>(0f);

	private float left_sleep_time;

	protected override bool OnCheck()
	{
		KickComponent kick = base.self_wgo.components.kick;
		bool flag = base.self_ch.WasDamaged(clear_flag: true);
		if (kick == null || (!flag && !kick.in_process && left_sleep_time < 0f))
		{
			return false;
		}
		if (flag)
		{
			if (!base.self_ch.IsStopped)
			{
				base.self_ch.StopMovement();
			}
			if (base.self_ch.components.character.attack.enabled && base.self_ch.components.character.attack.performing_attack)
			{
				base.self_ch.InterruptAttack();
			}
			kick.KickFrom(base.player_wgo.pos).SetSpeed(speed.value).SetFriction(friction.value);
			left_sleep_time = sleep_time.value;
		}
		if (!flag && !kick.in_process)
		{
			left_sleep_time -= Time.deltaTime;
		}
		return true;
	}
}
