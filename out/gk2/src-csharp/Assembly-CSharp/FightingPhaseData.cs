using System;

[Serializable]
public abstract class FightingPhaseData
{
	public int duration;

	public virtual void UpdatePhase(float progress, FightingLevelPresetProcessor processor, FightingLevelPreset.FightingLineData line, out int sentToSpawnThisTime)
	{
		sentToSpawnThisTime = 0;
	}
}
