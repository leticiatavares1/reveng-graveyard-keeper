using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class PerpendicularSpriteUpdateSMB : StateMachineBehaviour
{
	private readonly struct ResolvedSprite
	{
		public readonly bool isValid;

		public readonly bool flipX;

		public readonly Sprite sprite;

		public static readonly ResolvedSprite Skip = new ResolvedSprite(isValid: false, flipX: false, null);

		public ResolvedSprite(bool isValid, bool flipX, Sprite sprite)
		{
			this.isValid = isValid;
			this.flipX = flipX;
			this.sprite = sprite;
		}

		public static ResolvedSprite FlipOnly(bool flipX)
		{
			return new ResolvedSprite(isValid: true, flipX, null);
		}
	}

	private const string STATIC_SUFFIX = "static";

	private const string DOWN = "down";

	private const string LEFT = "left";

	private const string UP = "up";

	[SerializeField]
	private List<SyncedSprite> syncedSprites;

	private string newDirectionName = string.Empty;

	private Direction lastDirection;

	private AnimationComponent animationComponent;

	private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>(64);

	private Dictionary<(int, Direction), ResolvedSprite> resolveCache = new Dictionary<(int, Direction), ResolvedSprite>(128);

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (syncedSprites == null)
		{
			syncedSprites = new List<SyncedSprite>();
		}
		animator.GetComponentsInChildren(syncedSprites);
		if (!animator.TryGetComponent<AnimationComponent>(out animationComponent))
		{
			animationComponent = animator.GetComponentInParent<AnimationComponent>();
		}
		if (animationComponent != null)
		{
			animationComponent.OnLateUpdate -= CustomLateUpdate;
			animationComponent.OnLateUpdate += CustomLateUpdate;
		}
		lastDirection = Direction.None;
		newDirectionName = string.Empty;
		spriteCache.Clear();
		resolveCache.Clear();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animationComponent != null)
		{
			animationComponent.OnLateUpdate -= CustomLateUpdate;
		}
		spriteCache.Clear();
		resolveCache.Clear();
	}

	public void CustomLateUpdate()
	{
		UpdateCachedDirection();
		if (string.IsNullOrEmpty(newDirectionName))
		{
			return;
		}
		foreach (SyncedSprite syncedSprite in syncedSprites)
		{
			if (syncedSprite == null || syncedSprite.SpriteRenderer == null || syncedSprite.targetSprite?.SpriteRenderer?.sprite == null || !syncedSprite.targetSprite.isActiveAndEnabled || syncedSprite.ignoreMe)
			{
				continue;
			}
			if (!syncedSprite.targetSprite.SpriteRenderer.enabled || !syncedSprite.gameObject.activeInHierarchy)
			{
				syncedSprite.SpriteRenderer.enabled = false;
				continue;
			}
			Sprite sprite = syncedSprite.targetSprite.SpriteRenderer.sprite;
			(int, Direction) key = (sprite.GetInstanceID(), lastDirection);
			if (!resolveCache.TryGetValue(key, out var value))
			{
				value = Resolve(sprite.name);
				resolveCache[key] = value;
			}
			if (value.isValid)
			{
				SpriteRenderer spriteRenderer = syncedSprite.SpriteRenderer;
				if (spriteRenderer.flipX != value.flipX)
				{
					spriteRenderer.flipX = value.flipX;
				}
				if (value.sprite != null && spriteRenderer.sprite != value.sprite)
				{
					spriteRenderer.sprite = value.sprite;
				}
			}
		}
	}

	private ResolvedSprite Resolve(string sourceSpriteName)
	{
		string spriteName = GetSpriteName(sourceSpriteName);
		if (!TryParseSpriteName(spriteName, out var parts) || parts.Length < 2)
		{
			return ResolvedSprite.Skip;
		}
		int directionIndex = GetDirectionIndex(spriteName, parts);
		string text = parts[directionIndex];
		if (text == newDirectionName)
		{
			return ResolvedSprite.Skip;
		}
		bool flipX = text == "up";
		parts[directionIndex] = newDirectionName;
		string text2 = string.Join("_", parts);
		if (spriteName == text2)
		{
			return ResolvedSprite.FlipOnly(flipX);
		}
		return new ResolvedSprite(isValid: true, flipX, GetCachedSprite(text2));
	}

	private int GetDirectionIndex(string spriteName, string[] spriteNameParts)
	{
		if (spriteName.Contains("static"))
		{
			if (IsNumericToken(spriteNameParts[^1]) && spriteNameParts.Length >= 2)
			{
				return spriteNameParts.Length - 2;
			}
			return spriteNameParts.Length - 1;
		}
		return spriteNameParts.Length - 2;
	}

	private static bool IsNumericToken(string token)
	{
		if (string.IsNullOrEmpty(token))
		{
			return false;
		}
		for (int i = 0; i < token.Length; i++)
		{
			if (token[i] < '0' || token[i] > '9')
			{
				return false;
			}
		}
		return true;
	}

	private void UpdateCachedDirection()
	{
		Direction direction = animationComponent.Animator.GetFloat(AnimationComponentBase.idDirectionAnimator).ConvertFromSignedAngle();
		if (direction != lastDirection)
		{
			lastDirection = direction;
			newDirectionName = GetDirectionNameForEnumDirection(direction);
		}
	}

	private string GetDirectionNameForEnumDirection(Direction direction)
	{
		switch (direction)
		{
		case Direction.Right:
		case Direction.Left:
			return "down";
		case Direction.Up:
		case Direction.Down:
			return "left";
		default:
			return string.Empty;
		}
	}

	private string GetSpriteName(string originalName)
	{
		bool num = originalName.IndexOf("(Clone)", StringComparison.Ordinal) >= 0;
		bool flag = originalName.Length > 0 && (char.IsWhiteSpace(originalName[0]) || char.IsWhiteSpace(originalName[originalName.Length - 1]));
		if (!num && !flag)
		{
			return originalName;
		}
		return originalName.Replace("(Clone)", "").Trim();
	}

	private Sprite GetCachedSprite(string spriteName)
	{
		if (spriteCache.TryGetValue(spriteName, out var value))
		{
			return value;
		}
		Sprite sprite = null;
		if (LazySingletonSO<EasySpritesCollection>.Instance.HasSprite(spriteName))
		{
			sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(spriteName);
		}
		else if (spriteName.Contains("static"))
		{
			string[] array = spriteName.Split('_');
			if (array.Length >= 2 && IsNumericToken(array[^1]))
			{
				string spriteName2 = string.Join("_", array, 0, array.Length - 1);
				if (LazySingletonSO<EasySpritesCollection>.Instance.HasSprite(spriteName2))
				{
					sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(spriteName2);
				}
			}
		}
		spriteCache[spriteName] = sprite;
		return sprite;
	}

	private bool TryParseSpriteName(string spriteName, out string[] parts)
	{
		parts = spriteName.Split('_');
		return parts.Length >= 2;
	}
}
