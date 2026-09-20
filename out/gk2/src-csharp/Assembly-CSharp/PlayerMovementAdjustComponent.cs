using UnityEngine;

public class PlayerMovementAdjustComponent : MovementAdjustComponentBase
{
	private PlayerController player;

	public override void Init(IMovable movable)
	{
		player = movable as PlayerController;
		if (player == null)
		{
			Debug.LogError("Wrong IMovable type");
		}
		else
		{
			player.PlayerData.position.ValueChanged += UpdatePosIfMoving;
		}
	}

	public override void DeInit()
	{
		if (player != null)
		{
			player.PlayerData.position.ValueChanged -= UpdatePosIfMoving;
		}
		else
		{
			Debug.LogWarning("MovementAdjustComponent: Data is null");
		}
	}

	protected override void UpdatePosIfMoving(Vector3 newPos)
	{
		if (player != null && player.MovementComponent.IsMoving)
		{
			UpdatePos(newPos);
		}
	}

	private void OnDestroy()
	{
		DeInit();
	}
}
