using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class PerkDef : BalanceBaseObject
{
	[AutoParse("icon")]
	[SerializeField]
	private string customIcon;

	[AutoParse("perk_type")]
	public PerkType perkType;

	[AutoParse("energy_add")]
	public float energyAdd;

	[AutoParse("insanity_add")]
	public float insanityAdd;

	[AutoParse("craft_done_hits")]
	public int craftStartTicks;

	[AutoParse("craft_add_duration")]
	public int craftTotalProgressTicksBonus;

	[AutoParse("craft_add_talent")]
	public int craftMasteryBonus;

	[AutoParse("duration")]
	public float duration;

	[AutoParse("has_hidden_timer")]
	public bool hiddenTimer;

	[AutoParse("perk_add_type")]
	public PerkAddType perkAddType;

	[AutoParse("is_hidden")]
	public bool isHidden;

	[AutoParse("set_res_on_add")]
	public GameRes setGameResOnAdd;

	[AutoParse("add_res_on_add")]
	public GameRes addGameResOnAdd;

	[AutoParse("exp_on_add")]
	public List<LazyExpression> onAddExpressions;

	[AutoParse("set_res_on_remove")]
	public GameRes setGameResOnRemove;

	[AutoParse("add_res_on_remove")]
	public GameRes addGameResOnRemove;

	[AutoParse("exp_on_remove")]
	public List<LazyExpression> onRemoveExpressions;

	[AutoParse("tick_rate")]
	public float tickRate;

	[AutoParse("add_res_per_tick")]
	public GameRes addGameResPerTick;

	[AutoParse("exp_per_tick")]
	public List<LazyExpression> onPerTickExpressions;

	[AutoParse("fertilizer_item")]
	public string fertilizerItemId;

	[AutoParse("world_fx_prefab")]
	public string worldFxPrefabId;

	[AutoParse("hud_fx_prefab")]
	public string hudFxPrefabId;

	public Sprite Icon => LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(string.IsNullOrEmpty(customIcon) ? id : customIcon, "b_sleep");

	public string IconId
	{
		get
		{
			if (!string.IsNullOrEmpty(customIcon))
			{
				return customIcon;
			}
			return id;
		}
	}

	public bool IsFertilizerPerk => id.StartsWith("perk_fertilize_");

	public string GetHeader()
	{
		return LLBase.L(id);
	}

	public string GetHeaderPrefix()
	{
		return LLBase.L("ui_perk");
	}
}
