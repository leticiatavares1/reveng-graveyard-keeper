using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Body Roll", 0)]
[Category("Game")]
public class Flow_Body : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput success;

	private FlowOutput fail;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), DoAction);
		success = AddFlowOutput("success".CapitalizeFirst());
		fail = AddFlowOutput("fail".CapitalizeFirst());
	}

	private void DoAction(Flow flow)
	{
		Debug.Log("Body Roll");
		int resInt = MainGame.PlayerData.GetResInt("cur_bodies_count");
		int num = (int)MainGame.WorldData.GetWorldZoneDataById("morgue").GetTotalQuality();
		if (resInt < num)
		{
			if (MainGame.PlayerData.GetRes("donkey_body_drop_chance") >= (float)Random.Range(0, 100))
			{
				MainGame.PlayerData.AddRes("donkey_body_drop_chance", ChanceReduceAfterSuccess());
				Debug.Log("Started body drop");
				success.Call(flow);
			}
			else
			{
				MainGame.PlayerData.AddRes("donkey_body_drop_chance", ChanceIncreaseAfterFail());
				Debug.Log("Skipped body drop, because roll failed");
				fail.Call(flow);
			}
		}
		else
		{
			MainGame.PlayerData.AddRes("donkey_body_drop_chance", ChanceIncreaseAfterSkip());
			Debug.Log("Skipped body drop, because morgue is full");
			fail.Call(flow);
		}
	}

	private float ChanceReduceAfterSuccess()
	{
		if (MainGame.PlayerData.GetResInt("nun_many_body_drop") == 1)
		{
			return ConstDef.Get("nun_many_drop_chance_reduce_after_success").FloatValue * -1f;
		}
		return ConstDef.Get("donkey_drop_chance_reduce_after_success").FloatValue * -1f;
	}

	private float ChanceIncreaseAfterFail()
	{
		if (MainGame.PlayerData.GetResInt("nun_many_body_drop") == 1)
		{
			return ConstDef.Get("nun_many_drop_chance_increase_after_fail").FloatValue;
		}
		return ConstDef.Get("donkey_drop_chance_increase_after_fail").FloatValue;
	}

	private float ChanceIncreaseAfterSkip()
	{
		if (MainGame.PlayerData.GetResInt("nun_many_body_drop") == 1)
		{
			return ConstDef.Get("nun_many_drop_chance_increase_after_skip").FloatValue;
		}
		return ConstDef.Get("donkey_drop_chance_increase_after_skip").FloatValue;
	}
}
