using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class CPA_SetZombieFogActive : CapturePointAction
{
	public enum GameObjectSource
	{
		GameObject,
		Addressable
	}

	public GameObjectSource source;

	public ZombieFog fog;

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
			ZombieFog zombieFog = ResolveTarget(level);
			if ((bool)zombieFog)
			{
				zombieFog.DoFade(isActive);
			}
		}
	}

	private ZombieFog ResolveTarget(FightingLevel level)
	{
		return source switch
		{
			GameObjectSource.GameObject => fog, 
			GameObjectSource.Addressable => ResolveFromAddressable(level), 
			_ => fog, 
		};
	}

	private ZombieFog ResolveFromAddressable(FightingLevel level)
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
		if (!componentInChildren.TryGetGameObject(gameObjectId, out var go))
		{
			return null;
		}
		if (!go)
		{
			return null;
		}
		return go.GetComponent<ZombieFog>();
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
