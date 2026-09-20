using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PerkSystemData
{
	public List<PerkData> activePerks = new List<PerkData>();

	private PlayerData PlayerData => MainGame.PlayerData;

	public event Action<PerkData> OnPerkAdded;

	public event Action<PerkData> OnPerkRemoved;

	public event Action<List<PerkData>> OnPerksUpdated;

	public event Action<PerkData> OnPerkReapplied;

	public void AddPerk(string id)
	{
		PerkData perkData = activePerks.Find((PerkData x) => x.id == id);
		if (perkData == null)
		{
			AddNewPerk(new PerkData(id));
			return;
		}
		switch (perkData.Definition.perkAddType)
		{
		case PerkAddType.Update:
			perkData.currentDuration = perkData.Definition.duration;
			perkData.tickTimer = 0f;
			this.OnPerkReapplied?.Invoke(perkData);
			this.OnPerksUpdated?.Invoke(new List<PerkData> { perkData });
			Debug.Log("[PerkSystemData]: perk duration [" + perkData.id + "] was set to start value");
			break;
		case PerkAddType.Sum:
			perkData.currentDuration += perkData.Definition.duration;
			this.OnPerkReapplied?.Invoke(perkData);
			this.OnPerksUpdated?.Invoke(new List<PerkData> { perkData });
			Debug.Log("[PerkSystemData]: added perk duration [" + perkData.id + "]");
			break;
		case PerkAddType.AsNew:
			AddNewPerk(new PerkData(id));
			break;
		}
	}

	public void RemovePerk(string id, bool silent = false)
	{
		PerkData perkData = activePerks.Find((PerkData x) => x.id == id);
		if (perkData != null)
		{
			RemovePerk(perkData, silent);
		}
	}

	public void RemovePerk(PerkData perk, bool silent = false)
	{
		activePerks.Remove(perk);
		if (perk.Definition.setGameResOnRemove.List.Count > 0)
		{
			PlayerData.SetRes(perk.Definition.setGameResOnRemove);
		}
		if (!perk.Definition.addGameResOnRemove.IsEmpty())
		{
			PlayerData.AddRes(perk.Definition.addGameResOnRemove);
		}
		foreach (LazyExpression onRemoveExpression in perk.Definition.onRemoveExpressions)
		{
			onRemoveExpression.Evaluate();
		}
		if (!silent)
		{
			this.OnPerkRemoved?.Invoke(perk);
		}
		Debug.Log("[PerkSystemData]: perk [" + perk.id + "] removed");
	}

	public bool HasPerk(string id)
	{
		return activePerks.Find((PerkData x) => x.id == id) != null;
	}

	public void NotifyUpdated()
	{
		this.OnPerksUpdated?.Invoke(activePerks);
	}

	private void AddNewPerk(PerkData perk)
	{
		perk.currentDuration = perk.Definition.duration;
		if (!perk.Definition.setGameResOnAdd.IsEmpty())
		{
			PlayerData.SetRes(perk.Definition.setGameResOnAdd);
		}
		if (!perk.Definition.addGameResOnAdd.IsEmpty())
		{
			PlayerData.AddRes(perk.Definition.addGameResOnAdd);
		}
		foreach (LazyExpression onAddExpression in perk.Definition.onAddExpressions)
		{
			onAddExpression.Evaluate();
		}
		activePerks.Add(perk);
		this.OnPerkAdded?.Invoke(perk);
		Debug.Log("[PerkSystemData]: new perk [" + perk.id + "] was added");
	}

	public static string GetFormattedDuration(float duration)
	{
		return $"{(int)(duration / 60f)}:{(int)(duration % 60f):00}";
	}
}
