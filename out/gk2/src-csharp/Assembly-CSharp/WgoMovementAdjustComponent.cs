using UnityEngine;

public class WgoMovementAdjustComponent : MovementAdjustComponentBase
{
	private WgoData wgoData;

	public override void Init(IMovable movable)
	{
		wgoData = movable as WgoData;
		if (wgoData == null)
		{
			Debug.LogError("Wrong IMovable type");
		}
		else
		{
			wgoData.OnPositionChanged += UpdatePosIfMoving;
		}
	}

	public override void DeInit()
	{
		if (wgoData != null)
		{
			wgoData.OnPositionChanged -= UpdatePosIfMoving;
		}
		else
		{
			Debug.LogWarning("MovementAdjustComponent: Data is null");
		}
	}

	public override void SetAdjustmentActive(bool active)
	{
		if (active == base.enabled)
		{
			return;
		}
		if (!active)
		{
			if (wgoData != null)
			{
				wgoData.OnPositionChanged -= UpdatePosIfMoving;
			}
		}
		else if (wgoData != null)
		{
			wgoData.OnPositionChanged += UpdatePosIfMoving;
		}
		base.enabled = active;
	}

	protected override void UpdatePosIfMoving(Vector3 newPos)
	{
		if (wgoData != null && wgoData.MovementComponent.IsMoving)
		{
			UpdatePos(newPos);
		}
	}

	private void OnDestroy()
	{
		DeInit();
	}
}
