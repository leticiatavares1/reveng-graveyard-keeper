using System;
using LazyBearTechnology;

public class UIFightingTimelineRendererData : LazyWidgetDataBase
{
	public FightingLevelPreset Preset { get; private set; }

	public FightingLevelPresetProcessor Processor { get; private set; }

	public FightingLevel CurrentLevel { get; private set; }

	public float TotalTime { get; private set; }

	public event Action<float> OnProgressChanged;

	public UIFightingTimelineRendererData(FightingLevelPreset preset, FightingLevelPresetProcessor processor, FightingLevel currentLevel)
	{
		Preset = preset;
		Processor = processor;
		CurrentLevel = currentLevel;
		TotalTime = 0f;
		float num = 0f;
		foreach (FightingLevelPreset.FightingLineData line in Preset.lines)
		{
			foreach (FightingPhaseData phase in line.phases)
			{
				TotalTime += phase.duration;
			}
			if (TotalTime > num)
			{
				num = TotalTime;
			}
			TotalTime = 0f;
		}
		TotalTime = num;
		Processor.OnProgressChanged += HandleProgressChanged;
	}

	public void UnsubscribeFromProcessor()
	{
		Processor.OnProgressChanged -= HandleProgressChanged;
	}

	public void HandleProgressChanged(float progress)
	{
		this.OnProgressChanged?.Invoke(progress);
	}
}
