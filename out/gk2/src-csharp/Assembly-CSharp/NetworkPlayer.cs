using System;
using UnityEngine;

[Serializable]
public class NetworkPlayer
{
	public PlayerData playerData;

	public int clientId;

	public NetworkPlayer()
	{
	}

	public NetworkPlayer(int clientId, PlayerData playerData)
	{
		this.playerData = playerData;
		this.clientId = clientId;
	}

	public void SubscribeToPlayerDataChanges(PlayerPhysicalBody playerBody)
	{
		playerData.position.ValueChanged += delegate(Vector3 newPosition)
		{
			playerBody.MoveByPosition(newPosition, playerData.Direction);
		};
		playerData.AddDirectionListener(delegate(Vector2 direction)
		{
			playerBody.PlayerView.PlayerAnimation.SetDirection(direction);
		});
		playerData.charState.ValueChanged += delegate(AnimationState state)
		{
			playerBody.PlayerView.PlayerAnimation.SetState(state);
		};
	}
}
