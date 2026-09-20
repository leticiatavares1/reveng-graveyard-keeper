using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Get Corpse Drop Amount", 0)]
[Category("Game")]
[Color("FFFFFF")]
public class Flow_GetCorpseDropAmount : GKCustomFlowNode
{
	private ValueOutput<int> amount;

	protected override void RegisterPorts()
	{
		amount = AddValueOutput("amount".CapitalizeFirst(), GetAmount);
	}

	private int GetAmount()
	{
		int num = 1;
		if (MainGame.PlayerData.GetRes("nun_many_body_drop") > 0f)
		{
			ConstDef constDef = ConstDef.Get("donkey_drop_additional");
			if (constDef != null)
			{
				num += ((constDef.IntValue != 0) ? constDef.IntValue : Mathf.RoundToInt(constDef.FloatValue));
			}
		}
		int resInt = MainGame.PlayerData.GetResInt("cur_bodies_count");
		int num2 = (int)MainGame.WorldData.GetWorldZoneDataById("morgue").GetTotalQuality();
		int b = Mathf.Max(1, num2 - resInt);
		return Mathf.Min(num, b);
	}
}
