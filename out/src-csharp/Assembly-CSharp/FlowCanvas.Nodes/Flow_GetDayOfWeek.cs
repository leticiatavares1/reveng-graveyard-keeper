using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Description("Day of Week")]
[Name("Day of Week", 0)]
public class Flow_GetDayOfWeek : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_error = AddFlowOutput("Error");
		FlowOutput flow_Sloth = AddFlowOutput("Sloth - Astrologer");
		FlowOutput flow_Wrath = AddFlowOutput("Wrath - Inquisitor");
		FlowOutput flow_Envy = AddFlowOutput("Envy - Cultist");
		FlowOutput flow_Gluttony = AddFlowOutput("Gluttony - Merchant");
		FlowOutput flow_Lust = AddFlowOutput("Lust - Actress");
		FlowOutput flow_Pride = AddFlowOutput("Pride - Bishop");
		Sins.SinType out_sin = Sins.SinType.Greed;
		AddValueOutput("sin type", () => out_sin);
		AddFlowInput("In", delegate(Flow f)
		{
			out_sin = (Sins.SinType)((12 - MainGame.me.save.day_of_week) % 6 + 1);
			switch (out_sin)
			{
			case Sins.SinType.Greed:
				flow_error.Call(f);
				break;
			case Sins.SinType.Sloth:
				flow_Sloth.Call(f);
				break;
			case Sins.SinType.Wrath:
				flow_Wrath.Call(f);
				break;
			case Sins.SinType.Envy:
				flow_Envy.Call(f);
				break;
			case Sins.SinType.Gluttony:
				flow_Gluttony.Call(f);
				break;
			case Sins.SinType.Lust:
				flow_Lust.Call(f);
				break;
			case Sins.SinType.Pride:
				flow_Pride.Call(f);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		});
	}
}
