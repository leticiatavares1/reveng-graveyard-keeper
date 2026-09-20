using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[ExecuteAlways]
public abstract class ConveyorAnimator : ConveyorSystemAnimator
{
	[Serializable]
	private class SpriteAnimationData
	{
		public string spriteNameWithoutIdx = string.Empty;

		public List<GenericSprite> spritesToAnimate = new List<GenericSprite>();
	}

	[SerializeField]
	private Sprite sprite;

	public Vector3 displacement = Vector3.zero;

	public Vector3 itemRotation = Vector3.zero;

	public Vector3 itemScale = Vector3.one;

	[SerializeField]
	private List<SpriteAnimationData> animationsData = new List<SpriteAnimationData>();

	[SerializeField]
	protected List<ConveyorAnimatableItem> itemsToAnimate = new List<ConveyorAnimatableItem>();

	private Dictionary<string, SpriteAnimationData> animationDataDict = new Dictionary<string, SpriteAnimationData>();

	private Dictionary<Sprite, Sprite[]> drivingSpriteToTargets = new Dictionary<Sprite, Sprite[]>();

	private Sprite currentDrivingSprite;

	private Sprite[] currentTargets;

	public abstract ConveyorSystemAnimatorType Type { get; }

	public virtual void TryRegister()
	{
	}

	public virtual void Unregister()
	{
	}

	public override void CustomUpdate()
	{
		if (this.sprite == null || (animationsData.Count == 0 && itemsToAnimate.Count == 0))
		{
			return;
		}
		Sprite[] orBuildTargets = GetOrBuildTargets(this.sprite);
		for (int i = 0; i < animationsData.Count; i++)
		{
			Sprite sprite = orBuildTargets[i];
			if (sprite == null)
			{
				continue;
			}
			foreach (GenericSprite item in animationsData[i].spritesToAnimate)
			{
				if (item.SpriteRenderer.sprite != sprite)
				{
					item.SpriteRenderer.sprite = sprite;
				}
			}
		}
		foreach (ConveyorAnimatableItem item2 in itemsToAnimate)
		{
			item2.transform.localPosition = displacement;
			item2.transform.localEulerAngles = itemRotation;
			item2.transform.localScale = itemScale;
		}
	}

	public void AddAnimatable(ConveyorAnimatable animatable)
	{
		if (animatable is ConveyorAnimatableSprite conveyorAnimatableSprite)
		{
			if (animationDataDict.TryGetValue(conveyorAnimatableSprite.spriteNameWithoutIdx, out var value))
			{
				value.spritesToAnimate.Add(conveyorAnimatableSprite.sprite);
			}
			else
			{
				value = new SpriteAnimationData
				{
					spriteNameWithoutIdx = conveyorAnimatableSprite.spriteNameWithoutIdx
				};
				animationDataDict.Add(conveyorAnimatableSprite.spriteNameWithoutIdx, value);
				animationsData.Add(value);
			}
			value.spritesToAnimate.Add(conveyorAnimatableSprite.sprite);
		}
		else if (animatable is ConveyorAnimatableItem item)
		{
			itemsToAnimate.Add(item);
		}
		animatable.CreateCache();
		InvalidateDrivingSpriteCache();
		if (sprite != null)
		{
			GetOrBuildTargets(sprite);
		}
	}

	public void RemoveAnimatable(ConveyorAnimatable animatable)
	{
		if (animatable is ConveyorAnimatableSprite conveyorAnimatableSprite)
		{
			if (animationDataDict.TryGetValue(conveyorAnimatableSprite.spriteNameWithoutIdx, out var value))
			{
				value.spritesToAnimate.Remove(conveyorAnimatableSprite.sprite);
			}
			if (value == null || value.spritesToAnimate.Count == 0)
			{
				animationDataDict.Remove(conveyorAnimatableSprite.spriteNameWithoutIdx);
				animationsData.Remove(value);
			}
		}
		else if (animatable is ConveyorAnimatableItem item)
		{
			itemsToAnimate.Remove(item);
		}
		animatable.RestoreCache();
		InvalidateDrivingSpriteCache();
		if (sprite != null)
		{
			GetOrBuildTargets(sprite);
		}
	}

	private Sprite[] GetOrBuildTargets(Sprite drivingSprite)
	{
		if (drivingSprite == currentDrivingSprite)
		{
			return currentTargets;
		}
		currentDrivingSprite = drivingSprite;
		if (!drivingSpriteToTargets.TryGetValue(drivingSprite, out currentTargets))
		{
			currentTargets = BuildTargetsForDrivingSprite(drivingSprite);
			drivingSpriteToTargets[drivingSprite] = currentTargets;
		}
		return currentTargets;
	}

	private Sprite[] BuildTargetsForDrivingSprite(Sprite drivingSprite)
	{
		Sprite[] array = new Sprite[animationsData.Count];
		string idxFromSpriteName = GetIdxFromSpriteName(drivingSprite.name);
		for (int i = 0; i < animationsData.Count; i++)
		{
			string spriteName = animationsData[i].spriteNameWithoutIdx + "_" + idxFromSpriteName;
			array[i] = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(spriteName);
		}
		return array;
	}

	private static string GetIdxFromSpriteName(string sourceName)
	{
		int num = sourceName.LastIndexOf('_');
		if (num < 0)
		{
			return sourceName;
		}
		return sourceName.Substring(num + 1);
	}

	private void InvalidateDrivingSpriteCache()
	{
		drivingSpriteToTargets.Clear();
		currentDrivingSprite = null;
		currentTargets = null;
	}
}
