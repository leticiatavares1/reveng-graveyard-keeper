using UnityEngine;

public class ConveyorAnimatableItem : ConveyorAnimatable
{
	public WgoPart parentWgoPart;

	private Vector3 cachedPosition;

	private Vector3 cachedRotation;

	private Vector3 cachedScale;

	public override ConveyorAnimatableType Type => ConveyorAnimatableType.Item;

	public override void CreateCache()
	{
		cachedPosition = base.transform.localPosition;
		cachedRotation = base.transform.localEulerAngles;
		cachedScale = base.transform.localScale;
	}

	public override void RestoreCache()
	{
		base.transform.localPosition = cachedPosition;
		base.transform.localEulerAngles = cachedRotation;
		base.transform.localScale = cachedScale;
	}

	protected override void OnEnable()
	{
		if (!(parentWgoPart == null))
		{
			ConveyorAnimator componentInParent = GetComponentInParent<ConveyorAnimator>();
			if (componentInParent != null)
			{
				componentInParent.AddAnimatable(this);
			}
			CreateCache();
		}
	}

	protected override void OnDisable()
	{
		if (!(parentWgoPart == null))
		{
			ConveyorAnimator componentInParent = GetComponentInParent<ConveyorAnimator>();
			if (componentInParent != null)
			{
				componentInParent.RemoveAnimatable(this);
			}
			RestoreCache();
		}
	}

	private void Awake()
	{
		if (parentWgoPart == null)
		{
			parentWgoPart = GetComponentInParent<WgoPart>();
		}
	}
}
