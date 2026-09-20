using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class CPA_SetGameObjectActive : CapturePointAction
{
	public enum GameObjectSource
	{
		GameObject,
		Addressable
	}

	public GameObjectSource source;

	public GameObject go;

	public AssetReferenceGameObject gameObjectReference;

	public string gameObjectId;

	public bool isActive;

	private FightingLevel currentLevel;

	private bool IsGameObjectSource => source == GameObjectSource.GameObject;

	private bool IsAddressableSource => source == GameObjectSource.Addressable;

	public override void Execute(LazyConsts.Fighting.TeamType teamType, FightingLevel level)
	{
		if (base.teamType == teamType)
		{
			currentLevel = level;
			GameObject gameObject = ResolveTarget(level);
			if ((bool)gameObject)
			{
				gameObject.SetActive(isActive);
			}
		}
	}

	private GameObject ResolveTarget(FightingLevel level)
	{
		return source switch
		{
			GameObjectSource.GameObject => go, 
			GameObjectSource.Addressable => ResolveFromAddressable(level), 
			_ => go, 
		};
	}

	private GameObject ResolveFromAddressable(FightingLevel level)
	{
		if (!TryGetLoadedAddressableRoot(out var root))
		{
			return null;
		}
		FightingStageGameObjectMapper componentInChildren = root.GetComponentInChildren<FightingStageGameObjectMapper>(includeInactive: true);
		if (!componentInChildren)
		{
			return null;
		}
		if (!componentInChildren.TryGetGameObject(gameObjectId, out var result))
		{
			return null;
		}
		return result;
	}

	private bool TryGetLoadedAddressableRoot(out GameObject root)
	{
		root = null;
		if (gameObjectReference == null)
		{
			return false;
		}
		root = currentLevel.GetStageInstanceFromAssetReference(gameObjectReference);
		return root != null;
	}
}
