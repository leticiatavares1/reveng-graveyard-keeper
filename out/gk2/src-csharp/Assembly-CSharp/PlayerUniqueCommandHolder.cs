using UnityEngine;

public class PlayerUniqueCommandHolder : UniqueCommandHolderDataWrapper<NetworkPlayer>
{
	private NetworkPlayer data;

	private Vector3 position;

	private Vector2 direction;

	private AnimationState animState;

	public override void RegisterData(NetworkPlayer data)
	{
		this.data = data;
		data.playerData.position.ValueChanged += UpdatePosition;
		data.playerData.AddDirectionListener(UpdateDirection);
		data.playerData.charState.ValueChanged += UpdateAnimState;
	}

	public override void UnregisterData()
	{
		data.playerData.position.ValueChanged -= UpdatePosition;
		data.playerData.RemoveDirectionListener(UpdateDirection);
		data.playerData.charState.ValueChanged -= UpdateAnimState;
		data = null;
	}

	public override Command GetCommand()
	{
		return new CharMoveCommand
		{
			playerId = data.clientId,
			position = position,
			direction = direction,
			animState = animState
		};
	}

	private void UpdatePosition(Vector3 position)
	{
		this.position = position;
		PrepareDataForSending();
	}

	private void UpdateDirection(Vector2 direction)
	{
		this.direction = direction;
		PrepareDataForSending();
	}

	private void UpdateAnimState(AnimationState animState)
	{
		this.animState = animState;
		PrepareDataForSending();
	}

	private void PrepareDataForSending()
	{
		LazyNetwork.ConnectionManager.CurrentState.SendNetworkData(this);
	}
}
