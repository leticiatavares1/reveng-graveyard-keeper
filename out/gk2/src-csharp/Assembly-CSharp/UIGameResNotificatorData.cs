using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UIGameResNotificatorData : LazyWidgetDataBase
{
	public Action<UIResElementData> onResAdd;

	private bool subscribedEvents;

	public Dictionary<string, UIResElementData> ResElementsWithAccumulators { get; private set; }

	public Dictionary<string, UIResElementData> ResElementsWithoutAccumulators { get; private set; }

	public UIGameResNotificatorData()
	{
		ResElementsWithAccumulators = new Dictionary<string, UIResElementData>();
		ResElementsWithoutAccumulators = new Dictionary<string, UIResElementData>();
		PlayerEnergyGameResSystem system = PlayerEnergyGameResSystem.GetSystem();
		system.onValueDeltaChanged = (Action<float>)Delegate.Combine(system.onValueDeltaChanged, new Action<float>(OnEnergyDeltaChanged));
		PlayerInsanityGameResSystem system2 = PlayerInsanityGameResSystem.GetSystem();
		system2.onValueDeltaChanged = (Action<float>)Delegate.Combine(system2.onValueDeltaChanged, new Action<float>(OnInstanityDeltaChanged));
		PlayerHPActivity.OnNotEnoughResOccurred += OnNotEnoughResOccurred;
		PlayerCraftActivity.OnNotEnoughResOccurred += OnNotEnoughResOccurred;
		ZombieWgoData.OnTechPointsAddedToZombie += OnZombieTechPointsAdded;
	}

	private void OnInspirationCompleted(string id)
	{
		if (ResElementsWithAccumulators.TryGetValue(id, out var value))
		{
			value.AddValue(1f);
			return;
		}
		UIResElementData value2 = new UIResElementData(id, UIGameResDisplayingType.AppearOverTargetType, "hint_inspiration", 1f, MainGame.PlayerController.PhysicalBody.PlayerView.BubblePoint, isUITarget: false);
		ResElementsWithAccumulators.Add(id, value2);
	}

	private void OnZombieTechPointsAdded(WgoData wgoData, ZombieWgoData zombieWgoData, string type, int value)
	{
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(wgoData.UniqueId);
		if (wgoViewGlobal != null && wgoViewGlobal.MainWgoPart != null)
		{
			ResElementsWithAccumulators[type] = new UIResElementData(type, UIGameResDisplayingType.AppearOverTargetType, type, value, wgoViewGlobal.MainWgoPart.BubblePoint, isUITarget: false);
		}
	}

	private void OnEnergyDeltaChanged(float value)
	{
		TryAddFromAccumulator("energy", "energy", value);
	}

	private void OnInstanityDeltaChanged(float value)
	{
		TryAddFromAccumulator("insanity", "insanity", value);
	}

	private void OnNotEnoughResOccurred(string resId)
	{
		if (!(resId == "energy"))
		{
			if (resId == "insanity")
			{
				AddIfNotShowed(resId, "no_insanity");
			}
		}
		else
		{
			AddIfNotShowed(resId, "no_energy");
		}
	}

	private void TryAddFromAccumulator(string id, string iconId, float value)
	{
		if (!ResElementsWithAccumulators.ContainsKey(id))
		{
			UIResElementData value2 = new UIResElementData(id, UIGameResDisplayingType.AppearOverTargetType, iconId, value, MainGame.PlayerController.PhysicalBody.PlayerView.BubblePoint, isUITarget: false);
			ResElementsWithAccumulators.Add(id, value2);
		}
		else
		{
			ResElementsWithAccumulators[id].AddValue(value);
		}
	}

	private void AddIfNotShowed(string id, string iconId)
	{
		if (!ResElementsWithoutAccumulators.ContainsKey(id))
		{
			UIResElementData value = new UIResElementData(id, UIGameResDisplayingType.AppearOverTargetType, iconId, 0f, MainGame.PlayerController.PhysicalBody.PlayerView.BubblePoint, isUITarget: false);
			ResElementsWithoutAccumulators.Add(id, value);
		}
	}
}
