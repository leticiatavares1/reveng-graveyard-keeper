using System;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class BakedChunkableObjectComponentData : IChunkableObject
{
	public string pathToObject;

	[HideInInspector]
	public Vector3 worldPos;

	[HideInInspector]
	public Vector3 lossyScale;

	[HideInInspector]
	public Quaternion rotation;

	[HideInInspector]
	public BurstableChunkBoundsPair chunkBounds;

	[HideInInspector]
	public Vector3 gndLocalPos;

	[HideInInspector]
	public string parentGdPointId;

	private bool isVisible;

	private BakedChunkableObjectComponent view;

	private GDPointData parentGdPointData;

	public MultiFlagOR<ChunkingIgnoreType> IgnoreMultiFlag { get; set; }

	public bool IgnoreChunkVisibility => false;

	public BurstableBounds GetChunkableData()
	{
		return chunkBounds.GetBounds();
	}

	public void UpdateChunkVisibility(bool isVisible)
	{
		if (Application.isPlaying)
		{
			this.isVisible = isVisible;
			RefreshViewVisibility();
			if (!isVisible)
			{
				UnsubscribeFromParentGdPoint();
			}
		}
	}

	private void RefreshViewVisibility()
	{
		if (isVisible && !IsDisabledByParentGdPoint())
		{
			if (!(view != null))
			{
				view = BakedChunkableObjectPool.Get(pathToObject);
				if (!(view == null))
				{
					view.SetData(this);
					view.ApplyData();
				}
			}
		}
		else if (view != null)
		{
			BakedChunkableObjectPool.Release(pathToObject, view);
			view = null;
		}
	}

	private bool IsDisabledByParentGdPoint()
	{
		if (!string.IsNullOrEmpty(parentGdPointId))
		{
			if (parentGdPointData == null)
			{
				parentGdPointData = MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(parentGdPointId);
				if (parentGdPointData != null)
				{
					parentGdPointData.OnActiveStateChanged += OnGdPointStateChanged;
					MainGame.OnGoToMainMenu = (Action)Delegate.Combine(MainGame.OnGoToMainMenu, new Action(OnGoToMainMenu));
				}
			}
			if (parentGdPointData != null && !parentGdPointData.Enabled)
			{
				return true;
			}
		}
		return false;
	}

	private void OnGdPointStateChanged(bool enabled)
	{
		RefreshViewVisibility();
	}

	private void OnGoToMainMenu()
	{
		UnsubscribeFromParentGdPoint();
	}

	private void UnsubscribeFromParentGdPoint()
	{
		if (parentGdPointData != null)
		{
			parentGdPointData.OnActiveStateChanged -= OnGdPointStateChanged;
			MainGame.OnGoToMainMenu = (Action)Delegate.Remove(MainGame.OnGoToMainMenu, new Action(OnGoToMainMenu));
			parentGdPointData = null;
		}
	}
}
