using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoadingPipeline
{
	private class TaskEntry
	{
		public readonly UniTaskCompletionSource Completion = new UniTaskCompletionSource();

		public readonly Func<float> ProgressGetter;

		public readonly float Weight;

		public bool IsComplete;

		public TaskEntry(Func<float> progressGetter, float weight)
		{
			ProgressGetter = progressGetter;
			Weight = weight;
		}
	}

	private static LoadingPipeline instance;

	private readonly Dictionary<LoadingStage, List<TaskEntry>> stages = new Dictionary<LoadingStage, List<TaskEntry>>();

	public static LoadingPipeline Instance => instance ?? (instance = new LoadingPipeline());

	public void RegisterTask(LoadingStage stage, UniTask task, float weight = 1f)
	{
		RegisterTask(stage, task, null, weight);
	}

	public void RegisterTask(LoadingStage stage, UniTask task, Func<float> progressGetter, float weight = 1f)
	{
		if (!stages.TryGetValue(stage, out var value))
		{
			value = new List<TaskEntry>();
			stages[stage] = value;
		}
		TaskEntry taskEntry = new TaskEntry(progressGetter, weight);
		value.Add(taskEntry);
		TrackCompletion(task, taskEntry).Forget();
	}

	public float GetStageProgress(LoadingStage stage)
	{
		if (GameShutdown.IsRequested)
		{
			return 1f;
		}
		if (!stages.TryGetValue(stage, out var value) || value.Count == 0)
		{
			return 1f;
		}
		float num = 0f;
		float num2 = 0f;
		foreach (TaskEntry item in value)
		{
			num += item.Weight;
			float num3 = (item.IsComplete ? 1f : (item.ProgressGetter?.Invoke() ?? 0f));
			num2 += num3 * item.Weight;
		}
		if (!(num > 0f))
		{
			return 1f;
		}
		return num2 / num;
	}

	public bool HasStage(LoadingStage stage)
	{
		if (stages.TryGetValue(stage, out var value))
		{
			return value.Count > 0;
		}
		return false;
	}

	public void ClearStage(LoadingStage stage)
	{
		stages.Remove(stage);
	}

	public async UniTask AwaitStage(LoadingStage stage)
	{
		GameShutdown.ThrowIfRequested();
		if (!stages.TryGetValue(stage, out var value))
		{
			return;
		}
		List<UniTask> list = new List<UniTask>(value.Count);
		foreach (TaskEntry item in value)
		{
			list.Add(item.Completion.Task);
		}
		await UniTask.WhenAll(list).AttachExternalCancellation(GameShutdown.Token);
	}

	public async UniTask AwaitFromStage(LoadingStage fromStage)
	{
		foreach (LoadingStage value in Enum.GetValues(typeof(LoadingStage)))
		{
			if (value > fromStage)
			{
				break;
			}
			await AwaitStage(value);
		}
	}

	public bool IsStageComplete(LoadingStage stage)
	{
		if (!stages.TryGetValue(stage, out var value))
		{
			return true;
		}
		foreach (TaskEntry item in value)
		{
			if (!item.IsComplete)
			{
				return false;
			}
		}
		return true;
	}

	private static async UniTaskVoid TrackCompletion(UniTask task, TaskEntry entry)
	{
		bool canceled = false;
		try
		{
			await UniTask.Yield(PlayerLoopTiming.Update, GameShutdown.Token);
			await task;
		}
		catch (OperationCanceledException)
		{
			canceled = true;
		}
		catch (Exception exception)
		{
			if (!GameShutdown.IsRequested)
			{
				Debug.LogException(exception);
			}
		}
		finally
		{
			entry.IsComplete = true;
			if (canceled || GameShutdown.IsRequested)
			{
				entry.Completion.TrySetCanceled();
			}
			else
			{
				entry.Completion.TrySetResult();
			}
		}
	}
}
