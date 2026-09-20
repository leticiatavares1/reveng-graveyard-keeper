using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CraftElementSurvey : CraftElementT<SurveyDef>
{
	[SerializeField]
	private string selectedSurveyItemId;

	public string SelectedSurveyItemId => selectedSurveyItemId;

	public CraftElementSurvey(CraftDefBase definition)
		: base(definition)
	{
	}

	public CraftElementSurvey(CraftDefBase definition, CraftParamsData craftParamsData)
		: base(definition, craftParamsData)
	{
	}

	public CraftElementSurvey(CraftDefBase definition, List<NeedItemData> requirements, CraftParamsData craftParamsData, string selectedSurveyItemId)
		: base(definition.id, 1, requirements, craftParamsData)
	{
		this.selectedSurveyItemId = selectedSurveyItemId;
	}

	protected CraftElementSurvey(CraftElementSurvey other, int count = 1)
		: base((CraftElementT<SurveyDef>)other, count)
	{
		selectedSurveyItemId = other.selectedSurveyItemId;
	}

	public override CraftElementBase Clone(int count = 1)
	{
		return new CraftElementSurvey(this, count);
	}

	public override void RemoveCraftRequirements(ICraftable craftable)
	{
		if (base.Definition.isScienceFuelCraft)
		{
			craftInput = craftable.GetCraftableMultiInventory().RemoveItems(base.Requirements, craftable as WgoData);
			return;
		}
		List<NeedItemData> list = new List<NeedItemData>();
		for (int i = 1; i < base.Requirements.Count; i++)
		{
			list.Add(base.Requirements[i]);
		}
		craftInput = craftable.GetCraftableMultiInventory().RemoveItems(list, craftable as WgoData);
	}

	public OutputPreview GetSelectedItemOutputPreview()
	{
		ItemDef dataOrNull = GameBalance.Me.GetDataOrNull<ItemDef>(selectedSurveyItemId);
		if (dataOrNull == null)
		{
			return base.Definition.GetOutputPreview();
		}
		return new OutputPreview(base.Definition.id, string.Empty, isStarOutput: false, 1, -1, dataOrNull.iconId);
	}

	protected override CraftDefBase GetCraftDef()
	{
		return GameBalance.GetSurveyDef(craftId);
	}
}
