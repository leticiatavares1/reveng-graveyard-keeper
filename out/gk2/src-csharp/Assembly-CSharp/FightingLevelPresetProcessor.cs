using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using UnityEngine;

public class FightingLevelPresetProcessor : MonoBehaviour
{
	public delegate void EnemiesSpawnDelegate(string id, int count, FightingLevelPreset.FightingLineData line);

	private FightingLevelPreset currentPreset;

	private Coroutine mainProcessorCoroutine;

	public bool IsPlaying { get; private set; }

	public float CurrentProgress { get; private set; }

	public float TotalDuration { get; private set; }

	public float ProgressNormalized
	{
		get
		{
			if (!(TotalDuration > 0f))
			{
				return 0f;
			}
			return CurrentProgress / TotalDuration;
		}
	}

	public Dictionary<int, int> SpawnedEnemiesCountByLine { get; private set; } = new Dictionary<int, int>();


	public event EnemiesSpawnDelegate OnEnemiesSpawn;

	public event Action OnPresetStarted;

	public event Action OnPresetFinished;

	public event Action<float> OnProgressChanged;

	public void StartPreset(FightingLevelPreset preset)
	{
		SpawnedEnemiesCountByLine.Clear();
		if (IsPlaying)
		{
			StopPreset();
		}
		currentPreset = preset;
		IsPlaying = true;
		mainProcessorCoroutine = StartCoroutine(ProcessPreset());
	}

	public void StopPreset()
	{
		if (IsPlaying)
		{
			if (mainProcessorCoroutine != null)
			{
				StopCoroutine(mainProcessorCoroutine);
				mainProcessorCoroutine = null;
			}
			IsPlaying = false;
			CurrentProgress = 0f;
			currentPreset = null;
		}
	}

	private IEnumerator ProcessPreset()
	{
		CalculateTotalDuration();
		CurrentProgress = 0f;
		this.OnPresetStarted?.Invoke();
		int remainingLines = currentPreset.lines.Count;
		for (int i = 0; i < currentPreset.lines.Count; i++)
		{
			FightingLevelPreset.FightingLineData line = currentPreset.lines[i];
			StartCoroutine(RunLineAndSignal(line, i, delegate
			{
				remainingLines--;
			}));
		}
		while (remainingLines > 0)
		{
			while ((LazySingleton<FightingGameController>.Instance != null && LazySingleton<FightingGameController>.Instance.IsPaused) || MainGame.IsGamePaused)
			{
				yield return null;
			}
			if (!IsPlaying)
			{
				yield break;
			}
			CurrentProgress += Time.deltaTime;
			this.OnProgressChanged?.Invoke(ProgressNormalized);
			yield return null;
		}
		CurrentProgress = TotalDuration;
		this.OnProgressChanged?.Invoke(ProgressNormalized);
		this.OnPresetFinished?.Invoke();
		IsPlaying = false;
	}

	private IEnumerator ProcessLineCoroutine(FightingLevelPreset.FightingLineData line, int lineIndex)
	{
		foreach (FightingPhaseData phase in line.phases)
		{
			if (!IsPlaying)
			{
				yield break;
			}
			if (phase is FightingPhasePauseData pausePhase)
			{
				float waited = 0f;
				while (waited < (float)pausePhase.duration)
				{
					if (!IsPlaying)
					{
						yield break;
					}
					while ((LazySingleton<FightingGameController>.Instance != null && LazySingleton<FightingGameController>.Instance.IsPaused) || MainGame.IsGamePaused)
					{
						yield return null;
					}
					waited += Time.deltaTime;
					yield return null;
				}
			}
			else if (phase is FightingPhaseSpawnEnemiesData phaseSpawnData)
			{
				yield return StartCoroutine(SpawnEnemiesForPhase(phaseSpawnData, line, lineIndex));
			}
		}
	}

	private IEnumerator RunLineAndSignal(FightingLevelPreset.FightingLineData line, int lineIndex, Action onComplete)
	{
		yield return ProcessLineCoroutine(line, lineIndex);
		onComplete?.Invoke();
	}

	public void SpawnEnemies(string id, int count, FightingLevelPreset.FightingLineData line)
	{
		if (!(currentPreset != null) || line.isEnabled)
		{
			this.OnEnemiesSpawn?.Invoke(id, count, line);
		}
	}

	private void CalculateTotalDuration()
	{
		TotalDuration = 0f;
		if (currentPreset == null)
		{
			return;
		}
		foreach (FightingLevelPreset.FightingLineData line in currentPreset.lines)
		{
			float num = line.phases?.Sum((Func<FightingPhaseData, float>)((FightingPhaseData phase) => phase.duration)) ?? 0f;
			if (num > TotalDuration)
			{
				TotalDuration = num;
			}
		}
	}

	public IEnumerator SpawnEnemiesForPhase(FightingPhaseSpawnEnemiesData phaseSpawnData, FightingLevelPreset.FightingLineData line, int lineIndex)
	{
		float progress = 0f;
		phaseSpawnData.Reset();
		int sentToSpawnThisTime = 0;
		while (progress < (float)phaseSpawnData.duration)
		{
			if (!IsPlaying)
			{
				yield break;
			}
			while ((LazySingleton<FightingGameController>.Instance != null && LazySingleton<FightingGameController>.Instance.IsPaused) || MainGame.IsGamePaused)
			{
				yield return null;
			}
			progress += Time.deltaTime;
			phaseSpawnData.UpdatePhase(progress, this, line, out sentToSpawnThisTime);
			if (SpawnedEnemiesCountByLine.TryGetValue(lineIndex, out var value))
			{
				SpawnedEnemiesCountByLine[lineIndex] = value + sentToSpawnThisTime;
			}
			else
			{
				SpawnedEnemiesCountByLine[lineIndex] = sentToSpawnThisTime;
			}
			yield return null;
		}
		phaseSpawnData.FlushRemaining(this, line, out sentToSpawnThisTime);
		if (SpawnedEnemiesCountByLine.TryGetValue(lineIndex, out var value2))
		{
			SpawnedEnemiesCountByLine[lineIndex] = value2 + sentToSpawnThisTime;
		}
		else
		{
			SpawnedEnemiesCountByLine[lineIndex] = sentToSpawnThisTime;
		}
	}
}
