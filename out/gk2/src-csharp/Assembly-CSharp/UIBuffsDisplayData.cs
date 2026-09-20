using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIBuffsDisplayData : LazyWidgetDataBase
{
	private List<PerkType> drawingTypes;

	public List<UIBuffElementData> UIBuffElementsData { get; set; }

	public event Action<UIBuffElementData> OnBuffAdded;

	public event Action<string> OnBuffRemoved;

	public event Action<string> OnBuffUpdated;

	public event Action<string> OnBuffFxRequested;

	public UIBuffsDisplayData(List<PerkType> drawingTypes)
	{
		this.drawingTypes = drawingTypes;
		UIBuffElementsData = new List<UIBuffElementData>();
		for (int i = 0; i < MainGame.Instance.GameSave.perkSystemData.activePerks.Count; i++)
		{
			PerkData perkData = MainGame.Instance.GameSave.perkSystemData.activePerks[i];
			if (drawingTypes.Contains(perkData.Definition.perkType))
			{
				UIBuffElementData item = new UIBuffElementData(perkData, perkData.currentDuration, perkData.Definition.hiddenTimer, perkData.Definition.duration < 0f);
				UIBuffElementsData.Add(item);
			}
		}
		MainGame.Instance.GameSave.perkSystemData.OnPerkAdded += AddPerkElementData;
		MainGame.Instance.GameSave.perkSystemData.OnPerksUpdated += UpdatePerkElementsData;
		MainGame.Instance.GameSave.perkSystemData.OnPerkRemoved += RemovePerkElementData;
		MainGame.Instance.GameSave.perkSystemData.OnPerkReapplied += RequestBuffFx;
	}

	private void AddPerkElementData(PerkData perkData)
	{
		if (!perkData.Definition.isHidden && drawingTypes.Contains(perkData.Definition.perkType))
		{
			if (UIBuffElementsData.Find((UIBuffElementData x) => x.PerkData.id == perkData.id) != null)
			{
				Debug.LogWarning("[UIBuffsDisplayData]: buff [" + perkData.id + "] has been already added");
				return;
			}
			UIBuffElementData uIBuffElementData = new UIBuffElementData(perkData, perkData.currentDuration, perkData.Definition.hiddenTimer, perkData.Definition.duration < 0f);
			UIBuffElementsData.Add(uIBuffElementData);
			this.OnBuffAdded?.Invoke(uIBuffElementData);
		}
	}

	private void RequestBuffFx(PerkData perkData)
	{
		if (!perkData.Definition.isHidden && drawingTypes.Contains(perkData.Definition.perkType) && UIBuffElementsData.Find((UIBuffElementData x) => x.PerkData.id == perkData.id) != null)
		{
			this.OnBuffFxRequested?.Invoke(perkData.id);
		}
	}

	private void RemovePerkElementData(PerkData perkData)
	{
		if (!perkData.Definition.isHidden && drawingTypes.Contains(perkData.Definition.perkType))
		{
			UIBuffElementData uIBuffElementData = UIBuffElementsData.Find((UIBuffElementData x) => x.PerkData.id == perkData.id);
			if (uIBuffElementData == null)
			{
				Debug.LogWarning("[UIBuffsDisplayData]: buff [" + perkData.id + "] has been already removed");
				return;
			}
			UIBuffElementsData.Remove(uIBuffElementData);
			this.OnBuffRemoved?.Invoke(uIBuffElementData.PerkData.id);
		}
	}

	private void UpdatePerkElementsData(List<PerkData> buffsData)
	{
		foreach (PerkData buffData in buffsData)
		{
			if (!buffData.Definition.isHidden && drawingTypes.Contains(buffData.Definition.perkType))
			{
				UIBuffElementData uIBuffElementData = UIBuffElementsData.Find((UIBuffElementData x) => x.PerkData.id == buffData.id);
				if (uIBuffElementData == null)
				{
					Debug.LogWarning("[UIBuffsDisplayData]: buff [" + buffData.id + "] tries to update data but it has been removed");
					continue;
				}
				uIBuffElementData.UpdateData(buffData.currentDuration);
				this.OnBuffUpdated?.Invoke(uIBuffElementData.PerkData.id);
			}
		}
	}
}
