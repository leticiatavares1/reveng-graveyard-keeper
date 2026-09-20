using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Trigger Sin Shard Charge", 0)]
[Category("Game Actions")]
[Description("Triggers sin shard animation, duration: 5sec.")]
public class Flow_TriggerSinShardCharge : MyFlowNode
{
	public enum PersonType
	{
		Astrologer,
		Inquisitor,
		Snake,
		Merchant,
		Actress,
		Bishop
	}

	protected override void RegisterPorts()
	{
		ValueInput<PersonType> person_type = AddValueInput<PersonType>("Person Type");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.player.TriggerSmartAnimation("sin_shard_charge");
			MainGame.me.player.wop.GetComponent<Animator>()?.SetFloat("sin_shard_color", (float)person_type.value + 1f);
			flow_out.Call(f);
		});
	}
}
