using System;
using LazyBearTechnology;

public class UIEnergySanityBarData : LazyWidgetDataBase
{
	public Action onFillValueChanged;

	public PlayerData PlayerData { get; }

	public float EnergyFillValue { get; private set; }

	public float InsanityFillValue { get; private set; }

	public UIEnergySanityBarData(GameSave gameSave)
	{
		PlayerData = gameSave.playerData;
		UpdateFillData(0f);
		PlayerEnergyGameResSystem system = PlayerEnergyGameResSystem.GetSystem();
		system.onValueChanged = (Action<float>)Delegate.Combine(system.onValueChanged, new Action<float>(UpdateFillData));
		PlayerInsanityGameResSystem system2 = PlayerInsanityGameResSystem.GetSystem();
		system2.onValueChanged = (Action<float>)Delegate.Combine(system2.onValueChanged, new Action<float>(UpdateFillData));
	}

	private void UpdateFillData(float value)
	{
		EnergyFillValue = PlayerData.GetRes("energy") / 100f;
		InsanityFillValue = PlayerData.GetRes("insanity") / 100f;
		onFillValueChanged?.Invoke();
	}
}
