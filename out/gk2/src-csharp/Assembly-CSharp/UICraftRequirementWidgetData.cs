using System.Collections.Generic;
using LazyBearTechnology;

public class UICraftRequirementWidgetData : LazyWidgetDataBase
{
	public string Id { get; private set; }

	public string IconId { get; private set; }

	public string RequirementValue { get; private set; }

	public List<PerkData> LinkedActivePerks { get; private set; }

	public CraftDefBase CraftDef { get; private set; }

	public Item ToolForWork { get; private set; }

	public bool IsRequirement { get; private set; }

	public bool IsEnough { get; private set; }

	public UICraftRequirementWidgetData(string id, string iconId, float value, List<PerkData> linkedActivePerks, CraftDefBase craftDef, Item toolForWork, bool isRequirement, bool isEnough = true)
	{
		Id = id;
		IconId = iconId;
		if (value != 0f)
		{
			RequirementValue = value.ToInvariantCultureString();
		}
		LinkedActivePerks = linkedActivePerks;
		CraftDef = craftDef;
		ToolForWork = toolForWork;
		IsRequirement = isRequirement;
		IsEnough = isEnough;
	}

	public UICraftRequirementWidgetData(string id, string value, bool isRequirement, bool isEnough = true)
	{
		Id = id;
		RequirementValue = value;
		IsEnough = isEnough;
		IsRequirement = isRequirement;
	}
}
