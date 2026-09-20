using UnityEngine;

public class ConveyorAnimatableSprite : ConveyorAnimatable
{
	public ConveyorSystemAnimatorType targetType;

	public string spriteNameWithoutIdx;

	public GenericSprite sprite;

	public override bool IsValid
	{
		get
		{
			if (sprite != null)
			{
				return !string.IsNullOrEmpty(spriteNameWithoutIdx);
			}
			return false;
		}
	}

	public override ConveyorAnimatableType Type => ConveyorAnimatableType.Sprite;

	protected override void OnEnable()
	{
		if (!IsValid)
		{
			return;
		}
		if (targetType == ConveyorSystemAnimatorType.Cell)
		{
			ConveyorAnimator componentInParent = GetComponentInParent<ConveyorAnimator>();
			if (componentInParent != null)
			{
				componentInParent.AddAnimatable(this);
			}
		}
		else
		{
			Debug.LogError($"ConveyorAnimatableSprite: {targetType} is not supported");
		}
	}

	protected override void OnDisable()
	{
		if (!IsValid)
		{
			return;
		}
		if (targetType == ConveyorSystemAnimatorType.Cell)
		{
			ConveyorAnimator componentInParent = GetComponentInParent<ConveyorAnimator>();
			if (componentInParent != null)
			{
				componentInParent.RemoveAnimatable(this);
			}
		}
		else
		{
			Debug.LogError($"ConveyorAnimatableSprite: {targetType} is not supported");
		}
	}
}
