using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class PerksWidgetData : LazyWidgetDataBase
{
	private List<PerkType> drawingTypes;

	public List<PerkData> Perks { get; private set; }

	public event Action<PerkData> OnPerkAdded;

	public event Action<PerkData> OnPerkRemoved;

	public event Action<PerkData> OnPerkUpdated;

	public PerksWidgetData()
		: this(MainGame.Instance.GameSave, new List<PerkType> { PerkType.Default })
	{
	}

	public PerksWidgetData(GameSave gameSave, List<PerkType> drawingTypes)
	{
		this.drawingTypes = drawingTypes;
		Perks = new List<PerkData>();
		for (int i = 0; i < gameSave.perkSystemData.activePerks.Count; i++)
		{
			PerkData perkData = gameSave.perkSystemData.activePerks[i];
			if (!perkData.Definition.isHidden && drawingTypes.Contains(perkData.Definition.perkType))
			{
				Perks.Add(perkData);
			}
		}
		MainGame.Instance.GameSave.perkSystemData.OnPerkAdded += AddPerkData;
		MainGame.Instance.GameSave.perkSystemData.OnPerksUpdated += UpdatePerkData;
		MainGame.Instance.GameSave.perkSystemData.OnPerkRemoved += RemovePerkData;
	}

	private void AddPerkData(PerkData perkData)
	{
		if (!perkData.Definition.isHidden && drawingTypes.Contains(perkData.Definition.perkType))
		{
			if (Perks.Find((PerkData x) => x.id == perkData.id) != null)
			{
				Debug.LogWarning("[PerkWidgetData]: perk [" + perkData.id + "] has been already added");
				return;
			}
			Perks.Add(perkData);
			this.OnPerkAdded?.Invoke(perkData);
		}
	}

	private void RemovePerkData(PerkData perkData)
	{
		if (!perkData.Definition.isHidden && drawingTypes.Contains(perkData.Definition.perkType))
		{
			PerkData perkData2 = Perks.Find((PerkData x) => x.id == perkData.id);
			if (perkData2 == null)
			{
				Debug.LogWarning("[PerksWidgetData]: perk [" + perkData.id + "] has been already removed");
				return;
			}
			Perks.Remove(perkData2);
			this.OnPerkRemoved?.Invoke(perkData2);
		}
	}

	private void UpdatePerkData(List<PerkData> perksData)
	{
		foreach (PerkData perkData in perksData)
		{
			if (!perkData.Definition.isHidden && drawingTypes.Contains(perkData.Definition.perkType))
			{
				PerkData perkData2 = Perks.Find((PerkData x) => x.id == perkData.id);
				if (perkData2 == null)
				{
					Debug.LogWarning("[PerksWidgetData]: perk [" + perkData.id + "] tries to update data but it has been removed");
				}
				else
				{
					this.OnPerkUpdated?.Invoke(perkData2);
				}
			}
		}
	}
}
