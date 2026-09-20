using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Description("Player stepped on shit")]
[Name("Player on shit", 0)]
public class PlayerSteppedOnShit : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<float> in_speed = AddValueInput<float>("speed");
		ValueInput<float> in_acceleration = AddValueInput<float>("acceleration");
		ValueInput<float> in_friction = AddValueInput<float>("friction");
		ValueInput<float> in_dec_speed = AddValueInput<float>("decrease speed");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.player.components.character.SetMovementModifiers(new MovementComponent.Modifier(-0.98f + in_acceleration.value, in_dec_speed.value), new MovementComponent.Modifier(0.38f + in_friction.value, in_dec_speed.value), new MovementComponent.Modifier(2f, in_dec_speed.value), new MovementComponent.Modifier(in_speed.value, in_dec_speed.value));
			MainGame.me.player_component.ForceTrailChange(Ground.GroudType.Shit);
			flow_out.Call(f);
		});
	}
}
