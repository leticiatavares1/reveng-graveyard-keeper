using System;
using UnityEngine;

[Serializable]
[Command]
public class CharMoveCommand : Command
{
	[CommandField]
	public int playerId;

	[CommandField]
	public Vector3 position;

	[CommandField]
	public Vector2 direction;

	[CommandField]
	public AnimationState animState;

	public override void Execute(ulong senderClientId, GameSave gameSave)
	{
		if (playerId != (int)LazyNetwork.NetworkManager.MyId)
		{
			gameSave.GetClient((int)senderClientId, out var clientPlayer, considerHost: true);
			clientPlayer.playerData.position.Value = position;
			clientPlayer.playerData.Direction = direction;
			clientPlayer.playerData.charState.Value = animState;
		}
	}
}
