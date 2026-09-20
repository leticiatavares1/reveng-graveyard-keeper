using System;
using System.Collections.Generic;
using LinqTools;

[Serializable]
public class AlchemyMixDef : CraftDef
{
	public string[] ingredients;

	public ItemDef ResultItem => GameBalance.Me.GetData<ItemDef>(outputItems.chanceOutputItems[0].id);

	public AlchemyFormulaDef Formula => GameBalance.Me.GetData<AlchemyFormulaDef>(outputItems.chanceOutputItems[0].id ?? "");

	public CraftDef AlchemyWorkBenchCraft => GameBalance.GetAlchemyMixCraftDef(id);

	public CraftDef BoostCraft
	{
		get
		{
			if (!id.EndsWith("_boost"))
			{
				return null;
			}
			return GameBalance.GetCraftDef(id.Substring(id.LastIndexOf(':') + 1));
		}
	}

	public bool IsResultKnown => (MainGame.Instance?.GameSave?.knowledgeSystem)?.IsAlchemyFormulaKnown(Formula) ?? true;

	public static bool IsUnknownMixResult(string craftId)
	{
		if (string.IsNullOrEmpty(craftId) || !craftId.StartsWith("mix"))
		{
			return false;
		}
		AlchemyMixDef alchemyMixDef = GameBalance.GetAlchemyMixDef(craftId);
		if (alchemyMixDef != null)
		{
			return !alchemyMixDef.IsResultKnown;
		}
		return false;
	}

	public static string MixId(string[] ingredients, CraftDef boostDef = null)
	{
		string text = "mix";
		List<string> list = ingredients.ToList();
		list.Sort();
		foreach (string item in list)
		{
			text = text + ":" + item;
		}
		if (boostDef != null && boostDef.id.EndsWith("_boost"))
		{
			text = text + ":" + boostDef.id;
		}
		return text;
	}
}
