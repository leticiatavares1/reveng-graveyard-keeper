using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class FightingStage : MonoBehaviour, IBakingContext
{
	private const int DEFAULT_STAGES_COUNT = 10;

	[SerializeField]
	private List<FightingStageData> stagesData = new List<FightingStageData>();

	[SerializeField]
	private List<ChunkableObjectComponent> chunkableObjects = new List<ChunkableObjectComponent>();

	[SerializeField]
	private List<BakedChunkableObjectComponentData> bakedData = new List<BakedChunkableObjectComponentData>();

	[SerializeField]
	private bool disabledBakingContext;

	[NonSerialized]
	private readonly List<BakedChunkableObjectComponentData> registeredBakedChunkCaches = new List<BakedChunkableObjectComponentData>();

	[NonSerialized]
	private readonly List<ChunkableObjectComponent> registeredLiveChunkables = new List<ChunkableObjectComponent>();

	public IReadOnlyList<FightingStageData> StagesData => stagesData;

	public List<BakedChunkableObjectComponentData> GetBakedData => bakedData;

	public int Editor_BakingContextPriority => 5;

	public bool IsEnabledForStage(int stageId)
	{
		foreach (FightingStageData stagesDatum in stagesData)
		{
			if (stagesDatum.id == stageId && stagesDatum.enabled)
			{
				return true;
			}
		}
		return false;
	}

	public void ApplyStage(int stageId)
	{
		bool flag = IsEnabledForStage(stageId);
		if (chunkableObjects.Count == 0)
		{
			CollectChunkableObjects();
		}
		ApplyCustomVisibilityDisabled(!flag);
		base.gameObject.SetActive(flag);
	}

	public void ApplyStageFromRuntimeInstance(int stageId)
	{
		bool flag = IsEnabledForStage(stageId);
		CollectChunkableObjects();
		ApplyCustomVisibilityDisabled(!flag);
		base.gameObject.SetActive(flag);
	}

	private void ApplyCustomVisibilityDisabled(bool disabled)
	{
		for (int i = 0; i < chunkableObjects.Count; i++)
		{
			ChunkableObjectComponent chunkableObjectComponent = chunkableObjects[i];
			if (!(chunkableObjectComponent == null))
			{
				chunkableObjectComponent.CustomVisibilityDisabled = disabled;
			}
		}
	}

	public int GetStageMask()
	{
		int num = 0;
		foreach (FightingStageData stagesDatum in stagesData)
		{
			if (stagesDatum.enabled)
			{
				num |= 1 << stagesDatum.id;
			}
		}
		return num;
	}

	public void CollectChunkableObjects()
	{
		chunkableObjects.Clear();
		GetComponentsInChildren(includeInactive: true, chunkableObjects);
	}

	public void Editor_SetContextEnableState(bool isActive)
	{
		disabledBakingContext = !isActive;
	}

	private void OnEnable()
	{
		if (!disabledBakingContext)
		{
			BakingContextRuntimeRegistration.Register(this, registeredBakedChunkCaches);
			RegisterLiveChunkableObjects();
		}
	}

	private void OnDisable()
	{
		ForceUnregisterBakingContext();
	}

	public void ForceUnregisterBakingContext()
	{
		if (!disabledBakingContext)
		{
			BakingContextRuntimeRegistration.Unregister(registeredBakedChunkCaches);
			UnregisterLiveChunkableObjects();
		}
	}

	private void RegisterLiveChunkableObjects()
	{
		UnregisterLiveChunkableObjects();
		if (chunkableObjects.Count == 0)
		{
			CollectChunkableObjects();
		}
		for (int i = 0; i < chunkableObjects.Count; i++)
		{
			ChunkableObjectComponent chunkableObjectComponent = chunkableObjects[i];
			if (!(chunkableObjectComponent == null))
			{
				chunkableObjectComponent.UpdateChunkVisibility(isVisible: false);
				registeredLiveChunkables.Add(chunkableObjectComponent);
			}
		}
		if (registeredLiveChunkables.Count != 0)
		{
			ChunkManager instance = LazySingleton<ChunkManager>.Instance;
			if (!(instance == null))
			{
				instance.RegisterChunks(registeredLiveChunkables, ChunkManagerLayerType.FightingLevelStaticObjects);
			}
		}
	}

	private void UnregisterLiveChunkableObjects()
	{
		if (registeredLiveChunkables.Count == 0)
		{
			return;
		}
		ChunkManager instance = LazySingleton<ChunkManager>.Instance;
		if (instance != null)
		{
			instance.UnregisterChunks(registeredLiveChunkables, ChunkManagerLayerType.FightingLevelStaticObjects);
		}
		for (int i = 0; i < registeredLiveChunkables.Count; i++)
		{
			ChunkableObjectComponent chunkableObjectComponent = registeredLiveChunkables[i];
			if (!(chunkableObjectComponent == null))
			{
				chunkableObjectComponent.UpdateChunkVisibility(isVisible: false);
			}
		}
		registeredLiveChunkables.Clear();
	}

	private void Reset()
	{
		for (int i = 0; i < 10; i++)
		{
			stagesData.Add(new FightingStageData(i + 1));
		}
	}
}
