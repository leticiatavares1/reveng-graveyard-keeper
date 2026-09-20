using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Description("Do Damage To WGO")]
[Name("Do Damage To WGO", 0)]
public class Flow_DoDamageToWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<float> in_damage = AddValueInput<float>("damage");
		ValueInput<Direction> in_direction = AddValueInput<Direction>("damaged from");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_damage.value < 1f)
			{
				Debug.LogError("Wrong damage!");
			}
			else
			{
				WorldGameObject worldGameObject = ((in_wgo.value != null) ? in_wgo.value : MainGame.me.player);
				if (!worldGameObject.is_dead && worldGameObject.components.hp.enabled)
				{
					worldGameObject.components.hp.DecHP(in_damage.value);
					float num = 0f;
					switch (in_direction.value)
					{
					case Direction.Right:
						num = 0f;
						break;
					case Direction.Up:
						num = 90f;
						break;
					case Direction.Left:
						num = 180f;
						break;
					case Direction.Down:
						num = -90f;
						break;
					case Direction.None:
					case Direction.IgnoreDirection:
					case Direction.ToPlayer:
						num = -90f;
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
					if (worldGameObject.components.character.enabled)
					{
						worldGameObject.components.character.OnWasDamaged(num);
					}
					flow_out.Call(f);
				}
			}
		});
	}
}
